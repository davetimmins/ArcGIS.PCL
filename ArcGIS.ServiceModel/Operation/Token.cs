using System;
using System.Runtime.Serialization;
using ArcGIS.ServiceModel.Common;
using System.Text;

namespace ArcGIS.ServiceModel.Operation
{
    /// <summary>
    /// This operation generates an access token in exchange for user credentials that can be used by clients when working with the ArcGIS Portal API. 
    /// The call is only allowed over HTTPS and must be a POST.
    /// </summary>
    [DataContract]
    public class GenerateToken : CommonParameters, IEndpoint
    {
        public GenerateToken(String username, String password)
        {
            Username = username;
            Password = password;
            ExpirationInMinutes = 60;
        }

        String _client;
        /// <summary>
        /// The client identification type for which the token is to be granted.
        /// </summary>
        /// <remarks>The default value is referer. Setting it to null will also set the Referer to null</remarks>
        [DataMember(Name = "client")]
        public String Client { get { return _client; } set { _client = value; if (String.IsNullOrWhiteSpace(_client)) Referer = null; } }

        String _referer;
        /// <summary>
        /// The base URL of the web app that will invoke the Portal API. 
        /// This parameter must be specified if the value of the client parameter is referer.
        /// </summary>
        [DataMember(Name = "referer")]
        public String Referer { get { return _referer; } set { _referer = value; if (!String.IsNullOrWhiteSpace(_referer)) Client = "referer"; } }

        /// <summary>
        /// Username of user who wants to get a token.
        /// </summary>
        [DataMember(Name = "username")]
        public String Username { get; private set; }

        /// <summary>
        /// Password of user who wants to get a token.
        /// </summary>
        [DataMember(Name = "password")]
        public String Password { get; private set; }

        [DataMember(Name = "encrypted")]
        public bool Encrypted { get; private set; }

        /// <summary>
        /// The token expiration time in minutes.
        /// </summary>
        /// <remarks> The default is 60 minutes.</remarks>
        [IgnoreDataMember]
        public int ExpirationInMinutes { get; set; }

        String _expiration;
        [DataMember(Name = "expiration")]
        public String Expiration { get { return String.IsNullOrWhiteSpace(_expiration) ? ExpirationInMinutes.ToString() : _expiration; } }

        /// <summary>
        /// Set this to true to prevent the BuildAbsoluteUrl returning https as the default scheme
        /// </summary>
        [IgnoreDataMember]
        public bool DontForceHttps { get; set; }

        public void Encrypt(String username, String password, String expiration = "", String client = "")
        {
            if (String.IsNullOrWhiteSpace(username)) throw new ArgumentNullException("username");
            if (String.IsNullOrWhiteSpace(password)) throw new ArgumentNullException("password");
            Username = username;
            Password = password;
            if (!String.IsNullOrWhiteSpace(expiration)) _expiration = expiration;
            if (!String.IsNullOrWhiteSpace(client)) Client = client;
            Encrypted = true;
            DontForceHttps = false;
        }

        public String RelativeUrl
        {
            get { return "tokens/" + Operations.GenerateToken; }
        }

        public String BuildAbsoluteUrl(String rootUrl)
        {
            if (String.IsNullOrWhiteSpace(rootUrl)) throw new ArgumentNullException("rootUrl");

            return (String.Equals(rootUrl, "http://www.arcgis.com/sharing/rest/", StringComparison.OrdinalIgnoreCase))
                ? (DontForceHttps ? rootUrl : rootUrl.Replace("http://", "https://")) + RelativeUrl.Replace("tokens/", "")
                : (DontForceHttps ? rootUrl : rootUrl.Replace("http://", "https://")) + RelativeUrl;
        }
    }

    /// <summary>
    /// Request for generating a token from the hosted OAuth provider on ArcGIS Online  
    /// that can be used to access credit-based ArcGIS Online services
    /// </summary>
    [DataContract]
    public class GenerateOAuthToken : CommonParameters
    {
        public GenerateOAuthToken(String clientId, String clientSecret)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            ExpirationInMinutes = 120;
        }

        /// <summary>
        /// The Client Id from your API access section of your application from developers.arcgis.com
        /// </summary>
        [DataMember(Name = "client_id")]
        public String ClientId { get; private set; }

        /// <summary>
        /// The Client Secret from your API access section of your application from developers.arcgis.com
        /// </summary>
        [DataMember(Name = "client_secret")]
        public String ClientSecret { get; private set; }

        [DataMember(Name = "grant_type")]
        public virtual String Type { get { return "client_credentials"; } }

        /// <summary>
        /// The token expiration time in minutes.
        /// </summary>
        /// <remarks> The default is 120 minutes. Maximum value is 14 days (20160 minutes)</remarks>
        [DataMember(Name = "expiration")]
        public int ExpirationInMinutes { get; set; }
    }

    [DataContract]
    public class OAuthToken : PortalResponse
    {
        [DataMember(Name = "access_token")]
        public String Value { get; set; }

        /// <summary>
        /// The expiration time of the token in seconds.
        /// </summary>
        [DataMember(Name = "expires_in")]
        public long Expiry { get; set; }

        public Token AsToken()
        {
            return new Token
            {
                Value = Value,
                Error = Error,
                AlwaysUseSsl = true,
                Expiry = DateTime.UtcNow.AddSeconds(Expiry).ToUnixTime()
            };
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

        [IgnoreDataMember]
        public String Referer { get; set; }

        /// <summary>
        /// True if the token must always pass over ssl.
        /// </summary>
        [DataMember(Name = "ssl")]
        public bool AlwaysUseSsl { get; set; }
    }
}
