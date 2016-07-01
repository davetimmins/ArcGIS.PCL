namespace ArcGIS.ServiceModel.Operation
{
    using Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// Request for the details of an ArcGIS Server service layer
    /// </summary>
    [DataContract]
    public class ServiceLayerDescription : ArcGISServerOperation
    {
        /// <summary>
        /// Request for the details of an ArcGIS Server service layer
        /// </summary>
        /// <param name="serviceEndpoint"></param>
        public ServiceLayerDescription(IEndpoint serviceEndpoint)
        {
            Endpoint = serviceEndpoint;
        }
    }

    [DataContract]
    public class ServiceLayerDescriptionResponse : PortalResponse
    {
        readonly static Dictionary<string, Func<Type>> TypeMap = new Dictionary<string, Func<Type>>
        {
            { GeometryTypes.Point, () => typeof(Point) },
            { GeometryTypes.MultiPoint, () => typeof(MultiPoint) },
            { GeometryTypes.Envelope, () => typeof(Extent) },
            { GeometryTypes.Polygon, () => typeof(Polygon) },
            { GeometryTypes.Polyline, () => typeof(Polyline) }
        };

        [DataMember(Name = "currentVersion")]
        public double CurrentVersion { get; set; }

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [IgnoreDataMember]
        public bool IsGroupLayer { get { return string.Equals(Type, "group layer", StringComparison.CurrentCultureIgnoreCase); } }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "definitionExpression")]
        public string DefinitionExpression { get; set; }

        [DataMember(Name = "geometryType")]
        public string GeometryTypeString { get; set; }

        [IgnoreDataMember]
        public Type GeometryType { get { return TypeMap[GeometryTypeString](); } }

        [DataMember(Name = "copyrightText")]
        public string CopyrightText { get; set; }

        [DataMember(Name = "parentLayer")]
        public RelatedLayer ParentLayer { get; set; }

        [DataMember(Name = "subLayers")]
        public List<RelatedLayer> SubLayers { get; set; }

        [DataMember(Name = "minScale")]
        public int MinimumScale { get; set; }

        [DataMember(Name = "maxScale")]
        public int MaximumScale { get; set; }

        [DataMember(Name = "defaultVisibility")]
        public bool DefaultVisibility { get; set; }

        [DataMember(Name = "extent")]
        public Extent Extent { get; set; }

        [DataMember(Name = "hasAttachments")]
        public bool HasAttachments { get; set; }

        [DataMember(Name = "htmlPopupType")]
        public string HtmlPopupType { get; set; }

        [DataMember(Name = "displayField")]
        public string DisplayField { get; set; }

        [DataMember(Name = "canModifyLayer")]
        public bool? CanModifyLayer { get; set; }

        [DataMember(Name = "canScaleSymbols")]
        public bool? CanScaleSymbols { get; set; }

        [DataMember(Name = "hasLabels")]
        public bool? HasLabels { get; set; }

        [DataMember(Name = "capabilities")]
        public string CapabilitiesValue { get; set; }

        [IgnoreDataMember]
        public List<string> Capabilities { get { return string.IsNullOrWhiteSpace(CapabilitiesValue) ? null : CapabilitiesValue.Split(',').ToList(); } }

        [DataMember(Name = "maxRecordCount")]
        public int MaximumRecordCount { get; set; }

        [DataMember(Name = "supportsStatistics")]
        public bool? SupportsStatistics { get; set; }

        [DataMember(Name = "supportsAdvancedQueries")]
        public bool? SupportsAdvancedQueries { get; set; }

        [DataMember(Name = "supportedQueryFormats")]
        public string SupportedQueryFormatsValue { get; set; }

        [IgnoreDataMember]
        public List<string> SupportedQueryFormats { get { return string.IsNullOrWhiteSpace(SupportedQueryFormatsValue) ? null : SupportedQueryFormatsValue.Split(',').ToList(); } }

        [DataMember(Name = "isDataVersioned")]
        public bool? IsDataVersioned { get; set; }
    }

    [DataContract]
    public class RelatedLayer
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
