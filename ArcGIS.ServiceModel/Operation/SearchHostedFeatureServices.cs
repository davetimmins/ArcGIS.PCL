using ArcGIS.ServiceModel.Common;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ArcGIS.ServiceModel.Operation
{
    /// <summary>
    /// Search for hosted feature services on ArcGIS Online
    /// </summary>
    [DataContract]
    public class SearchHostedFeatureServices : ArcGISServerOperation
    {
        /// <summary>
        /// Search for hosted feature services on ArcGIS Online
        /// </summary>
        /// <param name="username">The name of the user (owner) of the feature services</param>
        public SearchHostedFeatureServices(String username)            
        {
            if (String.IsNullOrWhiteSpace(username)) throw new ArgumentNullException("You must specify a user to search for.");

            Query = String.Format("owner:{0} AND (type:\"Feature Service\")", username);
            SortField = "created";
            SortOrder = "desc";
            NumberToReturn = 100;
            StartIndex = 1;

            Endpoint = new ArcGISOnlineEndpoint(Operations.ArcGISOnlineSearch);
        }

        [DataMember(Name = "q")]
        public String Query { get; private set; }

        [DataMember(Name = "sortField")]
        public String SortField { get; set; }

        [DataMember(Name = "sortOrder")]
        public String SortOrder { get; set; }

        [DataMember(Name = "num")]
        public int NumberToReturn { get; set; }

        [DataMember(Name = "start")]
        public int StartIndex { get; set; }
    }

    [DataContract]
    public class SearchHostedFeatureServicesResponse : PortalResponse
    {
        [DataMember(Name = "results")]
        public HostedFeatureService[] Results { get; set; }
    }

    [DataContract]
    public class HostedFeatureService
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }
    }
}
