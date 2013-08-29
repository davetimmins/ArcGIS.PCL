using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel.Operation;
using System.Net.Http.Headers;

namespace ArcGIS.ServiceModel
{
    public interface IPortalGateway
    {
        /// <summary>
        /// Made up of scheme://host:port/site
        /// </summary>
        String RootUrl { get; }

        ITokenProvider TokenProvider { get; set; }

        ISerializer Serializer { get; }
    }

    /// <summary>
    /// Used for (de)serializtion of requests and responses. 
    /// </summary>
    /// <remarks>Split out as interface to allow injection. Also moves implementation out of this
    /// library so it can use whatever framework the developer wants.</remarks>
    public interface ISerializer
    {
        /// <summary>
        /// Convert an object into a dictionary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToConvert"></param>
        /// <returns></returns>
        Dictionary<String, String> AsDictionary<T>(T objectToConvert) where T : CommonParameters;

        /// <summary>
        /// Deserialize string as a <see cref="IPortalResponse"/>
        /// </summary>
        /// <typeparam name="T">The type of the result from the call</typeparam>
        /// <param name="dataToConvert">Json string to deserialize</param>
        /// <returns></returns>
        T AsPortalResponse<T>(String dataToConvert) where T : IPortalResponse;
    }

    /// <summary>
    /// ArcGIS Online gateway
    /// </summary>
    public class ArcGISOnlineGateway : PortalGateway
    {
        /// <summary>
        /// Create an ArcGIS Online gateway to access non secure resources
        /// </summary>
        /// <param name="serializer">Used to (de)serialize requests and responses</param>
        public ArcGISOnlineGateway(ISerializer serializer)
            : base(PortalGateway.AGOPortalUrl, serializer, null)
        { }

        /// <summary>
        /// Create an ArcGIS Online gateway to access secure resources
        /// </summary>
        /// <param name="serializer">Used to (de)serialize requests and responses</param>
        /// <param name="tokenProvider">Provide access to a token for secure resources</param>
        public ArcGISOnlineGateway(ISerializer serializer, ArcGISOnlineTokenProvider tokenProvider)
            : base(PortalGateway.AGOPortalUrl, serializer, tokenProvider)
        { }
    }

    /// <summary>
    /// Provides a secure ArcGIS Server gateway where the token service is at the same root url
    /// </summary>
    public abstract class SecureArcGISServerGateway : PortalGateway
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootUrl">The root url of ArcGIS Server and the token service</param>
        /// <param name="username">ArcGIS Server user name</param>
        /// <param name="password">ArcGIS Server user password</param>
        /// <param name="serializer">Used to (de)serialize requests and responses</param>
        protected SecureArcGISServerGateway(String rootUrl, String username, String password, ISerializer serializer)
            : base(rootUrl, serializer, new TokenProvider(rootUrl, username, password, serializer))
        { }
    }


    /// <summary>
    /// ArcGIS Server gateway
    /// </summary>
    public abstract class PortalGateway : IPortalGateway, IDisposable
    {
        internal const String AGOPortalUrl = "http://www.arcgis.com/sharing/rest/";
        HttpClientHandler _httpClientHandler;
        HttpClient _httpClient;

        /// <summary>
        /// Create an ArcGIS Server gateway to access non secure resources
        /// </summary>
        /// <param name="rootUrl">Made up of scheme://host:port/site</param>
        /// <param name="serializer">Used to (de)serialize requests and responses</param>
        protected PortalGateway(String rootUrl, ISerializer serializer)
            : this(rootUrl, serializer, null)
        { }

        /// <summary>
        /// Create an ArcGIS Server gateway to access secure resources
        /// </summary>
        /// <param name="rootUrl">Made up of scheme://host:port/site</param>
        /// <param name="serializer">Used to (de)serialize requests and responses</param>
        /// <param name="tokenProvider">Provide access to a token for secure resources</param>
        protected PortalGateway(String rootUrl, ISerializer serializer, ITokenProvider tokenProvider)
        {
            RootUrl = rootUrl.AsRootUrl();
            TokenProvider = tokenProvider;
            Serializer = serializer;
            if (Serializer == null) throw new ArgumentNullException("serializer", "Serializer has not been set.");

            _httpClientHandler = new HttpClientHandler();
            if (_httpClientHandler.SupportsAutomaticDecompression)
                _httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            if (_httpClientHandler.SupportsUseProxy()) _httpClientHandler.UseProxy = true;
            if (_httpClientHandler.SupportsAllowAutoRedirect()) _httpClientHandler.AllowAutoRedirect = true;
            if (_httpClientHandler.SupportsPreAuthenticate()) _httpClientHandler.PreAuthenticate = true;

            _httpClient = new HttpClient(_httpClientHandler);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jsonp"));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        }

#if DEBUG
        ~PortalGateway()
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

        public ITokenProvider TokenProvider { get; set; }

        public ISerializer Serializer { get; private set; }

        /// <summary>
        /// Recursively parses an ArcGIS Server site and discovers the resources available
        /// </summary>
        /// <returns>An ArcGIS Server site hierarchy</returns>
        public async Task<SiteDescription> DescribeSite()
        {
            var result = new SiteDescription();

            var folderDescriptions = await DescribeEndpoint("/".AsEndpoint());

            foreach (var description in folderDescriptions)
            {
                if (description.Version > result.Version) result.Version = description.Version;

                foreach (var service in description.Services)
                {
                    result.Resources.Add(new ArcGISServerEndpoint(String.Format("{0}/{1}", service.Name, service.Type)));
                }
            }

            return result;
        }

        async Task<List<SiteFolderDescription>> DescribeEndpoint(IEndpoint endpoint)
        {
            var result = new List<SiteFolderDescription>();

            var folderDescription = await Get<SiteFolderDescription>(endpoint);
            folderDescription.Path = endpoint.RelativeUrl;

            result.Add(folderDescription);

            foreach (var folder in folderDescription.Folders)
            {
                result.AddRange(await DescribeEndpoint((endpoint.RelativeUrl + folder).AsEndpoint()));
            }

            return result;
        }

        /// <summary>
        /// Pings the endpoint specified
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns>HTTP error if there is a problem with the request, otherwise an
        /// empty <see cref="IPortalResponse"/> object if successful otherwise the Error property is populated</returns>
        public Task<PortalResponse> Ping(IEndpoint endpoint)
        {
            return Get<PortalResponse>(endpoint);
        }

        Token CheckGenerateToken()
        {
            if (TokenProvider == null) return null;

            return TokenProvider.Token;
        }

        protected Task<T> Get<T, TRequest>(TRequest requestObject)
            where TRequest : CommonParameters, IEndpoint
            where T : IPortalResponse
        {
            return Get<T>((requestObject.RelativeUrl + AsRequestQueryString(requestObject)).AsEndpoint());
        }

        protected async Task<T> Get<T>(IEndpoint endpoint) where T : IPortalResponse
        {
            var token = CheckGenerateToken();

            var url = endpoint.BuildAbsoluteUrl(RootUrl);
            if (!url.Contains("f=")) url += (url.Contains("?") ? "&" : "?") + "f=json";
            if (token != null && !String.IsNullOrWhiteSpace(token.Value) && !url.Contains("token="))
            {
                url += (url.Contains("?") ? "&" : "?") + "token=" + token.Value;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token.Value);
                if (token.AlwaysUseSsl) url = url.Replace("http:", "https:");
            }

            // use POST if request is too long
            if (url.Length > 2082)
                return await Post<T>(endpoint, endpoint.RelativeUrl.ParseQueryString());

            _httpClient.CancelPendingRequests();
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = Serializer.AsPortalResponse<T>(await response.Content.ReadAsStringAsync());

            if (result.Error != null) throw new InvalidOperationException(result.Error.ToString());

            return result;
        }

        protected Task<T> Post<T, TRequest>(TRequest requestObject)
            where TRequest : CommonParameters, IEndpoint
            where T : IPortalResponse
        {
            return Post<T>(requestObject, Serializer.AsDictionary(requestObject));
        }

        protected async Task<T> Post<T>(IEndpoint endpoint, Dictionary<String, String> parameters) where T : IPortalResponse
        {
            var url = endpoint.BuildAbsoluteUrl(RootUrl).Split('?').FirstOrDefault();

            var token = CheckGenerateToken();

            // these should have already been added
            if (!parameters.ContainsKey("f")) parameters.Add("f", "json");
            if (!parameters.ContainsKey("token") && token != null && !String.IsNullOrWhiteSpace(token.Value))
            {
                parameters.Add("token", token.Value);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token.Value);
                if (token.AlwaysUseSsl) url = url.Replace("http:", "https:");
            }

            HttpContent content = null;
            try
            {
                content = new FormUrlEncodedContent(parameters);
            }
            catch (FormatException)
            {
                var tempContent = new MultipartFormDataContent();
                foreach (var keyValuePair in parameters)
                {
                    tempContent.Add(new StringContent(keyValuePair.Value), keyValuePair.Key);
                }
                content = tempContent;
            }
            _httpClient.CancelPendingRequests();
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var result = Serializer.AsPortalResponse<T>(await response.Content.ReadAsStringAsync());

            if (result.Error != null) throw new InvalidOperationException(result.Error.ToString());

            return result;
        }

        String AsRequestQueryString<T>(T objectToConvert) where T : CommonParameters
        {
            var dictionary = Serializer.AsDictionary(objectToConvert);

            return "?" + String.Join("&", dictionary.Keys.Select(k => String.Format("{0}={1}", k, dictionary[k].UrlEncode())));
        }
    }
}
