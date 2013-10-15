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
            
            // Take a copy now, as gateway.Buffer oprration below will update features collection.
            int featuresCount = features.Count;
            List<int> pointsCount = new List<int>();
            foreach (Feature<Polygon> feature in features)
            {
                pointsCount.Add(feature.Geometry.Rings[0].Points.Count);
            } 


            double distance = 0.01;
            var featuresBuffered = await gateway.Buffer<Polygon>(features, result.SpatialReference, distance);

            int featuresBufferedCount = featuresBuffered.Count;
            List<int> pointsBufferedCount = new List<int>();
            foreach (Feature<Polygon> featureBuffered in featuresBuffered)
            {
                pointsBufferedCount.Add(featureBuffered.Geometry.Rings[0].Points.Count);
            } 


            Assert.NotNull(featuresBuffered);
            Assert.Equal(featuresCount, featuresBufferedCount);
            Assert.True(pointsBufferedCount.Count == pointsCount.Count);
            for (int indexPointsCount = 0; indexPointsCount < pointsBufferedCount.Count; ++indexPointsCount)
            {
                Assert.True(pointsBufferedCount[indexPointsCount] > pointsCount[indexPointsCount], "Expecting buffered polygon to contain more points than original");       // Possible more complex shape, so equal or greater than number of points.
                Assert.True(featuresBuffered[indexPointsCount].Geometry.Rings[0].Points.Count > features[indexPointsCount].Geometry.Rings[0].Points.Count, "Expecting buffered polygon to contain more points than original");       // Possible more complex shape, so equal or greater than number of points.
            }

            /*
             * This code doesn't work, as features and featuresBuffered point to the same object.
            for (int indexFeature = 0; indexFeature < features.Count; ++indexFeature)
            {
                Feature<Polygon> featureOriginal = features[indexFeature];
                Feature<Polygon> featureBuffered = featuresBuffered[indexFeature];
                Assert.Equal(featureOriginal.Geometry.Rings.Count, featureBuffered.Geometry.Rings.Count);

                for (int indexRing = 0; indexRing < featureOriginal.Geometry.Rings.Count; ++indexRing)
                {
                    PointCollection pointCollectionOriginal = featureOriginal.Geometry.Rings[indexRing];
                    PointCollection pointCollectionBuffered = featureBuffered.Geometry.Rings[indexRing];
                    Assert.Equal(pointCollectionOriginal.Count, pointCollectionBuffered.Count);

                    for (int indexPoint = 0; indexPoint < pointCollectionOriginal.Count; ++indexPoint)
                    {
                        double[] pointOriginal = pointCollectionOriginal[indexPoint];
                        double[] pointBuffered = pointCollectionBuffered[indexPoint];

                        Assert.Equal(pointOriginal.Count(), 2);
                        Assert.Equal(pointOriginal.Count(), pointBuffered.Count());

                        Assert.True(Math.Abs(pointOriginal[0] - pointBuffered[0]) < distance);
                        Assert.True(Math.Abs(pointOriginal[1] - pointBuffered[1]) < distance);
                        Assert.True(Math.Abs(pointOriginal[0] - pointBuffered[0]) > 0);
                        Assert.True(Math.Abs(pointOriginal[1] - pointBuffered[1]) > 0);
                    }
                }
            }
             * */
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
            var op = new BufferGeometry<T>("/Utilities/Geometry/GeometryServer".AsEndpoint(), features, inputSpatialReference, distance);
            var buffered = await Post<GeometryOperationResponse<T>, BufferGeometry<T>>(op);
            for (int i = 0; i < buffered.Geometries.Count; i++)
                features[i].Geometry = buffered.Geometries[i];

            return features;
        }
    }
}
