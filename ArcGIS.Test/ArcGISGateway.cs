using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel.Extensions;
using ArcGIS.ServiceModel.Logic;
using ArcGIS.ServiceModel.Operation;
using ServiceStack.Text;
using Xunit;

namespace ArcGIS.Test
{
    public class ArcGISGateway : PortalGateway
    {
        public ArcGISGateway()
            : this(@"http://sampleserver3.arcgisonline.com/ArcGIS/", String.Empty, String.Empty)
        { }

        public ArcGISGateway(String root, String username, String password)
            : base(root, username, password)
        {
            Serializer = new Serializer();
        }

        public async Task<QueryResponse<T>> Query<T>(Query queryOptions) where T : IGeometry
        {
            var result = await Post<QueryResponse<T>, Query>(queryOptions, queryOptions);
            return result;
        }

        public async Task<QueryResponse<T>> QueryAsGet<T>(Query queryOptions) where T : IGeometry
        {
            var result = await Get<QueryResponse<T>, Query>(queryOptions, queryOptions);
            return result;
        }
    }

    public class Serializer : ISerializer
    {
        public Dictionary<String, String> AsDictionary<T>(T objectToConvert) where T : CommonParameters
        {
            return objectToConvert == null ?
                null :
                JsonSerializer.DeserializeFromString<Dictionary<String, String>>(JsonSerializer.SerializeToString(objectToConvert));
        }

        public T AsPortalResponse<T>(String dataToConvert) where T : PortalResponse
        {
            return String.IsNullOrWhiteSpace(dataToConvert) ?
                null :
                JsonSerializer.DeserializeFromString<T>(dataToConvert);
        }
    }

    public class ArcGISGatewayTests
    {
        [Fact]
        public async Task CanPingServer()
        {
            var gateway = new ArcGISGateway();

            var endpoint = new ArcGISServerEndpoint("/");

            var response = await gateway.Ping(endpoint);

            Assert.Null(response.Error);
        }

        [Fact]
        public void RootUrlHasCorrectFormat()
        {
            var gateway = new ArcGISGateway();
            Assert.True(gateway.RootUrl.EndsWith("/"));
            Assert.True(gateway.RootUrl.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) || gateway.RootUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase));
            Assert.False(gateway.RootUrl.ToLowerInvariant().Contains("/rest/services/"));
        }

        [Fact]
        public async Task GatewayDoesAutoPost()
        {
            var gateway = new ArcGISGateway();

            var longWhere = new StringBuilder("region = '");
            for (var i = 0; i < 3000; i++)
                longWhere.Append(i);
            
            var query = new Query(@"/Earthquakes/EarthquakesFromLastSevenDays/MapServer/0".AsEndpoint()) { Where = longWhere + "'" };

            try
            {
                await gateway.QueryAsGet<Point>(query);
            }
            catch (HttpRequestException)
            {
                Assert.False(true);
                return;
            }
            catch (InvalidOperationException)
            {
                Assert.True(true);
                return;
            }

            Assert.False(true);
        }

        [Fact]
        public async Task QueryCanReturnFeatures()
        {
            var gateway = new ArcGISGateway();

            var query = new Query(@"/Earthquakes/EarthquakesFromLastSevenDays/MapServer/0".AsEndpoint());
            var result = await gateway.Query<Point>(query);

            Assert.True(result.Features.Any());
        }

        [Fact]
        public async Task QueryCanReturnDifferentGeometryTypes()
        {
            var gateway = new ArcGISGateway();

            var queryPoint = new Query(@"Earthquakes/EarthquakesFromLastSevenDays/MapServer/0".AsEndpoint());
            var resultPoint = await gateway.Query<Point>(queryPoint);

            Assert.True(resultPoint.Features.Any());
            Assert.True(resultPoint.Features.All(i => i.Geometry != null));

            var queryPolyline = new Query(@"Hydrography/Watershed173811/MapServer/1".AsEndpoint()) { OutFields = "lengthkm" };
            var resultPolyline = await gateway.QueryAsGet<Polyline>(queryPolyline);

            Assert.True(resultPolyline.Features.Any());
            Assert.True(resultPolyline.Features.All(i => i.Geometry != null));

            var queryPolygon = new Query(@"/Hydrography/Watershed173811/MapServer/0".AsEndpoint()) { Where = "areasqkm = 0.012", OutFields = "areasqkm" };
            var resultPolygon = await gateway.Query<Polygon>(queryPolygon);

            Assert.True(resultPolygon.Features.Any());
            Assert.True(resultPolygon.Features.All(i => i.Geometry != null));
        }

        [Fact]
        public async Task QueryCanReturnNoGeometry()
        {
            var gateway = new ArcGISGateway();

            var queryPoint = new Query(@"Earthquakes/EarthquakesFromLastSevenDays/MapServer/0".AsEndpoint()) { ReturnGeometry = false };
            var resultPoint = await gateway.QueryAsGet<Point>(queryPoint);

            Assert.True(resultPoint.Features.Any());
            Assert.True(resultPoint.Features.All(i => i.Geometry == null));

            var queryPolyline = new Query(@"Hydrography/Watershed173811/MapServer/1".AsEndpoint()) { OutFields = "lengthkm", ReturnGeometry = false };
            var resultPolyline = await gateway.Query<Polyline>(queryPolyline);

            Assert.True(resultPolyline.Features.Any());
            Assert.True(resultPolyline.Features.All(i => i.Geometry == null));
        }

        [Fact]
        public async Task QueryOutFieldsAreHonored()
        {
            var gateway = new ArcGISGateway();

            var queryPolyline = new Query(@"Hydrography/Watershed173811/MapServer/1".AsEndpoint()) { OutFields = "lengthkm", ReturnGeometry = false };
            var resultPolyline = await gateway.Query<Polyline>(queryPolyline);

            Assert.True(resultPolyline.Features.Any());
            Assert.True(resultPolyline.Features.All(i => i.Geometry == null));
            Assert.True(resultPolyline.Features.All(i => i.Attributes != null && i.Attributes.Count == 1));

            var queryPolygon = new Query(@"/Hydrography/Watershed173811/MapServer/0".AsEndpoint())
                {
                    Where = "areasqkm = 0.012",
                    OutFields = "areasqkm,elevation,resolution,reachcode"
                };
            var resultPolygon = await gateway.QueryAsGet<Polygon>(queryPolygon);

            Assert.True(resultPolygon.Features.Any());
            Assert.True(resultPolygon.Features.All(i => i.Geometry != null));
            Assert.True(resultPolygon.Features.All(i => i.Attributes != null && i.Attributes.Count == 4));
        }
    }
}
