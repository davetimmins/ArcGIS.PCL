using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel.Extensions;
using ArcGIS.ServiceModel.Operation;
using System.Net.Http.Headers;

namespace ArcGIS.ServiceModel.Logic
{
    public interface IPortalGateway
    {
        /// <summary>
        /// Made up of scheme://host:port/site
        /// </summary>
        String RootUrl { get; }
        Token Token { get; }
        ISerializer Serializer { get; set; }
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

    public abstract class PortalGateway : IPortalGateway, IDisposable
    {
        internal const String AGOPortalUrl = "http://www.arcgis.com/sharing/rest/";
        protected readonly GenerateToken TokenRequest;
        HttpClientHandler _httpClientHandler;
        HttpClient _httpClient;

        protected PortalGateway()
            : this(AGOPortalUrl, String.Empty, String.Empty)
        { }

        protected PortalGateway(String rootUrl)
            : this(rootUrl, String.Empty, String.Empty)
        { }

        protected PortalGateway(String username, String password)
            : this(AGOPortalUrl, username, password)
        { }

        protected PortalGateway(String rootUrl, String username, String password)
        {
            if (String.IsNullOrWhiteSpace(rootUrl)) throw new ArgumentNullException("rootUrl");
            rootUrl = rootUrl.TrimEnd('/');
            rootUrl = rootUrl.Replace("/rest/services", "");
            RootUrl = rootUrl.ToLower() + '/';

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

            if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password))
                TokenRequest = new GenerateToken { Username = username, Password = password };
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
            GC.SuppressFinalize(this);
        }

        public string RootUrl { get; private set; }

        public Token Token { get; private set; }

        public ISerializer Serializer { get; set; }

        /// <summary>
        /// Generates a token if a username and password have been set for this gateway.
        /// </summary>
        /// <returns>The generated token or null if not applicable</returns>
        /// <remarks>This sets the Token property for the gateway. It will be auto appended to 
        /// any requests sent through the gateway.</remarks>
        protected async Task<Token> CheckGenerateToken()
        {
            if (Serializer == null) throw new NullReferenceException("Serializer has not been set.");

            if (TokenRequest == null) return null;
            if (Token != null && !Token.IsExpired) return Token;

            Token = null; // reset the Token
            Token = await Post<Token>(TokenRequest, Serializer.AsDictionary(TokenRequest));
            return Token;
        }

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

        protected Task<T> Get<T, TRequest>(TRequest requestObject)
            where TRequest : CommonParameters, IEndpoint
            where T : IPortalResponse
        {
            return Get<T>((requestObject.RelativeUrl + AsRequestQueryString(requestObject)).AsEndpoint());
        }

        protected async Task<T> Get<T>(IEndpoint endpoint) where T : IPortalResponse
        {
            if (Serializer == null) throw new NullReferenceException("Serializer has not been set.");

            await CheckGenerateToken();

            var url = endpoint.BuildAbsoluteUrl(RootUrl);
            if (!url.Contains("f=")) url += (url.Contains("?") ? "&" : "?") + "f=json";
            if (Token != null && !String.IsNullOrWhiteSpace(Token.Value) && !url.Contains("token="))
            {
                url += (url.Contains("?") ? "&" : "?") + "token=" + Token.Value;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Token.Value);
                if (Token.AlwaysUseSsl) url = url.Replace("http:", "https:");
            }            

            // use POST if request is too long
            if (url.Length > 2082)
                return await Post<T>(endpoint, ParseQueryString(endpoint.RelativeUrl));

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
            if (Serializer == null) throw new NullReferenceException("Serializer has not been set.");

            return Post<T>(requestObject, Serializer.AsDictionary(requestObject));
        }

        protected async Task<T> Post<T>(IEndpoint endpoint, Dictionary<String, String> parameters) where T : IPortalResponse
        {
            if (Serializer == null) throw new NullReferenceException("Serializer has not been set.");

            var url = endpoint.BuildAbsoluteUrl(RootUrl).Split('?').FirstOrDefault();

            // these should have already been added
            if (!parameters.ContainsKey("f")) parameters.Add("f", "json");
            if (!parameters.ContainsKey("token") && Token != null && !String.IsNullOrWhiteSpace(Token.Value))
            {
                parameters.Add("token", Token.Value);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Token.Value);
                if (Token.AlwaysUseSsl) url = url.Replace("http:", "https:");
            }            

            HttpContent content = new FormUrlEncodedContent(parameters);            
            _httpClient.CancelPendingRequests();
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var result = Serializer.AsPortalResponse<T>(await response.Content.ReadAsStringAsync());

            if (result.Error != null) throw new InvalidOperationException(result.Error.ToString());

            return result;
        }

        String AsRequestQueryString<T>(T objectToConvert) where T : CommonParameters
        {
            if (Serializer == null) throw new NullReferenceException("Serializer has not been set.");

            var dictionary = Serializer.AsDictionary(objectToConvert);

            return "?" + String.Join("&", dictionary.Keys.Select(k => String.Format("{0}={1}", k, dictionary[k].UrlEncode())));
        }

        static Dictionary<String, String> ParseQueryString(String queryString)
        {
            if (String.IsNullOrWhiteSpace(queryString)) return new Dictionary<String, String>();

            // remove anything other than query string from url
            if (queryString.Contains("?"))
                queryString = queryString.Substring(queryString.IndexOf('?') + 1);

            return Regex.Split(queryString, "&")
                .Select(vp => Regex.Split(vp, "="))
                .ToDictionary(singlePair => singlePair[0], singlePair => singlePair.Length == 2 ? singlePair[1] : String.Empty);
        }
    }
}
