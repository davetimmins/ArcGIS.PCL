using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ArcGIS.ServiceModel.Common;

namespace ArcGIS.ServiceModel.Operation
{
    /// <summary>
    /// Basic query request operation
    /// </summary>
    [DataContract]
    public class Query : ArcGISServerOperation
    {      
        /// <summary>
        /// Represents a request for a query against a service resource
        /// </summary>
        /// <param name="endpoint">Resource to apply the query against</param>
        public Query(ArcGISServerEndpoint endpoint)
            : base(endpoint, Operations.Query)
        {
            Where = "1=1";
            OutFields = "*";
            ReturnGeometry = true;
            SpatialRelationship = SpatialRelationshipTypes.Intersects;
        }

        /// <summary>
        /// A where clause for the query filter. Any legal SQL where clause operating on the fields in the layer is allowed.
        /// </summary>
        /// <remarks>Default is '1=1'</remarks>
        [DataMember(Name = "where")]
        public String Where { get; set; }

        /// <summary>
        /// The list of fields to be included in the returned resultset. This list is a comma delimited list of field names. 
        /// If you specify the shape field in the list of return fields, it is ignored. To request geometry, set returnGeometry to true. 
        /// </summary>
        /// <remarks>Default is '*' (all fields)</remarks>
        [DataMember(Name = "outFields")]
        public String OutFields { get; set; }

        /// <summary>
        /// The spatial reference of the input geometry. 
        /// </summary>
        [DataMember(Name = "inSR")]
        public SpatialReference InputSpatialReference 
        {
            get { return Geometry == null ? null : Geometry.SpatialReference ?? SpatialReference.WGS84; }
        }

        /// <summary>
        /// The spatial reference of the returned geometry. 
        /// If not specified, the geometry is returned in the spatial reference of the input.
        /// </summary>
        [DataMember(Name = "outSR")]
        public SpatialReference OutputSpatialReference { get; set; }

        /// <summary>
        /// The geometry to apply as the spatial filter.
        /// The structure of the geometry is the same as the structure of the json geometry objects returned by the ArcGIS REST API.
        /// </summary>
        /// <remarks>Default is empty</remarks>
        [DataMember(Name = "geometry")]
        public IGeometry Geometry { get; set; }

        /// <summary>
        /// The type of geometry specified by the geometry parameter. 
        /// The geometry type can be an envelope, point, line, or polygon.
        /// The default geometry type is "esriGeometryEnvelope". 
        /// Values: esriGeometryPoint | esriGeometryMultipoint | esriGeometryPolyline | esriGeometryPolygon | esriGeometryEnvelope
        /// </summary>
        /// <remarks>Default is esriGeometryEnvelope</remarks>
        [DataMember(Name = "geometryType")]
        public String GeometryType 
        {
            get { return Geometry == null 
                ? GeometryTypes.Envelope
                : GeometryTypes.TypeMap[Geometry.GetType()]();
            } 
        }

        /// <summary>
        /// The spatial relationship to be applied on the input geometry while performing the query.
        /// The supported spatial relationships include intersects, contains, envelope intersects, within, etc.
        /// The default spatial relationship is "esriSpatialRelIntersects".
        /// Values: esriSpatialRelIntersects | esriSpatialRelContains | esriSpatialRelCrosses | esriSpatialRelEnvelopeIntersects | esriSpatialRelIndexIntersects | esriSpatialRelOverlaps | esriSpatialRelTouches | esriSpatialRelWithin | esriSpatialRelRelation
        /// </summary>
        [DataMember(Name = "spatialRel")]
        public String SpatialRelationship { get; set; }

        /// <summary>
        /// If true, the resultset includes the geometry associated with each result.
        /// </summary>
        /// <remarks>Default is true</remarks>
        [DataMember(Name = "returnGeometry")]
        public bool ReturnGeometry { get; set; }

        [IgnoreDataMember]
        public DateTime? From { get; set; }

        [IgnoreDataMember]
        public DateTime? To { get; set; }

        /// <summary>
        /// The time instant or the time extent to query.
        /// </summary>
        /// <remarks>If no To value is specified we will use the From value again, equivalent of using a time instant.</remarks>
        [DataMember(Name = "time")]
        public String Time
        {
            get
            {
                return (From == null) ? String.Empty : String.Format("{0},{1}",
                  From.Value.ToUnixTime(),
                  (To ?? From.Value).ToUnixTime());
            }
        }

        /// <summary>
        /// This option can be used to specify the maximum allowable offset to be used for generalizing geometries returned by the query operation.
        /// </summary>
        [DataMember(Name = "maxAllowableOffset")]
        public int? MaxAllowableOffset { get; set; }

        /// <summary>
        /// This option can be used to specify the number of decimal places in the response geometries returned by the query operation. 
        /// This applies to X and Y values only (not m or z values).
        /// </summary>
        [DataMember(Name = "geometryPrecision")]
        public int? GeometryPrecision { get; set; }

        /// <summary>
        /// If true, Z values will be included in the results if the features have Z values. Otherwise, Z values are not returned.
        /// </summary>
        /// <remarks>Default is false. This parameter only applies if returnGeometry=true.</remarks>
        [DataMember(Name = "returnZ")]
        public bool ReturnZ { get; set; }

        /// <summary>
        /// If true, M values will be included in the results if the features have M values. Otherwise, M values are not returned.
        /// </summary>
        /// <remarks>Default is false. This parameter only applies if returnGeometry=true.</remarks>
        [DataMember(Name = "returnM")]
        public bool ReturnM { get; set; }

        /// <summary>
        /// GeoDatabase version to query.
        /// </summary>
        [DataMember(Name = "gdbVersion")]
        public String GdbVersion { get; set; }

        /// <summary>
        /// If true, returns distinct values based on the fields specified in outFields. 
        /// This parameter applies only if supportsAdvancedQueries property of the layer is true.
        [DataMember(Name = "returnDistinctValues")]
        public bool ReturnDistinctValues { get; set; }
    }

    [DataContract]
    public class QueryResponse<T> : PortalResponse where T : IGeometry
    {
        [DataMember(Name = "features")]
        public IEnumerable<Feature<T>> Features { get; set; }

        [DataMember(Name = "spatialReference")]
        public SpatialReference SpatialReference { get; set; }
    }

    public static class GeometryTypes
    {
        internal readonly static Dictionary<Type, Func<String>> TypeMap = new Dictionary<Type, Func<String>>
            {
                { typeof(Point), () => GeometryTypes.Point },
                { typeof(MultiPoint), () => GeometryTypes.MultiPoint },
                { typeof(Extent), () => GeometryTypes.Envelope },
                { typeof(Polygon), () => GeometryTypes.Polygon },
                { typeof(Polyline), () => GeometryTypes.Polyline }
            };

        public const String Point = "esriGeometryPoint";
        public const String MultiPoint = "esriGeometryMultipoint";
        public const String Polyline = "esriGeometryPolyline";
        public const String Polygon = "esriGeometryPolygon";
        public const String Envelope = "esriGeometryEnvelope";
    }

    public static class SpatialRelationshipTypes
    {
        public const String Intersects = "esriSpatialRelIntersects";
        public const String Contains = "esriSpatialRelContains";
        public const String Crosses = "esriSpatialRelCrosses";
        public const String EnvelopeIntersects = "esriSpatialRelEnvelopeIntersects";
        public const String IndexIntersects = "esriSpatialRelIndexIntersects";
        public const String Overlaps = "esriSpatialRelOverlaps";
        public const String Touches = "esriSpatialRelTouches";
        public const String Within = "esriSpatialRelWithin";
        public const String Relation = "esriSpatialRelRelation";
    }
}
