using ArcGIS.ServiceModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ArcGIS.ServiceModel.Operation
{
    [DataContract]
    public class SingleInputCustomGeocode : ArcGISServerOperation
    {
        public SingleInputCustomGeocode(ArcGISServerEndpoint endpoint)
            : base(endpoint, Operations.SingleInputCustomGeocode)
        { }

        /// <summary>
        /// Specifies the location to be searched for.
        /// </summary>
        [DataMember(Name = "text")]
        public String Text { get; set; }

        /// <summary>
        /// The spatial reference of the x/y coordinates returned by a geocode request. 
        /// This is useful for applications using a map with a spatial reference different than that of the geocode service. 
        /// If the outSR is not specified, the spatial reference of the output locations is the same as that of the service. 
        /// The world geocoding service spatial reference is WGS84 (WKID = 4326). 
        /// </summary>
        [DataMember(Name = "outSR")]
        public SpatialReference OutputSpatialReference { get; set; }

        /// <summary>
        /// A set of bounding box coordinates that limit the search area to a specific region. 
        /// This is especially useful for applications in which a user will search for places and addresses only within the current map extent. 
        /// </summary>
        [DataMember(Name = "searchExtent")]
        public Extent SearchExtent { get; set; }

        /// <summary>
        ///  The field names of the attributes to be returned.
        /// </summary>
        [IgnoreDataMember]
        public List<String> OutFields { get; set; }

        [DataMember(Name = "outFields")]
        public String OutFieldsValue { get { return OutFields == null ? String.Empty : String.Join(",", OutFields); } }
    }

    [DataContract]
    public class SingleInputCustomGeocodeResponse
    {
        [DataMember(Name = "spatialReference")]
        public SpatialReference SpatialReference { get; set; }

        [DataMember(Name = "candidates")]
        public Candidate[] Candidates { get; set; }
    }

    [DataContract]
    public class Candidate
    {
        [DataMember(Name = "address")]
        public String Address { get; set; }

        [DataMember(Name = "location")]
        public Point Location { get; set; }

        [DataMember(Name = "score")]
        public double Score { get; set; }

        [DataMember(Name = "attributes")]
        public Dictionary<String, object> Attributes { get; set; }
    }

    [DataContract]
    public class SuggestGeocode : GeocodeOperation
    {
        public SuggestGeocode(ArcGISServerEndpoint endpoint)
            : base(endpoint, Operations.SuggestGeocode)
        {
            Distance = null;
        }

        /// <summary>
        /// Specifies the location to be searched for.
        /// </summary>
        [DataMember(Name = "text")]
        public String Text { get; set; }

        /// <summary>
        /// The maximum number of suggestions that can be returned.
        /// </summary>
        [DataMember(Name = "maxLocations")]
        public Extent MaximumNumberOfSuggestions { get; set; }
    }

    [DataContract]
    public class SuggestGeocodeResponse
    {
        [DataMember(Name = "suggestions")]
        public Suggestion[] Suggestions { get; set; }
    }

    [DataContract]
    public class Suggestion
    {
        [DataMember(Name = "text")]
        public String Text { get; set; }

        [DataMember(Name = "magicKey")]
        public String MagicKey { get; set; }
    }
}
