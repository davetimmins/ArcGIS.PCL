using ArcGIS.ServiceModel.Common;
using System;
using Xunit;

namespace ArcGIS.Test
{
    public class GeometryTests : TestsFixture
    {
        [Fact]
        public void FeaturesAreTheSame()
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
        public void FeaturesAreNotTheSame()
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

            var feature5 = new Feature<Point>
            {
                Geometry = new Point { X = 50.342, Y = -30.331, SpatialReference = SpatialReference.WGS84 }
            };
            feature5.Attributes.Add("random", "rtcxbvbx");
            feature5.Attributes.Add("something", 4);
            feature5.Attributes.Add("somethingelse", 4);

            var feature6 = new Feature<Point>
            {
                Geometry = new Point { X = 50.342, Y = -30.331, SpatialReference = SpatialReference.WGS84 }
            };
            feature6.Attributes.Add("random", "rtcxbvbx");
            feature6.Attributes.Add("something", 4);
            feature6.Attributes.Add("somethingelseagain", 4);

            Assert.NotEqual(feature1, feature2);
            Assert.NotEqual(feature3, feature4);
            Assert.NotEqual(feature1, feature3);
            Assert.NotEqual(feature1, feature4);
            Assert.NotEqual(feature2, feature3);
            Assert.NotEqual(feature2, feature4);
            Assert.NotEqual(feature1, feature5);
            Assert.NotEqual(feature2, feature5);
            Assert.NotEqual(feature3, feature5);
            Assert.NotEqual(feature4, feature5);
            Assert.NotEqual(feature1, feature6);
            Assert.NotEqual(feature2, feature6);
            Assert.NotEqual(feature3, feature6);
            Assert.NotEqual(feature4, feature6);
            Assert.NotEqual(feature5, feature6);
        }

        [Fact]
        public void SpatialReferencesAreTheSame()
        {
            var sr = new SpatialReference { Wkid = SpatialReference.WGS84.Wkid };
            Assert.Equal(sr, SpatialReference.WGS84);

            var sr2 = new SpatialReference { Wkid = SpatialReference.WGS84.LatestWkid };
            Assert.Equal(sr2, SpatialReference.WGS84);

            var sr3 = SpatialReference.WGS84;
            Assert.Equal(sr3, SpatialReference.WGS84);

            Assert.True(sr == sr2);
            Assert.True(sr == sr3);
            Assert.True(sr3 == sr2);
            Assert.False(SpatialReference.WGS84 == null);
            Assert.False(null == SpatialReference.WGS84);
            Assert.False(new SpatialReference { Wkid = 2193 } == SpatialReference.WGS84);
        }

        [Fact]
        public void SpatialReferencesAreNotTheSame()
        {
            var sr = new SpatialReference { Wkid = SpatialReference.WebMercator.Wkid };
            Assert.NotEqual(sr, SpatialReference.WGS84);

            var sr2 = SpatialReference.WebMercator;
            Assert.NotEqual(sr2, SpatialReference.WGS84);

            Assert.True(SpatialReference.WGS84 != SpatialReference.WebMercator);
            Assert.False(SpatialReference.WGS84 == SpatialReference.WebMercator);

            Assert.True(sr != SpatialReference.WGS84);
            Assert.True(SpatialReference.WebMercator != SpatialReference.WGS84);

            Assert.True(SpatialReference.WGS84 != null);
            Assert.True(null != SpatialReference.WGS84);
            Assert.True(new SpatialReference { Wkid = 2193 } != SpatialReference.WGS84);
            Assert.False(SpatialReference.WGS84 != new SpatialReference { Wkid = SpatialReference.WGS84.Wkid });
        }

        [Fact]
        public void CanGetObjectID()
        {
            long oid = 34223;

            var feature = new Feature<Point>();
            feature.Attributes.Add("ObjectId", oid);
            Assert.True(feature.ObjectID > 0);
            Assert.Equal(feature.ObjectID, oid);

            var feature2 = new Feature<Point>();
            feature2.Attributes.Add("ObjectID", oid);
            Assert.True(feature2.ObjectID > 0);
            Assert.Equal(feature2.ObjectID, oid);

            var feature3 = new Feature<Point>();
            feature3.Attributes.Add("Objectid", oid);
            Assert.True(feature3.ObjectID > 0);
            Assert.Equal(feature3.ObjectID, oid);

            var feature4 = new Feature<Point>();
            feature4.Attributes.Add("objectid", oid);
            Assert.True(feature4.ObjectID > 0);
            Assert.Equal(feature4.ObjectID, oid);

            var feature5 = new Feature<Point>();
            feature5.Attributes.Add("objevxvcvxcctid", oid);
            Assert.Equal(feature5.ObjectID, 0);

            var feature6 = new Feature<Point>();
            Assert.Equal(feature6.ObjectID, 0);
        }

        [Fact]
        public void CanGetGlobalID()
        {
            var guid = Guid.NewGuid();

            var feature = new Feature<Point>();
            feature.Attributes.Add("GlobalId", guid);
            Assert.False(string.IsNullOrWhiteSpace(feature.GlobalID));
            Assert.Equal(feature.GlobalID, guid.ToString());

            var feature2 = new Feature<Point>();
            feature2.Attributes.Add("GlobalID", guid);
            Assert.False(string.IsNullOrWhiteSpace(feature2.GlobalID));
            Assert.Equal(feature2.GlobalID, guid.ToString());

            var feature3 = new Feature<Point>();
            feature3.Attributes.Add("Globalid", guid);
            Assert.False(string.IsNullOrWhiteSpace(feature3.GlobalID));
            Assert.Equal(feature3.GlobalID, guid.ToString());

            var feature4 = new Feature<Point>();
            feature4.Attributes.Add("globalid", guid);
            Assert.False(string.IsNullOrWhiteSpace(feature4.GlobalID));
            Assert.Equal(feature4.GlobalID, guid.ToString());

            var feature5 = new Feature<Point>();
            feature5.Attributes.Add("globavcbbcblid", guid);
            Assert.True(string.IsNullOrWhiteSpace(feature5.GlobalID));
            Assert.Equal(feature5.GlobalID, "");

            var feature6 = new Feature<Point>();
            Assert.True(string.IsNullOrWhiteSpace(feature6.GlobalID));
            Assert.Equal(feature6.GlobalID, "");
        }
    }
}
