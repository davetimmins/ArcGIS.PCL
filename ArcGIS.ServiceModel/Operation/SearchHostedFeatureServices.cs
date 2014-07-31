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

        public SearchHostedFeatureServices()
            : base("type:\"Feature Service\"")
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
        /// Search query to execute. See http://resources.arcgis.com/en/help/arcgis-rest-api/index.html#//02r3000000mn000000 for more information
        /// </summary>
        [DataMember(Name = "q")]
        public String Query { get; protected set; }

        /// <summary>
        /// Field to sort results by.
        /// Valid fields are: title, created, type, owner, avgRating, numRatings, numComments and numViews
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
        /// The maximum number of results to be included in the result set response. 
        /// The default value is 10 and the maximum allowed value is 100. 
        /// The start parameter combined with the NumberToReturn parameter can be used to paginate the search results. 
        /// Note that the actual number of returned results may be less than NumberToReturn if the number of 
        /// results remaining after start is less than NumberToReturn
        /// </summary>
        [DataMember(Name = "num")]
        public int NumberToReturn { get; set; }

        /// <summary>
        /// The number of the first entry in the result set response. 
        /// The index number is 1-based. The StartIndex parameter, along with the NumberToReturn parameter 
        /// can be used to paginate the search results.
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
