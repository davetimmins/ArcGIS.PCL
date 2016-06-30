namespace ArcGIS.ServiceModel
{
    using System;
    using System.Net.Http;

    /// <summary>
    /// Provides a secure ArcGIS Server gateway where the token service is at the same root url
    /// </summary>
    public class SecurePortalGateway : PortalGateway
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="rootUrl">The root url of ArcGIS Server and the token service</param>
        /// <param name="username">ArcGIS Server user name</param>
        /// <param name="password">ArcGIS Server user password</param>
        /// <param name="serializer">Used to (de)serialize requests and responses</param>
        /// <param name="httpClientFunc">Function that resolves to a HTTP client used to send requests</param>
        public SecurePortalGateway(string rootUrl, string username, string password, ISerializer serializer = null, Func<HttpClient> httpClientFunc = null)
            : base(rootUrl, serializer,
            (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                ? null
                : new TokenProvider(rootUrl, username, password, serializer),
            httpClientFunc)
        { }
    }
}
