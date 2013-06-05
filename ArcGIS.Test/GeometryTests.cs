using ArcGIS.ServiceModel.Common;
using Xunit;

namespace ArcGIS.Test
{
    public class GeometryTests
    {
        [Fact]
        public void FeaturesAreSame()
        {
            var feature1 = new Feature<Point>
            {
                Geometry = new Point { X = 50.342, Y = -30.331, SpatialReference = SpatialReference.WGS84 }
            };
            feature1.Attributes.Add("random", "rtcxbvbx");
            feature1.Attributes.Add("something", 45445);

            var feature2 = new Feature<Point>
            {
                Geometry = new Point { X = 50.342, Y = -30.331, SpatialReference = SpatialReference.WGS84 }
            };
            feature2.Attributes.Add("random", "rtcxbvbx");
            feature2.Attributes.Add("something", 45445);

            Assert.Equal(feature1, feature2);
        }

        [Fact]
        public void FeaturesAreNotSame()
        {
            var feature1 = new Feature<Point>
            {
                Geometry = new Point { X = 50.342, Y = -2, SpatialReference = SpatialReference.WGS84 }
            };

            var feature2 = new Feature<Point>
            {
                Geometry = new Point { X = 50.342, Y = -30.331, SpatialReference = SpatialReference.WGS84 }
            };

            var feature3 = new Feature<Point>
            {
                Geometry = new Point { X = 50.342, Y = -30.331, SpatialReference = SpatialReference.WGS84 }
            };
            feature3.Attributes.Add("random", "rtcxbvbx");
            feature3.Attributes.Add("something", 45445);

            var feature4 = new Feature<Point>
            {
                Geometry = new Point { X = 50.342, Y = -30.331, SpatialReference = SpatialReference.WGS84 }
            };
            feature4.Attributes.Add("random", "rtcxbvbx");
            feature4.Attributes.Add("something", 4);

            Assert.NotEqual(feature1, feature2);
            Assert.NotEqual(feature3, feature4);
            Assert.NotEqual(feature1, feature3);
            Assert.NotEqual(feature1, feature4);
            Assert.NotEqual(feature2, feature3);
            Assert.NotEqual(feature2, feature4);
        }
    }
}
