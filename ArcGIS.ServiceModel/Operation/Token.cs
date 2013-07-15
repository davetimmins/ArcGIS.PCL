using System;
using System.Runtime.Serialization;
using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel.Extensions;

namespace ArcGIS.ServiceModel.Operation
{
    /// <summary>
    /// This operation generates an access token in exchange for user credentials that can be used by clients when working with the ArcGIS Portal API. 
    /// The call is only allowed over HTTPS and must be a POST.
    /// </summary>
    [DataContract]
    public class GenerateToken : CommonParameters, IEndpoint
    {
        public GenerateToken()
        {
            Client = "referer";
            Referer = "requestip";
            ExpirationInMinutes = 60;
        }

        /// <summary>
        /// The client identification type for which the token is to be granted.
        /// </summary>
        /// <remarks>The default and only value supported is referer.</remarks>
        [DataMember(Name = "client")]
        public String Client { get; private set; }

        /// <summary>
        /// The base URL of the web app that will invoke the Portal API. 
        /// This parameter must be specified if the value of the client parameter is referer.
        /// </summary>
        [DataMember(Name = "referer")]
        public String Referer { get; set; }

        /// <summary>
        /// Username of user who wants to get a token.
        /// </summary>
        [DataMember(Name = "username")]
        public String Username { get; set; }

        /// <summary>
        /// Password of user who wants to get a token.
        /// </summary>
        [DataMember(Name = "password")]
        public String Password { get; set; }

        /// <summary>
        /// The token expiration time in minutes.
        /// </summary>
        /// <remarks> The default is 60 minutes.</remarks>
        [DataMember(Name = "expiration")]
        public int ExpirationInMinutes { get; set; }

        public String RelativeUrl
        {
            get { return "tokens/" + Operations.GenerateToken; }
        }

        public String BuildAbsoluteUrl(String rootUrl)
        {
            if (String.IsNullOrWhiteSpace(rootUrl)) throw new ArgumentNullException("rootUrl");
 
            return (String.Equals(rootUrl, ArcGIS.ServiceModel.Logic.PortalGateway.AGOPortalUrl, StringComparison.OrdinalIgnoreCase))
                ? rootUrl.Replace("http://", "https://") + RelativeUrl.Replace("tokens/", "")
                : rootUrl.Replace("http://", "https://") + RelativeUrl;
        }
    }

    /// <summary>
    /// Represents a token object with a value that can be used to access secure resources
    /// </summary>
    [DataContract]
    public class Token : PortalResponse
    {
        [DataMember(Name = "token")]
        public String Value { get; set; }

        /// <summary>
        /// The expiration time of the token in milliseconds since Jan 1st, 1970.
        /// </summary>
        [DataMember(Name = "expires")]
        public long Expiry { get; set; }

        /// <summary>
        /// If we have a token value then check if it has expired
        /// </summary>
        [IgnoreDataMember]
        public bool IsExpired
        {
            get { return !String.IsNullOrWhiteSpace(Value) && Expiry > 0 && DateTime.Compare(Expiry.FromUnixTime(), DateTime.UtcNow) < 1; }
        }

        /// <summary>
        /// True if the token must always pass over ssl.
        /// </summary>
        [DataMember(Name = "ssl")]
        public bool AlwaysUseSsl { get; set; }
    }
}
