using ArcGIS.ServiceModel.GeoJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ArcGIS.ServiceModel.Common
{
    /// <summary>
    /// The REST API supports the following five geometry types:
    /// Points,
    /// Multipoints,
    /// Polylines,
    /// Polygons,
    /// Envelopes
    /// </summary>
    /// <remarks>Starting at ArcGIS Server 10.1, geometries containing m and z values are supported</remarks>
    public interface IGeometry
    {
        /// <summary>
        /// The spatial reference can be defined using a well-known ID (wkid) or well-known text (wkt) 
        /// </summary>
        [DataMember(Name = "spatialReference")]
        SpatialReference SpatialReference { get; set; }

        IGeoJsonGeometry ToGeoJson();
    }

    [DataContract]
    public class SpatialReference : IEquatable<SpatialReference>
    {
        /// <summary>
        /// World Geodetic System 1984 (WGS84)
        /// </summary>
        public static SpatialReference WGS84 = new SpatialReference
            {
                Wkid = 4326,
                LatestWkid = 4326
            };

        /// <summary>
        /// WGS 1984 Web Mercator (Auxiliary Sphere)
        /// </summary>
        public static SpatialReference WebMercator = new SpatialReference
            {
                Wkid = 102100,
                LatestWkid = 3857
            };

        [DataMember(Name = "wkid")]
        public int Wkid { get; set; }

        [DataMember(Name = "latestWkid")]
        public int LatestWkid { get; set; }

        [DataMember(Name = "vcsWkid")]
        public int VCSWkid { get; set; }

        [DataMember(Name = "latestVcsWkid")]
        public int LatestVCSWkid { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SpatialReference)obj);
        }

        public bool Equals(SpatialReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Wkid == other.Wkid && LatestWkid == other.LatestWkid && VCSWkid == other.VCSWkid && LatestVCSWkid == other.LatestVCSWkid;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Wkid;
                hashCode = (hashCode * 397) ^ LatestWkid;
                hashCode = (hashCode * 397) ^ VCSWkid;
                hashCode = (hashCode * 397) ^ LatestVCSWkid;
                return hashCode;
            }
        }
    }

    [DataContract]
    public class Point : IGeometry, IEquatable<Point>
    {
        [DataMember(Name = "spatialReference", Order = 5)]
        public SpatialReference SpatialReference { get; set; }

        [DataMember(Name = "x", Order = 1)]
        public double X { get; set; }

        [DataMember(Name = "y", Order = 2)]
        public double Y { get; set; }

        [DataMember(Name = "z", Order = 3)]
        public double? Z { get; set; }

        [DataMember(Name = "m", Order = 4)]
        public double? M { get; set; }

        public bool Equals(Point other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && M.Equals(other.M) && Equals(SpatialReference, other.SpatialReference);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                hashCode = (hashCode * 397) ^ M.GetHashCode();
                hashCode = (hashCode * 397) ^ (SpatialReference != null ? SpatialReference.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Point)obj);
        }

        public IGeoJsonGeometry ToGeoJson()
        {
            return new GeoJsonPoint { Type = "Point", Coordinates = new[] { X, Y } };
        }
    }

    [DataContract]
    public class MultiPoint : IGeometry, IEquatable<MultiPoint>
    {
        [DataMember(Name = "spatialReference", Order = 4)]
        public SpatialReference SpatialReference { get; set; }

        [DataMember(Name = "hasM", Order = 1)]
        public bool HasM { get; set; }

        [DataMember(Name = "hasZ", Order = 2)]
        public bool HasZ { get; set; }

        [DataMember(Name = "points", Order = 3)]
        public PointCollection Points { get; set; }

        public bool Equals(MultiPoint other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(SpatialReference, other.SpatialReference) && HasM.Equals(other.HasM) && HasZ.Equals(other.HasZ) && Equals(Points, other.Points);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (SpatialReference != null ? SpatialReference.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ HasM.GetHashCode();
                hashCode = (hashCode * 397) ^ HasZ.GetHashCode();
                hashCode = (hashCode * 397) ^ (Points != null ? Points.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MultiPoint)obj);
        }

        public IGeoJsonGeometry ToGeoJson()
        {
            return new GeoJsonLineString { Type = "MultiPoint", Coordinates = Points };
        }
    }

    [DataContract]
    public class Polyline : IGeometry, IEquatable<Polyline>
    {
        [DataMember(Name = "spatialReference", Order = 4)]
        public SpatialReference SpatialReference { get; set; }

        [DataMember(Name = "hasM", Order = 1)]
        public bool HasM { get; set; }

        [DataMember(Name = "hasZ", Order = 2)]
        public bool HasZ { get; set; }

        [DataMember(Name = "paths", Order = 3)]
        public PointCollectionList Paths { get; set; }

        public bool Equals(Polyline other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(SpatialReference, other.SpatialReference) && HasM.Equals(other.HasM) && HasZ.Equals(other.HasZ) && Equals(Paths, other.Paths);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (SpatialReference != null ? SpatialReference.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ HasM.GetHashCode();
                hashCode = (hashCode * 397) ^ HasZ.GetHashCode();
                hashCode = (hashCode * 397) ^ (Paths != null ? Paths.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Polyline)obj);
        }

        public IGeoJsonGeometry ToGeoJson()
        {
            return Paths.Any() ? new GeoJsonLineString { Type = "LineString", Coordinates = Paths.First() } : null;
        }
    }

    public class PointCollection : List<double[]>
    {
        public List<Point> Points
        {
            get { return this.Select(point => point != null ? new Point { X = point.First(), Y = point.Last() } : null).ToList(); }
        }
    }

    public class PointCollectionList : List<PointCollection>
    {
    }

    [DataContract]
    public class Polygon : IGeometry, IEquatable<Polygon>
    {
        [DataMember(Name = "spatialReference", Order = 4)]
        public SpatialReference SpatialReference { get; set; }

        [DataMember(Name = "hasM", Order = 1)]
        public bool HasM { get; set; }

        [DataMember(Name = "hasZ", Order = 2)]
        public bool HasZ { get; set; }

        [DataMember(Name = "rings", Order = 3)]
        public PointCollectionList Rings { get; set; }

        public bool Equals(Polygon other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(SpatialReference, other.SpatialReference) && HasM.Equals(other.HasM) && HasZ.Equals(other.HasZ) && Equals(Rings, other.Rings);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (SpatialReference != null ? SpatialReference.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ HasM.GetHashCode();
                hashCode = (hashCode * 397) ^ HasZ.GetHashCode();
                hashCode = (hashCode * 397) ^ (Rings != null ? Rings.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Polygon)obj);
        }

        public IGeoJsonGeometry ToGeoJson()
        {
            return new GeoJsonPolygon { Type = "Polygon", Coordinates = Rings };
        }
    }

    [DataContract]
    public class Extent : IGeometry, IEquatable<Extent>
    {
        [DataMember(Name = "spatialReference", Order = 5)]
        public SpatialReference SpatialReference { get; set; }

        [DataMember(Name = "xmin", Order = 1)]
        public double XMin { get; set; }

        [DataMember(Name = "xmax", Order = 3)]
        public double XMax { get; set; }

        [DataMember(Name = "ymin", Order = 2)]
        public double YMin { get; set; }

        [DataMember(Name = "ymax", Order = 4)]
        public double YMax { get; set; }

        public bool Equals(Extent other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(SpatialReference, other.SpatialReference) && XMin.Equals(other.XMin) && XMax.Equals(other.XMax) && YMin.Equals(other.YMin) && YMax.Equals(other.YMax);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (SpatialReference != null ? SpatialReference.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ XMin.GetHashCode();
                hashCode = (hashCode * 397) ^ XMax.GetHashCode();
                hashCode = (hashCode * 397) ^ YMin.GetHashCode();
                hashCode = (hashCode * 397) ^ YMax.GetHashCode();
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Extent)obj);
        }

        public IGeoJsonGeometry ToGeoJson()
        {
            return new GeoJsonPolygon
            {
                Type = "Polygon",
                Coordinates = new PointCollectionList 
                { 
                    new PointCollection 
                    {
                        new[]{ XMin, YMin },                        
                        new[]{ XMax, YMin },
                        new[]{ XMax, YMax },
                        new[]{ XMin, YMax }, 
                        new[]{ XMin, YMin }
                    }
                }
            };
        }
    }
}
