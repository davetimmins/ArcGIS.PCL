using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel.Logic;
using ArcGIS.ServiceModel.Operation;
using ArcGIS.ServiceModel.Extensions;
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
            var gateway = new GeometryGateway();
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
    }

    public class GeometryGateway : PortalGateway
    {
        public GeometryGateway()
            : base(@"http://sampleserver6.arcgisonline.com/arcgis")
        {
            Serializer = new ServiceStackSerializer();
        }

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
    }
}
