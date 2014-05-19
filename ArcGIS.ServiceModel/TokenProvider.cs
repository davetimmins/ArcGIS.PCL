using ArcGIS.ServiceModel.Operation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ArcGIS.ServiceModel
{
    /// <summary>
    /// ArcGIS Online application login type OAuth 2.0 token provider
    /// </summary>
    public class ArcGISOnlineAppLoginOAuthProvider : ITokenProvider, IDisposable
    {
        HttpClient _httpClient;
        protected readonly GenerateOAuthToken OAuthRequest;
        Token _token;

        /// <summary>
        /// Create an OAuth token provider to authenticate against ArcGIS Online
        /// </summary>
        /// <param name="clientId">The Client Id from your API access section of your application from developers.arcgis.com</param>
        /// <param name="clientSecret">The Client Secret from your API access section of your application from developers.arcgis.com</param>
        /// <param name="serializer">Used to (de)serialize requests and responses</param>
        public ArcGISOnlineAppLoginOAuthProvider(String clientId, String clientSecret, ISerializer serializer = null)            
        {
            if (String.IsNullOrWhiteSpace(clientId) || String.IsNullOrWhiteSpace(clientSecret))
            {
                System.Diagnostics.Debug.WriteLine("ArcGISOnlineAppLoginOAuthProvider not initialized as client Id/secret not supplied.");
                return;
            }
            
            Serializer = serializer ?? SerializerFactory.Get();
            if (Serializer == null) throw new ArgumentNullException("serializer", "Serializer has not been set.");
            OAuthRequest = new GenerateOAuthToken(clientId, clientSecret);

            _httpClient = HttpClientFactory.Get();

            System.Diagnostics.Debug.WriteLine("Created TokenProvider for " + RootUrl);
        }

        #if DEBUG
        ~ArcGISOnlineAppLoginOAuthProvider()
        {
            Dispose(false);
        }
#endif

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_httpClient != null)
                {
                    _httpClient.Dispose();
                    _httpClient = null;
                }
                _token = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
#if DEBUG
            GC.SuppressFinalize(this);
#endif
        }

        public string RootUrl
        {
            get { return "https://www.arcgis.com/sharing/oauth2/token"; }
        }

        public ISerializer Serializer { get; private set; }

        public async Task<Token> CheckGenerateToken()
        {
            if (OAuthRequest == null) return null;

            if (_token != null && !_token.IsExpired) return _token;

            _token = null; // reset the Token            

            HttpContent content = new FormUrlEncodedContent(Serializer.AsDictionary(OAuthRequest));

            _httpClient.CancelPendingRequests();
            HttpResponseMessage response = await _httpClient.PostAsync(RootUrl, content);
            response.EnsureSuccessStatusCode();

            var resultString = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine("Generate OAuth token result: " + resultString);
            var result = Serializer.AsPortalResponse<OAuthToken>(resultString);

            if (result.Error != null) throw new InvalidOperationException(result.Error.ToString());
            
            _token = result.AsToken();
            return _token;
        }
    }

    /// <summary>
    /// ArcGIS Online token provider
    /// </summary>
    public class ArcGISOnlineTokenProvider : TokenProvider
    {
        /// <summary>
        /// Create a token provider to authenticate against ArcGIS Online
        /// </summary>
        /// <param name="username">ArcGIS Online user name</param>
        /// <param name="password">ArcGIS Online user password</param>
        /// <param name="serializer">Used to (de)serialize requests and responses</param>
        /// <param name="referer">Referer url to use for the token generation</param>
        public ArcGISOnlineTokenProvider(String username, String password, ISerializer serializer = null, String referer = "https://www.arcgis.com")
            : base(PortalGateway.AGOPortalUrl, username, password, serializer, referer)
        { }
    }

    /// <summary>
    /// ArcGIS Server token provider
    /// </summary>
    public class TokenProvider : ITokenProvider, IDisposable
    {
        HttpClient _httpClient;
        protected readonly GenerateToken TokenRequest;
        Token _token;

        /// <summary>
        /// Create a token provider to authenticate against ArcGIS Server
        /// </summary>
        /// <param name="rootUrl">Made up of scheme://host:port/site</param>
        /// <param name="username">ArcGIS Server user name</param>
        /// <param name="password">ArcGIS Server user password</param>
        /// <param name="serializer">Used to (de)serialize requests and responses</param>
        /// <param name="referer">Referer url to use for the token generation</param>
        public TokenProvider(String rootUrl, String username, String password, ISerializer serializer = null, String referer = "")
        {
            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
            {
                System.Diagnostics.Debug.WriteLine("TokenProvider for '" + RootUrl + "' not initialized as username/password not supplied.");
                return;
            }
            
            Serializer = serializer ?? SerializerFactory.Get();
            if (Serializer == null) throw new ArgumentNullException("serializer", "Serializer has not been set.");
            RootUrl = rootUrl.AsRootUrl();
            TokenRequest = new GenerateToken(username, password) { Referer = referer };
                      
            _httpClient = HttpClientFactory.Get();                 

            System.Diagnostics.Debug.WriteLine("Created TokenProvider for " + RootUrl);
        }

#if DEBUG
        ~TokenProvider()
        {
            Dispose(false);
        }
#endif

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_httpClient != null)
                {
                    _httpClient.Dispose();
                    _httpClient = null;
                }
                _token = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
#if DEBUG
            GC.SuppressFinalize(this);
#endif
        }

        public string RootUrl { get; private set; }

        public ISerializer Serializer { get; private set; }

        void CheckRefererHeader()
        {
            if (_httpClient == null || String.IsNullOrWhiteSpace(TokenRequest.Referer)) return;

            Uri referer;
            bool validReferrerUrl = Uri.TryCreate(TokenRequest.Referer, UriKind.Absolute, out referer);
            if (!validReferrerUrl)
                throw new HttpRequestException(String.Format("Not a valid url for referrer: {0}", TokenRequest.Referer));
            _httpClient.DefaultRequestHeaders.Referrer = referer;
        }
        
        //Token _token;
        /// <summary>
        /// Generates a token using the username and password set for this provider.
        /// </summary>
        /// <returns>The generated token or null if not applicable</returns>
        /// <remarks>This sets the Token property for the provider. It will be auto appended to 
        /// any requests sent through the gateway used by this provider.</remarks>
        public async Task<Token> CheckGenerateToken()
        {
            if (TokenRequest == null) return null;

            if (_token != null && !_token.IsExpired) return _token;
            
            _token = null; // reset the Token

            CheckRefererHeader();

            var url = TokenRequest.BuildAbsoluteUrl(RootUrl).Split('?').FirstOrDefault();
            Uri uri;
            bool validUrl = Uri.TryCreate(url, UriKind.Absolute, out uri);
            if (!validUrl)
                throw new HttpRequestException(String.Format("Not a valid url: {0}", url));

            HttpContent content = new FormUrlEncodedContent(Serializer.AsDictionary(TokenRequest));

            _httpClient.CancelPendingRequests();
            HttpResponseMessage response = await _httpClient.PostAsync(uri, content);
            response.EnsureSuccessStatusCode();

            var resultString = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine("Generate token result: " + resultString);
            var result = Serializer.AsPortalResponse<Token>(resultString);

            if (result.Error != null) throw new InvalidOperationException(result.Error.ToString());

            if (!String.IsNullOrWhiteSpace(TokenRequest.Referer)) result.Referer = TokenRequest.Referer;

            _token = result;
            return _token;
        }
    }
}
