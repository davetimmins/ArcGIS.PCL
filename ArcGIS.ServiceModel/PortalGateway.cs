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
    /// <summary>
    /// ArcGIS Online gateway
    /// </summary>
    public class ArcGISOnlineGateway : PortalGateway
    {       
        /// <summary>
        /// Create an ArcGIS Online gateway to access resources
        /// </summary>
        /// <param name="serializer">Used to (de)serialize requests and responses</param>
        /// <param name="tokenProvider">Provide access to a token for secure resources</param>
        public ArcGISOnlineGateway(ISerializer serializer = null, ArcGISOnlineTokenProvider tokenProvider = null)
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
        protected SecureArcGISServerGateway(String rootUrl, String username, String password, ISerializer serializer = null)
            : base(rootUrl, serializer, new TokenProvider(rootUrl, username, password, serializer))
        { }
    }


    /// <summary>
    /// ArcGIS Server gateway
    /// </summary>
    public abstract class PortalGateway : IPortalGateway, IDisposable
    {
        internal const String AGOPortalUrl = "http://www.arcgis.com/sharing/rest/";
        HttpClient _httpClient;

        /// <summary>
        /// Create an ArcGIS Server gateway to access secure resources
        /// </summary>
        /// <param name="rootUrl">Made up of scheme://host:port/site</param>
        /// <param name="serializer">Used to (de)serialize requests and responses</param>
        /// <param name="tokenProvider">Provide access to a token for secure resources</param>
        protected PortalGateway(String rootUrl, ISerializer serializer = null, ITokenProvider tokenProvider = null)
        {
            RootUrl = rootUrl.AsRootUrl();
            TokenProvider = tokenProvider;
            Serializer = serializer ?? SerializerFactory.Get();
            if (Serializer == null) throw new ArgumentNullException("serializer", "Serializer has not been set.");
                      
            _httpClient = HttpClientFactory.Get();         
            System.Diagnostics.Debug.WriteLine("Created PortalGateway for " + RootUrl);
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

        public ITokenProvider TokenProvider { get; private set; }

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
            SiteFolderDescription folderDescription = null;
            var result = new List<SiteFolderDescription>();
            try
            {
                folderDescription = await Get<SiteFolderDescription>(endpoint);
            }
            catch (HttpRequestException ex) 
            {
                // don't have access to the folder
                System.Diagnostics.Debug.WriteLine("HttpRequestException for Get SiteFolderDescription at path " + endpoint.RelativeUrl);
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return result;
            }

            folderDescription.Path = endpoint.RelativeUrl;            
            result.Add(folderDescription);

            if (folderDescription.Folders != null)
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

        async Task<Token> CheckGenerateToken()
        {
            if (TokenProvider == null) return null;

            var token = await TokenProvider.CheckGenerateToken();

            if (token != null) CheckRefererHeader(token.Referer);
            return token;
        }

        void CheckRefererHeader(String referrer)
        {
            if (_httpClient == null || String.IsNullOrWhiteSpace(referrer)) return;

            Uri referer;
            bool validReferrerUrl = Uri.TryCreate(referrer, UriKind.Absolute, out referer);
            if (!validReferrerUrl)
                throw new HttpRequestException(String.Format("Not a valid url for referrer: {0}", referrer));
            _httpClient.DefaultRequestHeaders.Referrer = referer;
        }

        protected Task<T> Get<T, TRequest>(TRequest requestObject)
            where TRequest : CommonParameters, IEndpoint
            where T : IPortalResponse
        {
            var url = requestObject.BuildAbsoluteUrl(RootUrl) + AsRequestQueryString(requestObject);

            if (url.Length > 2000)
                return Post<T>(requestObject, url.ParseQueryString());

            return Get<T>(url);
        }

        protected async Task<T> Get<T>(String url) where T : IPortalResponse
        {
            var token = await CheckGenerateToken();

            if (!url.Contains("f=")) url += (url.Contains("?") ? "&" : "?") + "f=json";
            if (token != null && !String.IsNullOrWhiteSpace(token.Value) && !url.Contains("token="))
            {
                url += (url.Contains("?") ? "&" : "?") + "token=" + token.Value;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token.Value);
                if (token.AlwaysUseSsl) url = url.Replace("http:", "https:");
            }
            
            Uri uri;
            bool validUrl = Uri.TryCreate(url, UriKind.Absolute, out uri);
            if (!validUrl)
                throw new HttpRequestException(String.Format("Not a valid url: {0}", url));

            _httpClient.CancelPendingRequests();

            System.Diagnostics.Debug.WriteLine(uri);
            HttpResponseMessage response = await _httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            var resultString = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine(resultString);
            var result = Serializer.AsPortalResponse<T>(resultString);
            if (result.Error != null) throw new InvalidOperationException(result.Error.ToString());

            return result;
        }

        protected Task<T> Get<T>(IEndpoint endpoint) where T : IPortalResponse
        {
            return Get<T>(endpoint.BuildAbsoluteUrl(RootUrl));            
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

            var token = await CheckGenerateToken();

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

            Uri uri;
            bool validUrl = Uri.TryCreate(url, UriKind.Absolute, out uri);
            if (!validUrl)
                throw new HttpRequestException(String.Format("Not a valid url: {0}", url));

            System.Diagnostics.Debug.WriteLine(uri);
            HttpResponseMessage response = await _httpClient.PostAsync(uri, content);
            response.EnsureSuccessStatusCode();

            var resultString = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine(resultString);
            var result = Serializer.AsPortalResponse<T>(resultString);

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
