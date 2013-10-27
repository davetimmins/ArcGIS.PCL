using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ArcGIS.ServiceModel.Common;

namespace ArcGIS.ServiceModel.Operation
{
    [DataContract]
    public class SingleInputGeocode : GeocodeOperation
    {
        public SingleInputGeocode(ArcGISServerEndpoint endpoint)
            : base(endpoint, Operations.SingleInputGeocode)
        {
            MaxResults = 1;
            Distance = null;
        }

        /// <summary>
        /// Specifies the location to be geocoded. This can be a street address, place name, postal code, or POI. It is a required parameter
        /// </summary>
        [DataMember(Name = "text")]
        public String Text { get; set; }

        /// <summary>
        /// A value representing the country. Providing this value increases geocoding speed.
        /// Acceptable values include the full country name, the ISO 3166-1 2-digit country code, or the ISO 3166-1 3-digit country code.
        /// </summary>
        [DataMember(Name = "sourceCountry")]
        public String SourceCountry { get; set; }

        /// <summary>
        /// The maximum number of results to be returned by a search, up to the maximum number allowed by the service. 
        /// If not specified, then one location will be returned. 
        /// The world geocoding service allows up to 20 candidates to be returned for a single request. 
        /// Note that up to 50 POI candidates can be returned
        /// </summary>
        [DataMember(Name = "maxLocations")]
        public int MaxResults { get; set; }

        [DataMember(Name = "magicKey")]
        public String MagicKey { get; set; }

        /// <summary>
        /// A set of bounding box coordinates that limit the search area to a specific region. 
        /// This is especially useful for applications in which a user will search for places and addresses only within the current map extent. 
        /// </summary>
        [DataMember(Name = "bbox")]
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
    public class SingleInputGeocodeResponse : PortalResponse
    {
        [DataMember(Name = "SpatialReference")]
        public SpatialReference SpatialReference { get; set; }

        [DataMember(Name = "locations")]
        public IEnumerable<Location> Results { get; set; }
    }

    [DataContract]
    public class Location
    {
        [DataMember(Name = "name")]
        public String Name { get; set; }

        [DataMember(Name = "extent")]
        public Extent Extent { get; set; }

        [DataMember(Name = "feature")]
        public Feature<Point> Feature { get; set; }
    }
}
