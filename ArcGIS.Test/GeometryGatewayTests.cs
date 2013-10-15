using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel.Operation;
using ArcGIS.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ArcGIS.Test
{
    public class GeometryGatewayTests
    {
        [Fact]
        public async Task CanProject()
        {
            var gateway = new GeometryGateway(new ServiceStackSerializer());
            var result = await gateway.Query<Polyline>(new Query("Hurricanes/MapServer/1".AsEndpoint()));

            var features = result.Features.Where(f => f.Geometry.Paths.Any()).ToList();

            Assert.NotNull(features);
            Assert.True(result.SpatialReference.Wkid != SpatialReference.WGS84.Wkid);

            if (result.SpatialReference.Wkid != SpatialReference.WGS84.Wkid)
            {
                var projectedFeatures = await gateway.Project<Polyline>(features, result.SpatialReference);

                Assert.NotNull(projectedFeatures);
                Assert.Equal(features.Count, projectedFeatures.Count);
            }
        }

        [Fact]
        public async Task CanBuffer()
        {
            var gateway = new GeometryGateway(new ServiceStackSerializer());
            var result = await gateway.Query<Polygon>(new Query("MontgomeryQuarters/MapServer/0/".AsEndpoint()));

            var features = result.Features.Where(f => f.Geometry.Rings.Any()).ToList();
            Assert.NotNull(features);
            
            int featuresCount = features.Count;

            double distance = 10.0;
            var featuresBuffered = await gateway.Buffer<Polygon>(features, result.SpatialReference, distance);

            Assert.NotNull(featuresBuffered);
            Assert.Equal(featuresCount, featuresBuffered.Count);
            for (int indexFeature = 0; indexFeature < featuresCount; ++indexFeature)
            {
                // Should be more complex shape, so expect greater number of points.
                Assert.True(featuresBuffered[indexFeature].Geometry.Rings[0].Points.Count > features[indexFeature].Geometry.Rings[0].Points.Count, "Expecting buffered polygon to contain more points than original");
            }
        }
    }

    public class GeometryGateway : PortalGateway
    {
        public GeometryGateway(ISerializer serializer)
            : base(@"http://sampleserver6.arcgisonline.com/arcgis", serializer, null)
        { }

        public async Task<QueryResponse<T>> Query<T>(Query queryOptions) where T : IGeometry
        {
            return await Get<QueryResponse<T>, Query>(queryOptions);
        }

        public async Task<List<Feature<T>>> Project<T>(List<Feature<T>> features, SpatialReference outputSpatialReference) where T : IGeometry
        {
            var op = new ProjectGeometry<T>("/Utilities/Geometry/GeometryServer".AsEndpoint(), features, outputSpatialReference);
            var projected = await Post<GeometryOperationResponse<T>, ProjectGeometry<T>>(op);
            for (int i = 0; i < projected.Geometries.Count; i++)
                features[i].Geometry = projected.Geometries[i];

            return features;
        }

        public async Task<List<Feature<T>>> Buffer<T>(List<Feature<T>> features, SpatialReference inputSpatialReference, double distance) where T : IGeometry
        {
            // Want to ensure that we don't get a shallow copy
            string jsonFeatures = Newtonsoft.Json.JsonConvert.SerializeObject(features);
            List<Feature<T>> deepCopy = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Feature<T>>>(jsonFeatures);

            var op = new BufferGeometry<T>("/Utilities/Geometry/GeometryServer".AsEndpoint(), deepCopy, inputSpatialReference, distance);
            var buffered = await Post<GeometryOperationResponse<T>, BufferGeometry<T>>(op);
            for (int i = 0; i < buffered.Geometries.Count; i++)
                deepCopy[i].Geometry = buffered.Geometries[i];

            return deepCopy;
        }
    }
}
