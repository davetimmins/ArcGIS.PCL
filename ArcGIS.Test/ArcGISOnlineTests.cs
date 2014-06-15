using ArcGIS.ServiceModel;
using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel.Operation;
using ArcGIS.ServiceModel.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ArcGIS.Test
{
    public class ArcGISOnlineTests
    {
        [Fact]
        public async Task CanBuffer()
        {
            var gateway = new GeometryGateway(new ServiceStackSerializer());
            var result = await gateway.Query<Polygon>(new Query("MontgomeryQuarters/MapServer/0/".AsEndpoint()));

            var features = result.Features.Where(f => f.Geometry.Rings.Any()).ToList();
            Assert.NotNull(features);

            await Buffer(new ArcGISOnlineGateway(new ServiceStackSerializer()), features, result.SpatialReference);
        }

        async Task Buffer(PortalGatewayBase gateway, List<Feature<Polygon>> features, SpatialReference spatialReference)
        {
            int featuresCount = features.Count;

            double distance = 10.0;
            var featuresBuffered = await gateway.Buffer<Polygon>(features, spatialReference, distance);

            Assert.NotNull(featuresBuffered);
            Assert.Equal(featuresCount, featuresBuffered.Count);
            for (int indexFeature = 0; indexFeature < featuresCount; ++indexFeature)
            {
                // Should be more complex shape, so expect greater number of points.
                Assert.True(featuresBuffered[indexFeature].Geometry.Rings[0].Points.Count > features[indexFeature].Geometry.Rings[0].Points.Count, "Expecting buffered polygon to contain more points than original");
            }
        }

        //[Fact]
        //public async Task CanSearchForHostedFeatureServices()
        //{
        //    var serializer = new ServiceStackSerializer();
        //    var gateway = new ArcGISOnlineGateway(serializer, new ArcGISOnlineTokenProvider("", "", serializer));

        //    var hostedServices = await gateway.DescribeSite();

        //    Assert.NotNull(hostedServices);
        //    Assert.Null(hostedServices.Error);
        //    Assert.NotNull(hostedServices.Results);
        //    Assert.NotEmpty(hostedServices.Results);
        //    Assert.False(hostedServices.Results.All(s => String.IsNullOrWhiteSpace(s.Id)));
        //}
        
        //[Fact]
        //public async Task OAuthTokenCanBeGenerated()
        //{
        //    // Set your client Id and secret here
        //    var tokenProvider = new ArcGISOnlineAppLoginOAuthProvider("", "", _serviceStackSerializer);

        //    var token = await tokenProvider.CheckGenerateToken();

        //    Assert.NotNull(token);
        //    Assert.NotNull(token.Value);
        //    Assert.False(token.IsExpired);
        //    Assert.Null(token.Error);
        //}
    }
}
