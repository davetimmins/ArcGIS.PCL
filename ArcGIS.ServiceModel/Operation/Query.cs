﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel.Extensions;

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
            GeometryType = "esriGeometryEnvelope";
            SpatialRelationship = "esriSpatialRelIntersects";
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
        /// The geometry to apply as the spatial filter.
        /// The structure of the geometry is the same as the structure of the json geometry objects returned by the ArcGIS REST API.
        /// In addition to the JSON structures, for envelopes and points, you can specify the geometry with a simpler comma-separated syntax.
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
        public string GeometryType { get; set; }

        /// <summary>
        /// The spatial relationship to be applied on the input geometry while performing the query.
        /// The supported spatial relationships include intersects, contains, envelope intersects, within, etc.
        /// The default spatial relationship is "esriSpatialRelIntersects".
        /// Values: esriSpatialRelIntersects | esriSpatialRelContains | esriSpatialRelCrosses | esriSpatialRelEnvelopeIntersects | esriSpatialRelIndexIntersects | esriSpatialRelOverlaps | esriSpatialRelTouches | esriSpatialRelWithin | esriSpatialRelRelation
        /// </summary>
        [DataMember(Name = "spatialRel")]
        public string SpatialRelationship { get; set; }

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

        // TODO : add more options
    }

    [DataContract]
    public class QueryResponse<T> : PortalResponse where T : IGeometry
    {
        [DataMember(Name = "features")]
        public IEnumerable<Feature<T>> Features { get; set; }

        [DataMember(Name = "spatialReference")]
        public SpatialReference SpatialReference { get; set; }
    }
}
