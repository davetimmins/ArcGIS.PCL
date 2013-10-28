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
    /// Used for generating a token which can then be appended to requests made through a gateway
    /// </summary>
    public interface ITokenProvider
    {
        /// <summary>
        /// Made up of scheme://host:port/site
        /// </summary>
        String RootUrl { get; }

        Token Token { get; }

        ISerializer Serializer { get; }
    }

    /// <summary>
    /// ArcGIS Online token provider
    /// </summary>
    public sealed class ArcGISOnlineTokenProvider : TokenProvider
    {
        /// <summary>
        /// Create a token provider to authenticate against ArcGIS Online
        /// </summary>
        /// <param name="username">ArcGIS Online user name</param>
        /// <param name="password">ArcGIS Online user password</param>
        /// <param name="serializer">Used to (de)serialize requests and responses</param>
        public ArcGISOnlineTokenProvider(String username, String password, ISerializer serializer)
            : base(PortalGateway.AGOPortalUrl, username, password, serializer)
        { }
    }

    /// <summary>
    /// ArcGIS Server token provider
    /// </summary>
    public class TokenProvider : ITokenProvider, IDisposable
    {
        HttpClientHandler _httpClientHandler;
        HttpClient _httpClient;
        protected readonly GenerateToken TokenRequest;

        /// <summary>
        /// Create a token provider to authenticate against ArcGIS Server
        /// </summary>
        /// <param name="rootUrl">Made up of scheme://host:port/site</param>
        /// <param name="username">ArcGIS Server user name</param>
        /// <param name="password">ArcGIS Server user password</param>
        /// <param name="serializer">Used to (de)serialize requests and responses</param>
        public TokenProvider(String rootUrl, String username, String password, ISerializer serializer)
        {
            RootUrl = rootUrl.AsRootUrl();
            Serializer = serializer;
            if (Serializer == null) throw new ArgumentNullException("serializer", "Serializer has not been set.");
            TokenRequest = new GenerateToken(username, password);

            _httpClientHandler = new HttpClientHandler();
            if (_httpClientHandler.SupportsAutomaticDecompression)
                _httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            if (_httpClientHandler.SupportsUseProxy()) _httpClientHandler.UseProxy = true;
            if (_httpClientHandler.SupportsAllowAutoRedirect()) _httpClientHandler.AllowAutoRedirect = true;
            if (_httpClientHandler.SupportsPreAuthenticate()) _httpClientHandler.PreAuthenticate = true;

            _httpClient = new HttpClient(_httpClientHandler);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

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
                if (_httpClientHandler != null)
                {
                    _httpClientHandler.Dispose();
                    _httpClientHandler = null;
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

        Token _token;
        public Token Token
        {
            get
            {
                return (_token != null && !_token.IsExpired) ?
                    _token :
                    CheckGenerateToken().Result;
            }
        }

        public ISerializer Serializer { get; private set; }

        /// <summary>
        /// Generates a token using the username and password set for this provider.
        /// </summary>
        /// <returns>The generated token or null if not applicable</returns>
        /// <remarks>This sets the Token property for the provider. It will be auto appended to 
        /// any requests sent through the gateway used by this provider.</remarks>
        protected async Task<Token> CheckGenerateToken()
        {
            if (TokenRequest == null) return null;

            _token = null; // reset the Token

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

            _token = result;
            return _token;
        }
    }
}
