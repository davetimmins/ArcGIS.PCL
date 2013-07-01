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
        /// Deserialize string as a <see cref="PortalResponse"/>
        /// </summary>
        /// <typeparam name="T">The type of the result from the call</typeparam>
        /// <param name="dataToConvert">Json string to deserialize</param>
        /// <returns></returns>
        T AsPortalResponse<T>(String dataToConvert) where T : PortalResponse;
    }

    public abstract class PortalGateway : IPortalGateway
    {
        const String AGOPortalUrl = "http://www.arcgis.com/sharing/rest/";
        protected readonly GenerateToken TokenRequest;

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
            rootUrl = rootUrl.TrimEnd('/');
            rootUrl = rootUrl.Replace("/rest/services", "");
            RootUrl = rootUrl.ToLower() + '/';

            if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password))
                TokenRequest = new GenerateToken { Username = username, Password = password };
        }

        public string RootUrl { get; private set; }

        public Token Token { get; private set; }

        public ISerializer Serializer { get; set; }

        async Task<Token> CheckGenerateToken()
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
        /// empty <see cref="PortalResponse"/> object if successful otherwise the Error property is populated</returns>
        public Task<PortalResponse> Ping(IEndpoint endpoint)
        {
            return Get<PortalResponse>(endpoint);
        }

        protected Task<T> Get<T, TRequest>(IEndpoint endpoint, TRequest requestObject)
            where TRequest : CommonParameters
            where T : PortalResponse
        {
            return Get<T>((endpoint.RelativeUrl + AsRequestQueryString(requestObject)).AsEndpoint());
        }

        protected async Task<T> Get<T>(IEndpoint endpoint) where T : PortalResponse
        {
            if (Serializer == null) throw new NullReferenceException("Serializer has not been set.");

            var token = await CheckGenerateToken();

            var url = endpoint.BuildAbsoluteUrl(RootUrl);
            if (token != null && !String.IsNullOrWhiteSpace(token.Value) && !url.Contains("token="))
                url += (url.Contains("?") ? "&" : "?") + "token=" + token.Value;
            if (!url.Contains("f="))
                url += (url.Contains("?") ? "&" : "?") + "f=json";
            
            // use POST if request is too long
            if (url.Length > 2082)
                return await Post<T>(endpoint, ParseQueryString(endpoint.RelativeUrl));
            
            using (var handler = new HttpClientHandler())
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                using (var httpClient = new HttpClient(handler))
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var result = Serializer.AsPortalResponse<T>(await response.Content.ReadAsStringAsync());

                    if (result.Error != null)
                        throw new InvalidOperationException(result.Error.ToString());

                    return result;
                }
            }
        }

        protected Task<T1> Post<T1, T2>(IEndpoint endpoint, T2 requestObject)
            where T2 : CommonParameters
            where T1 : PortalResponse
        {
            if (Serializer == null) throw new NullReferenceException("Serializer has not been set.");

            return Post<T1>(endpoint, Serializer.AsDictionary(requestObject));
        }

        protected async Task<T> Post<T>(IEndpoint endpoint, Dictionary<String, String> parameters) where T : PortalResponse
        {
            if (Serializer == null) throw new NullReferenceException("Serializer has not been set.");

            // these should have already been added
            if (!parameters.ContainsKey("f"))
                parameters.Add("f", "json");
            if (!parameters.ContainsKey("token") && Token != null && !String.IsNullOrWhiteSpace(Token.Value))
                parameters.Add("token", Token.Value);

            var url = endpoint.BuildAbsoluteUrl(RootUrl).Split('?').FirstOrDefault();
           
            HttpContent content = new FormUrlEncodedContent(parameters);
            using (var handler = new HttpClientHandler())
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                using (var httpClient = new HttpClient(handler))
                {
                    HttpResponseMessage response = await httpClient.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                    var result = Serializer.AsPortalResponse<T>(await response.Content.ReadAsStringAsync());
                    if (result.Error != null)
                        throw new InvalidOperationException(result.Error.ToString());

                    return result;
                }
            }
        }

        String AsRequestQueryString<T>(T objectToConvert) where T : CommonParameters
        {
            if (Serializer == null) throw new NullReferenceException("Serializer has not been set.");

            var dictionary = Serializer.AsDictionary(objectToConvert);

            return "?" + String.Join("&", dictionary.Keys.Select(k => String.Format("{0}={1}", k, dictionary[k].UrlEncode())));
        }

        static Dictionary<String, String> ParseQueryString(String queryString)
        {
            // remove anything other than query string from url
            if (queryString.Contains("?"))
                queryString = queryString.Substring(queryString.IndexOf('?') + 1);

            return Regex.Split(queryString, "&")
                .Select(vp => Regex.Split(vp, "="))
                .ToDictionary(singlePair => singlePair[0], singlePair => singlePair.Length == 2 ? singlePair[1] : String.Empty);
        }
    }
}
