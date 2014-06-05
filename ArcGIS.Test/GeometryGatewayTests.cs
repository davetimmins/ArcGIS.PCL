using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel.Operation;
using ArcGIS.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ArcGIS.ServiceModel.Serializers;

namespace ArcGIS.Test
{
    public class GeometryGatewayTests
    {
        [Fact]
        public async Task CanProject()
        {
            var gateway = new GeometryGateway(new ServiceStackSerializer(), @"http://services.arcgisonline.co.nz/arcgis");
            var result = await gateway.Query<Polygon>(new Query("STATS/territorialauthorities/MapServer/0".AsEndpoint()) { Where = "NAME = 'Hamilton City'" });

            var features = result.Features.Where(f => f.Geometry.Rings.Any()).ToList();

            Assert.NotNull(features);
            Assert.True(result.SpatialReference.Wkid != SpatialReference.WGS84.Wkid);
            Assert.True(features[0].Geometry.Rings.Count > 0);
            features[0].Geometry.SpatialReference = result.SpatialReference;

            var projectedFeatures = await gateway.Project<Polygon>(features, SpatialReference.WGS84);

            Assert.NotNull(projectedFeatures);
            Assert.Equal(features.Count, projectedFeatures.Count);

            Assert.True(features[0].Geometry.Rings.Count > 0);  // If this fails, 2 issues: 1) features has been shallow copied, and 2) geometries aren't being populated.
            Assert.True(projectedFeatures[0].Geometry.Rings.Count > 0); // If this fails, just problem 2 above - geometries aren't being copied.
            Assert.NotEqual(features, projectedFeatures);
            Assert.NotEqual(projectedFeatures[0].Geometry.Rings[0], features[0].Geometry.Rings[0]);
        }

        [Fact]
        public async Task CanBuffer()
        {            
            var gateway = new GeometryGateway(new ServiceStackSerializer());

            var result = await gateway.Query<Polygon>(new Query("MontgomeryQuarters/MapServer/0/".AsEndpoint()));

            var features = result.Features.Where(f => f.Geometry.Rings.Any()).ToList();
            Assert.NotNull(features);

            await Buffer(new ArcGISOnlineGateway(new ServiceStackSerializer()), features, result.SpatialReference);
            await Buffer(gateway, features, result.SpatialReference);
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

        [Fact]
        public async Task CanSimplify()
        {
            var gateway = new GeometryGateway(new ServiceStackSerializer(), @"http://services.arcgisonline.co.nz/arcgis");
            var result = await gateway.Query<Polygon>(new Query("STATS/territorialauthorities/MapServer/0".AsEndpoint()) { Where = "NAME = 'Hamilton City'" });

            var features = result.Features.Where(f => f.Geometry.Rings.Any()).ToList();

            Assert.NotNull(features);
            Assert.True(result.SpatialReference.Wkid != SpatialReference.WGS84.Wkid);
            Assert.True(features[0].Geometry.Rings.Count > 0);
            features[0].Geometry.SpatialReference = result.SpatialReference;

            var simplifiedFeatures = await gateway.Simplify<Polygon>(features, result.SpatialReference);

            Assert.NotNull(simplifiedFeatures);
            Assert.Equal(features.Count, simplifiedFeatures.Count);

            Assert.True(features[0].Geometry.Rings.Count > 0); 
            Assert.True(simplifiedFeatures[0].Geometry.Rings.Count > 0); 
            Assert.NotEqual(features, simplifiedFeatures);
            Assert.NotEqual(simplifiedFeatures[0].Geometry.Rings[0], features[0].Geometry.Rings[0]);
        }
    }

    public class GeometryGateway : PortalGateway
    {
        public GeometryGateway(ISerializer serializer, String baseUrl = @"http://sampleserver6.arcgisonline.com/arcgis")
            : base(baseUrl, serializer, null)
        { }
    }
}
