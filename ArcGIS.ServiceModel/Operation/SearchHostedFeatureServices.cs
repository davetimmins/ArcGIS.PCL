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
    public class SearchHostedFeatureServices : SearchArcGISOnline
    {
        /// <summary>
        /// Search for hosted feature services on ArcGIS Online
        /// </summary>
        /// <param name="username">The name of the user (owner) of the feature services</param>
        public SearchHostedFeatureServices(String username)
            : base(String.Format("owner:{0} AND (type:\"Feature Service\")", username))            
        { }
    }

    /// <summary>
    /// Search against ArcGISOnline / Portal
    /// </summary>
    [DataContract]
    public class SearchArcGISOnline : ArcGISServerOperation
    {
        /// <summary>
        /// SSearch against ArcGISOnline / Portal
        /// </summary>
        /// <param name="query">The search query to execute</param>
        public SearchArcGISOnline(String query)            
        {
            Query = query;
            SortField = "created";
            SortOrder = "desc";
            NumberToReturn = 100;
            StartIndex = 1;

            Endpoint = new ArcGISOnlineEndpoint(Operations.ArcGISOnlineSearch);
        }

        /// <summary>
        /// Search query to execute
        /// </summary>
        [DataMember(Name = "q")]
        public String Query { get; protected set; }

        /// <summary>
        /// Field to sort results by
        /// </summary>
        /// <remarks>Default is created</remarks>
        [DataMember(Name = "sortField")]
        public String SortField { get; set; }

        /// <summary>
        /// Order results by desc or asc
        /// </summary>
        /// <remarks>Default is desc</remarks>
        [DataMember(Name = "sortOrder")]
        public String SortOrder { get; set; }

        /// <summary>
        /// Maximum number of results to return
        /// </summary>
        [DataMember(Name = "num")]
        public int NumberToReturn { get; set; }

        /// <summary>
        /// Start index of results
        /// </summary>
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
        public String Id { get; set; }

        [DataMember(Name = "name")]
        public String Name { get; set; }

        [DataMember(Name = "url")]
        public String Url { get; set; }
    }
}
