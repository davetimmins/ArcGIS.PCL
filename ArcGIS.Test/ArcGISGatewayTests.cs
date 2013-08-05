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
using Xunit;
using ServiceStack.Text;

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
            Serializer = new ServiceStackSerializer();
        }

        public Task<QueryResponse<T>> Query<T>(Query queryOptions) where T : IGeometry
        {
            return Post<QueryResponse<T>, Query>(queryOptions);
        }

        public Task<QueryResponse<T>> QueryAsGet<T>(Query queryOptions) where T : IGeometry
        {
            return Get<QueryResponse<T>, Query>(queryOptions);
        }

        public Task<ApplyEditsResponse> ApplyEdits<T>(ApplyEdits<T> edits) where T : IGeometry
        {
            return Post<ApplyEditsResponse, ApplyEdits<T>>(edits);
        }

        public Task<AgsObject> GetAnything(ArcGISServerEndpoint endpoint)
        {
            return Get<AgsObject>(endpoint);
        }
    }

    public class AgsObject : JsonObject, IPortalResponse
    {
        [System.Runtime.Serialization.DataMember(Name = "error")]
        public ArcGISError Error { get; set; }
    }

    public class ArcGISGatewayTests
    {
        [Fact]
        public async Task CanGetAnythingFromServer()
        {
            var gateway = new ArcGISGateway();

            var endpoint = new ArcGISServerEndpoint("/Earthquakes/EarthquakesFromLastSevenDays/MapServer");

            var response = await gateway.GetAnything(endpoint);

            Assert.Null(response.Error);
            Assert.True(response.ContainsKey("capabilities"));
            Assert.True(response.ContainsKey("mapName"));
            Assert.True(response.ContainsKey("layers"));
            Assert.True(response.ContainsKey("documentInfo"));
        }

        [Fact]
        public async Task CanPingServer()
        {
            var gateway = new ArcGISGateway();

            var endpoint = new ArcGISServerEndpoint("/");

            var response = await gateway.Ping(endpoint);

            Assert.Null(response.Error);
        }

        [Fact]
        public async Task CanDescribeSite()
        {
            var gateway = new ArcGISGateway();

            var response = await gateway.DescribeSite();

            Assert.NotNull(response);
            Assert.True(response.Version > 0);

            foreach (var resource in response.Resources)
            {
                var ping = await gateway.Ping(resource);
                Assert.Null(ping.Error);
            }
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

            var exception = await SecureGISGatewayTests.ThrowsAsync<InvalidOperationException>(async () => await gateway.QueryAsGet<Point>(query));
            Assert.NotNull(exception);
            Assert.Contains("Unable to complete Query operation", exception.Message);
        }

        [Fact]
        public async Task QueryCanReturnFeatures()
        {
            var gateway = new ArcGISGateway();

            var query = new Query(@"/Earthquakes/EarthquakesFromLastSevenDays/MapServer/0".AsEndpoint());
            var result = await gateway.Query<Point>(query);

            Assert.NotNull(result);
            Assert.Null(result.Error);
            Assert.NotNull(result.SpatialReference);
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

            gateway.Serializer = new JsonDotNetSerializer();

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
            gateway.Serializer = new JsonDotNetSerializer();

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

        /// <summary>
        /// Performs unfiltered query, then filters by Extent and Polygon to SE quadrant of globe and verifies both filtered
        /// results contain same number of features as each other, and that both filtered resultsets contain fewer features than unfiltered resultset.
        /// </summary>
        /// <param name="serviceUrl"></param>
        /// <returns></returns>
        public async Task QueryGeometryCriteriaHonored(string serviceUrl)
        {
            int countAllResults = 0;
            int countExtentResults = 0;
            int countPolygonResults = 0;

            var gateway = new ArcGISGateway();
            gateway.Serializer = new JsonDotNetSerializer();

            var queryPointAllResults = new Query(serviceUrl.AsEndpoint());

            var resultPointAllResults = await gateway.QueryAsGet<Point>(queryPointAllResults);

            var queryPointExtentResults = new Query(serviceUrl.AsEndpoint())
            {
                Geometry = new Extent { XMin = 0, YMin = 0, XMax = 180, YMax = -90, SpatialReference = SpatialReference.WGS84 }, // SE quarter of globe
                OutputSpatialReference = SpatialReference.WebMercator
            };
            var resultPointExtentResults = await gateway.QueryAsGet<Point>(queryPointExtentResults);

            PointCollectionList rings = new PointCollectionList();
            PointCollection pointCollection = new PointCollection();
            pointCollection.Add(new double[] { 0, 0 });
            pointCollection.Add(new double[] { 180, 0 });
            pointCollection.Add(new double[] { 180, -90 });
            pointCollection.Add(new double[] { 0, -90 });
            pointCollection.Add(new double[] { 0, 0 });
            rings.Add(pointCollection);

            var queryPointPolygonResults = new Query(serviceUrl.AsEndpoint())
            {
                Geometry = new Polygon { Rings = rings }
            };
            var resultPointPolygonResults = await gateway.QueryAsGet<Point>(queryPointPolygonResults);

            countAllResults = resultPointAllResults.Features.Count();
            countExtentResults = resultPointExtentResults.Features.Count();
            countPolygonResults = resultPointPolygonResults.Features.Count();

            Assert.Equal(resultPointExtentResults.SpatialReference.Wkid, queryPointExtentResults.OutputSpatialReference.LatestWkid);
            Assert.True(countAllResults > countExtentResults);
            Assert.True(countPolygonResults == countExtentResults);
        }

        /// <summary>
        /// Test geometry query against MapServer
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task QueryMapServerGeometryCriteriaHonored()
        {
            await QueryGeometryCriteriaHonored(@"/Earthquakes/EarthquakesFromLastSevenDays/MapServer/0");
        }

        /// <summary>
        /// Test geometry query against FeatureServer
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task QueryFeatureServerGeometryCriteriaHonored()
        {
            await QueryGeometryCriteriaHonored(@"/Earthquakes/EarthquakesFromLastSevenDays/FeatureServer/0");
        }

        [Fact]
        public async Task CanAddUpdateAndDelete()
        {
            var gateway = new ArcGISGateway();

            var feature = new Feature<Point>();
            feature.Attributes.Add("type", 0);
            feature.Geometry = new Point { SpatialReference = new SpatialReference { Wkid = SpatialReference.WebMercator.Wkid }, X = -13073617.8735768, Y = 4071422.42978062 };

            var adds = new ApplyEdits<Point>(@"Fire/Sheep/FeatureServer/0".AsEndpoint())
            {
                Adds = new List<Feature<Point>> { feature }
            };
            var resultAdd = await gateway.ApplyEdits(adds);

            Assert.True(resultAdd.Adds.Any());
            Assert.True(resultAdd.Adds.First().Success);

            var id = resultAdd.Adds.First().ObjectId;

            feature.Attributes.Add("description", "'something'"); // problem with serialization means we need single quotes around string values
            feature.Attributes.Add("objectId", id);

            var updates = new ApplyEdits<Point>(@"Fire/Sheep/FeatureServer/0".AsEndpoint())
            {
                Updates = new List<Feature<Point>> { feature }
            };
            var resultUpdate = await gateway.ApplyEdits(updates);

            Assert.True(resultUpdate.Updates.Any());
            Assert.True(resultUpdate.Updates.First().Success);
            Assert.Equal(resultUpdate.Updates.First().ObjectId, id);

            var deletes = new ApplyEdits<Point>(@"Fire/Sheep/FeatureServer/0".AsEndpoint())
            {
                Deletes = new List<int> { id }
            };
            var resultDelete = await gateway.ApplyEdits(deletes);

            Assert.True(resultDelete.Deletes.Any());
            Assert.True(resultDelete.Deletes.First().Success);
            Assert.Equal(resultDelete.Deletes.First().ObjectId, id);
        }
    }
}
