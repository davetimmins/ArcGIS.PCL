using ArcGIS.ServiceModel.Operation;
using ArcGIS.ServiceModel.Operation.Admin;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ArcGIS.ServiceModel
{
    public class FederatedTokenProvider : ITokenProvider, IDisposable
    {
        HttpClient _httpClient;
        protected readonly GenerateFederatedToken TokenRequest;
        Token _token;

        /// <summary>
        /// Create a token provider to authenticate against an ArcGIS Server that is federated
        /// </summary>
        /// <param name="tokenProvider"></param>
        /// <param name="rootUrl"></param>
        /// <param name="serverUrl"></param>
        /// <param name="serializer">Used to (de)serialize requests and responses</param>
        /// <param name="referer">Referer url to use for the token generation. For federated servers this will be the portal rootUrl</param>
        public FederatedTokenProvider(ITokenProvider tokenProvider, string rootUrl, string serverUrl, ISerializer serializer = null, string referer = null)
        {
            Guard.AgainstNullArgument("tokenProvider", tokenProvider);
            if (string.IsNullOrWhiteSpace(rootUrl)) throw new ArgumentNullException("rootUrl", "rootUrl is null.");
            Guard.AgainstNullArgument("serverUrl", serverUrl);

            Serializer = serializer ?? SerializerFactory.Get();
            if (Serializer == null) throw new ArgumentNullException("serializer", "Serializer has not been set.");

            RootUrl = rootUrl.AsRootUrl();
            _httpClient = HttpClientFactory.Get();
            TokenRequest = new GenerateFederatedToken(serverUrl, tokenProvider) { Referer = referer };
        }

        ~FederatedTokenProvider()
        {
            Dispose(false);
        }

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
            GC.SuppressFinalize(this);
        }

        public ISerializer Serializer { get; private set; }

        public ICryptoProvider CryptoProvider { get { return null; } }

        public string RootUrl { get; private set; }

        public string UserName { get { return null; } }

        public async Task<Token> CheckGenerateToken(CancellationToken ct)
        {
            if (TokenRequest == null) return null;

            if (_token != null && !_token.IsExpired) return _token;

            _token = null; // reset the Token

            TokenRequest.FederatedToken = await TokenRequest.TokenProvider.CheckGenerateToken(ct);

            HttpContent content = new FormUrlEncodedContent(Serializer.AsDictionary(TokenRequest));

            _httpClient.CancelPendingRequests();

            var url = TokenRequest.BuildAbsoluteUrl(RootUrl).Split('?').FirstOrDefault();
            Uri uri;
            bool validUrl = Uri.TryCreate(url, UriKind.Absolute, out uri);
            if (!validUrl)
                throw new HttpRequestException(string.Format("Not a valid url: {0}", url));

            string resultString = string.Empty;
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(url, content, ct);
                response.EnsureSuccessStatusCode();

                resultString = await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException)
            {
                return null;
            }

            var result = Serializer.AsPortalResponse<Token>(resultString);

            if (result.Error != null) throw new InvalidOperationException(result.Error.ToString());

            _token = result;
            return _token;
        }
    }

    public class ArcGISOnlineFederatedTokenProvider : FederatedTokenProvider
    {
        public ArcGISOnlineFederatedTokenProvider(ITokenProvider tokenProvider, string serverUrl, ISerializer serializer = null, string referer = "https://www.arcgis.com")
            : base(tokenProvider, PortalGateway.AGOPortalUrl, serverUrl, serializer, referer)
        { }
    }

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
        public ArcGISOnlineAppLoginOAuthProvider(string clientId, string clientSecret, ISerializer serializer = null)
        {
            Guard.AgainstNullArgument("clientId", clientId);
            Guard.AgainstNullArgument("clientSecret", clientSecret);

            Serializer = serializer ?? SerializerFactory.Get();
            if (Serializer == null) throw new ArgumentNullException("serializer", "Serializer has not been set.");
            OAuthRequest = new GenerateOAuthToken(clientId, clientSecret);

            _httpClient = HttpClientFactory.Get();
        }

        ~ArcGISOnlineAppLoginOAuthProvider()
        {
            Dispose(false);
        }

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
            GC.SuppressFinalize(this);
        }

        public string RootUrl
        {
            get { return "https://www.arcgis.com/sharing/oauth2/token"; }
        }

        public string UserName { get { return null; } }

        public ISerializer Serializer { get; private set; }

        public ICryptoProvider CryptoProvider { get { return null; } }

        public async Task<Token> CheckGenerateToken(CancellationToken ct)
        {
            if (OAuthRequest == null) return null;

            if (_token != null && !_token.IsExpired) return _token;

            _token = null; // reset the Token

            HttpContent content = new FormUrlEncodedContent(Serializer.AsDictionary(OAuthRequest));

            _httpClient.CancelPendingRequests();

            string resultString = string.Empty;
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(RootUrl, content, ct);
                response.EnsureSuccessStatusCode();

                resultString = await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException)
            {
                return null;
            }

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
        public ArcGISOnlineTokenProvider(string username, string password, ISerializer serializer = null, string referer = "https://www.arcgis.com")
            : base(PortalGateway.AGOPortalUrl, username, password, serializer, referer)
        {
            CanAccessPublicKeyEndpoint = false;
            TokenRequest.IsFederated = true;
        }
    }

    /// <summary>
    /// Provides a token for ArcGIS Server when it is federated with Portal for ArcGIS
    /// </summary>
    public class ServerFederatedWithPortalTokenProvider : TokenProvider
    {
        /// <summary>
        /// Create a token provider to authenticate against an ArcGIS Server federated with Portal for ArcGIS
        /// </summary>
        /// <param name="rootUrl"></param>
        /// <param name="username">Portal for ArcGIS user name</param>
        /// <param name="password">Portal for ArcGIS user password</param>
        /// <param name="serializer">Used to (de)serialize requests and responses</param>
        /// <param name="referer">Referer url to use for the token generation. For federated servers this will be the rootUrl + '/rest'</param>
        public ServerFederatedWithPortalTokenProvider(string rootUrl, string username, string password, ISerializer serializer = null, string referer = null)
            : base(rootUrl, username, password, serializer, referer)
        {
            TokenRequest.IsFederated = true;
            if (string.IsNullOrWhiteSpace(referer)) TokenRequest.Referer = rootUrl + "/rest";
        }
    }

    /// <summary>
    /// ArcGIS Server token provider
    /// </summary>
    public class TokenProvider : ITokenProvider, IDisposable
    {
        HttpClient _httpClient;
        protected GenerateToken TokenRequest;
        Token _token;
        PublicKeyResponse _publicKey;
        protected bool CanAccessPublicKeyEndpoint = true;

        /// <summary>
        /// Create a token provider to authenticate against ArcGIS Server
        /// </summary>
        /// <param name="rootUrl">Made up of scheme://host:port/site</param>
        /// <param name="username">ArcGIS Server user name</param>
        /// <param name="password">ArcGIS Server user password</param>
        /// <param name="serializer">Used to (de)serialize requests and responses</param>
        /// <param name="referer">Referer url to use for the token generation</param>
        /// <param name="useEncryption">If true then the token generation request will be encryted</param>
        public TokenProvider(string rootUrl, string username, string password, ISerializer serializer = null, string referer = "", ICryptoProvider cryptoProvider = null)
        {
            if (string.IsNullOrWhiteSpace(rootUrl)) throw new ArgumentNullException("rootUrl", "rootUrl is null.");
            Guard.AgainstNullArgument("username", username);
            Guard.AgainstNullArgument("password", password);

            Serializer = serializer ?? SerializerFactory.Get();
            if (Serializer == null) throw new ArgumentNullException("serializer", "Serializer has not been set.");
            RootUrl = rootUrl.AsRootUrl();
            CryptoProvider = cryptoProvider ?? CryptoProviderFactory.Get();
            TokenRequest = new GenerateToken(username, password) { Referer = referer };
            UserName = username;

            _httpClient = HttpClientFactory.Get();
        }

        ~TokenProvider()
        {
            Dispose(false);
        }

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
            GC.SuppressFinalize(this);
        }

        public ICryptoProvider CryptoProvider { get; private set; }

        public string RootUrl { get; private set; }

        public string UserName { get; private set; }

        public ISerializer Serializer { get; private set; }

        void CheckRefererHeader()
        {
            if (_httpClient == null || string.IsNullOrWhiteSpace(TokenRequest.Referer)) return;

            Uri referer;
            bool validReferrerUrl = Uri.TryCreate(TokenRequest.Referer, UriKind.Absolute, out referer);
            if (!validReferrerUrl)
                throw new HttpRequestException(string.Format("Not a valid url for referrer: {0}", TokenRequest.Referer));
            _httpClient.DefaultRequestHeaders.Referrer = referer;
        }

        //Token _token;
        /// <summary>
        /// Generates a token using the username and password set for this provider.
        /// </summary>
        /// <returns>The generated token or null if not applicable</returns>
        /// <remarks>This sets the Token property for the provider. It will be auto appended to 
        /// any requests sent through the gateway used by this provider.</remarks>
        public async Task<Token> CheckGenerateToken(CancellationToken ct)
        {
            if (TokenRequest == null) return null;

            if (_token != null && !_token.IsExpired) return _token;

            _token = null; // reset the Token
            _publicKey = null;

            CheckRefererHeader();

            var url = TokenRequest.BuildAbsoluteUrl(RootUrl).Split('?').FirstOrDefault();
            Uri uri;
            bool validUrl = Uri.TryCreate(url, UriKind.Absolute, out uri);
            if (!validUrl)
                throw new HttpRequestException(string.Format("Not a valid url: {0}", url));

            if (CryptoProvider != null && _publicKey == null && CanAccessPublicKeyEndpoint)
            {
                var publicKey = new PublicKey();
                var encryptionInfoEndpoint = publicKey.BuildAbsoluteUrl(RootUrl) + PortalGateway.AsRequestQueryString(Serializer, publicKey);

                string publicKeyResultString = null;
                try
                {
                    _httpClient.CancelPendingRequests();
                    HttpResponseMessage response = await _httpClient.GetAsync(encryptionInfoEndpoint, ct);
                    response.EnsureSuccessStatusCode();
                    publicKeyResultString = await response.Content.ReadAsStringAsync();
                }
                catch (TaskCanceledException)
                {
                    return null;
                }
                catch (HttpRequestException)
                {
                    CanAccessPublicKeyEndpoint = false;
                }

                if (ct.IsCancellationRequested) return null;

                if (CanAccessPublicKeyEndpoint)
                {
                    _publicKey = Serializer.AsPortalResponse<PublicKeyResponse>(publicKeyResultString);
                    if (_publicKey.Error != null) throw new InvalidOperationException(_publicKey.Error.ToString());

                    TokenRequest = CryptoProvider.Encrypt(TokenRequest, _publicKey.Exponent, _publicKey.Modulus);
                }
            }

            if (ct.IsCancellationRequested) return null;
            HttpContent content = new FormUrlEncodedContent(Serializer.AsDictionary(TokenRequest));

            _httpClient.CancelPendingRequests();

            string resultString = string.Empty;
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(uri, content, ct);
                response.EnsureSuccessStatusCode();

                resultString = await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException)
            {
                return null;
            }

            var result = Serializer.AsPortalResponse<Token>(resultString);

            if (result.Error != null) throw new InvalidOperationException(result.Error.ToString());

            if (!string.IsNullOrWhiteSpace(TokenRequest.Referer)) result.Referer = TokenRequest.Referer;

            _token = result;
            return _token;
        }
    }
}
