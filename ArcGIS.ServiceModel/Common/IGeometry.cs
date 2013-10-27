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

        /// <summary>
        /// Calculates the minimum bounding extent for the geometry
        /// </summary>
        /// <returns>Extent that can contain the geometry </returns>
        Extent GetExtent();

        /// <summary>
        /// Calculates the center of the minimum bounding extent for the geommetry
        /// </summary>
        /// <returns>The value for the center of the extent for the geometry</returns>
        Point GetCenter();

        /// <summary>
        /// Converts the geometry to its GeoJSON representation
        /// </summary>
        /// <returns>The corresponding GeoJSON for the geometry</returns>
        IGeoJsonGeometry ToGeoJson();
    }
    
    /// <summary>
    /// Spatial reference used for operations. If WKT is set then other properties are nulled
    /// </summary>
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
        public int? Wkid { get; set; }

        [DataMember(Name = "latestWkid")]
        public int? LatestWkid { get; set; }

        [DataMember(Name = "vcsWkid")]
        public int? VCSWkid { get; set; }

        [DataMember(Name = "latestVcsWkid")]
        public int? LatestVCSWkid { get; set; }

        String _wkt;
        [DataMember(Name = "wkt")]
        public String Wkt
        {
            get { return _wkt; }
            set
            {
                _wkt = value;
                Wkid = null;
                LatestWkid = null;
                VCSWkid = null;
                LatestVCSWkid = null;
            }
        }

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
            return (String.IsNullOrWhiteSpace(Wkt))
                ? (Wkid == other.Wkid && LatestWkid == other.LatestWkid && VCSWkid == other.VCSWkid && LatestVCSWkid == other.LatestVCSWkid)
                : String.Equals(Wkt, other.Wkt);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Wkid != null ? Wkid.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LatestWkid != null ? LatestWkid.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (VCSWkid != null ? VCSWkid.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LatestVCSWkid != null ? LatestVCSWkid.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Wkt != null ? Wkt.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public abstract class GeometryBase { }

    [DataContract]
    public class Point : GeometryBase, IGeometry, IEquatable<Point>
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

        public Extent GetExtent()
        {
            return new Extent { XMin = X - 1, YMin = Y - 1, XMax = X + 1, YMax = Y +1, SpatialReference = this.SpatialReference };
        }

        public Point GetCenter()
        {
            return this;
        }

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
    public class MultiPoint : GeometryBase, IGeometry, IEquatable<MultiPoint>
    {
        [DataMember(Name = "spatialReference", Order = 4)]
        public SpatialReference SpatialReference { get; set; }

        [DataMember(Name = "hasM", Order = 1)]
        public bool HasM { get; set; }

        [DataMember(Name = "hasZ", Order = 2)]
        public bool HasZ { get; set; }

        [DataMember(Name = "points", Order = 3)]
        public PointCollection Points { get; set; }

        public Extent GetExtent()
        {
            return Points.CalculateExtent();
        }

        public Point GetCenter()
        {
            return GetExtent().GetCenter();
        }

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
    public class Polyline : GeometryBase, IGeometry, IEquatable<Polyline>
    {
        [DataMember(Name = "spatialReference", Order = 4)]
        public SpatialReference SpatialReference { get; set; }

        [DataMember(Name = "hasM", Order = 1)]
        public bool HasM { get; set; }

        [DataMember(Name = "hasZ", Order = 2)]
        public bool HasZ { get; set; }

        [DataMember(Name = "paths", Order = 3)]
        public PointCollectionList Paths { get; set; }

        public Extent GetExtent()
        {
            Extent extent = null;
            foreach (PointCollection path in this.Paths)
            {
                if (extent != null)
                    extent = extent.Union(path.CalculateExtent());
                else
                    extent = path.CalculateExtent();
            }
            if (extent != null)
                extent.SpatialReference = this.SpatialReference;

            return extent;
        }

        public Point GetCenter()
        {
            return GetExtent().GetCenter();
        }

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
        public Extent CalculateExtent()
        {
            SpatialReference spatialReference;
            double x = double.NaN;
            double y = double.NaN;
            double num = double.NaN;
            double y1 = double.NaN;
            SpatialReference spatialReference1 = null;
            foreach (var point in Points)
            {
                if (point == null) continue;

                if (point.X < x || double.IsNaN(x)) x = point.X;

                if (point.Y < y || double.IsNaN(y)) y = point.Y;

                if (point.X > num || double.IsNaN(num)) num = point.X;

                if (point.Y > y1 || double.IsNaN(y1)) y1 = point.Y;

                if (spatialReference1 != null) continue;

                spatialReference1 = point.SpatialReference;
            }
            if (double.IsNaN(x) || double.IsNaN(y) || double.IsNaN(num) || double.IsNaN(y1))
            {
                return null;
            }
            var envelope = new Extent { XMin = x, YMin = y, XMax = num, YMax = y1 };
            var envelope1 = envelope;
            if (spatialReference1 == null)
                spatialReference = null;
            else
                spatialReference = new SpatialReference { Wkid = spatialReference1.Wkid };

            envelope1.SpatialReference = spatialReference;
            return envelope;
        }

        public List<Point> Points
        {
            get { return this.Select(point => point != null ? new Point { X = point.First(), Y = point.Last() } : null).ToList(); }
        }

    }

    public class PointCollectionList : List<PointCollection>
    { }

    [DataContract]
    public class Polygon : GeometryBase, IGeometry, IEquatable<Polygon>
    {
        [DataMember(Name = "spatialReference", Order = 4)]
        public SpatialReference SpatialReference { get; set; }

        [DataMember(Name = "hasM", Order = 1)]
        public bool HasM { get; set; }

        [DataMember(Name = "hasZ", Order = 2)]
        public bool HasZ { get; set; }

        [DataMember(Name = "rings", Order = 3)]
        public PointCollectionList Rings { get; set; }

        public Extent GetExtent()
        {
            Extent extent = null;
            foreach (var ring in this.Rings)
            {
                if (ring == null) continue;

                if (extent != null)
                    extent = extent.Union(ring.CalculateExtent());
                else
                    extent = ring.CalculateExtent();
            }
            if (extent != null)
            {
                extent.SpatialReference = this.SpatialReference;
                return extent;
            }
            return null;
        }

        public Point GetCenter()
        {            
            return GetExtent().GetCenter();
        }

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
    public class Extent : GeometryBase, IGeometry, IEquatable<Extent>
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

        public Extent GetExtent()
        {
            return this;
        }

        public Point GetCenter()
        {
            var point = new Point { SpatialReference = this.SpatialReference };
            if (!double.IsNaN(this.XMin) && !double.IsNaN(this.XMax))
                point.X = (this.XMin + this.XMax) / 2;

            if (!double.IsNaN(this.YMin) && !double.IsNaN(this.YMax))
                point.Y = (this.YMin + this.YMax) / 2;

            return point;
        }

        public Extent Union(Extent extent)
        {
            var envelope = new Extent();
            if (extent == null)
            {
                extent = this;
            }
            if (!this.SpatialReference.Equals(extent.SpatialReference))
                throw new ArgumentException("Spatial references must match for union operation.");

            envelope.SpatialReference = this.SpatialReference ?? extent.SpatialReference;
            if (double.IsNaN(this.XMin))
            {
                envelope.XMin = extent.XMin;
            }
            else if (!double.IsNaN(extent.XMin))
            {
                envelope.XMin = Math.Min(extent.XMin, this.XMin);
            }
            else
            {
                envelope.XMin = this.XMin;
            }
            if (double.IsNaN(this.XMax))
            {
                envelope.XMax = extent.XMax;
            }
            else if (!double.IsNaN(extent.XMax))
            {
                envelope.XMax = Math.Max(extent.XMax, this.XMax);
            }
            else
            {
                envelope.XMax = this.XMax;
            }
            if (double.IsNaN(this.YMin))
            {
                envelope.YMin = extent.YMin;
            }
            else if (!double.IsNaN(extent.YMin))
            {
                envelope.YMin = Math.Min(extent.YMin, this.YMin);
            }
            else
            {
                envelope.YMin = this.YMin;
            }
            if (double.IsNaN(this.YMax))
            {
                envelope.YMax = extent.YMax;
            }
            else if (!double.IsNaN(extent.YMax))
            {
                envelope.YMax = Math.Max(extent.YMax, this.YMax);
            }
            else
            {
                envelope.YMax = this.YMax;
            }
            return envelope;
        }

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
