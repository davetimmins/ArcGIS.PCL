namespace ArcGIS.ServiceModel.Operation
{
    using Common;
    using System.Runtime.Serialization;
    using System;

    /// <summary>
    /// Represents a request for a query against the info route for a server
    /// </summary>
    [DataContract]
    public class ServerInfo : CommonParameters, IEndpoint
    {
        public string RelativeUrl { get { return Operations.ServerInfoRoute; } }

        public string BuildAbsoluteUrl(string rootUrl)
        {
            if (string.IsNullOrWhiteSpace(rootUrl))
            {
                throw new ArgumentNullException(nameof(rootUrl), "rootUrl is null.");
            }

            return !RelativeUrl.Contains(rootUrl.Substring(6)) && !RelativeUrl.Contains(rootUrl.Substring(6))
                       ? rootUrl + RelativeUrl
                       : RelativeUrl;
        }
    }

    [DataContract]
    public class ServerInfoResponse : PortalResponse
    {
        [DataMember(Name = "currentVersion")]
        public double CurrentVersion { get; set; }

        [DataMember(Name = "fullVersion")]
        public string FullVersion { get; set; }

        [DataMember(Name = "soapUrl")]
        public string SoapUrl { get; set; }

        [DataMember(Name = "secureSoapUrl")]
        public string SecureSoapUrl { get; set; }

        [DataMember(Name = "owningSystemUrl")]
        public string OwningSystemUrl { get; set; }

        [DataMember(Name = "authInfo")]
        public AuthInfo AuthenticationInfo { get; set; }
    }

    [DataContract]
    public class AuthInfo
    {
        [DataMember(Name = "isTokenBasedSecurity")]
        public bool TokenBasedSecurity { get; set; }

        [DataMember(Name = "tokenServicesUrl")]
        public string TokenServicesUrl { get; set; }

        [DataMember(Name = "shortLivedTokenValidity")]
        public int ShortLivedTokenValidity { get; set; }
    }
}
