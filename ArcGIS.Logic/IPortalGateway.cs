using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGIS.Logic
{
    public interface IPortalGateway
    {
        String RootUrl { get; }
        Token Token { get; }
    }

    public class PortalGateway : IPortalGateway
    {
        const String AGOPortalUrl = "http://www.arcgis.com/sharing/rest/";
        readonly String _username;
        readonly String _password;

        public PortalGateway()
            : this(AGOPortalUrl, String.Empty, String.Empty)
        { }

        public PortalGateway(String rootUrl)
            : this(rootUrl, String.Empty, String.Empty)
        { }

        public PortalGateway(String username, String password)
            : this(AGOPortalUrl, username, password)
        { }

        public PortalGateway(String rootUrl, String username, String password)
        {
            if (!rootUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase)) rootUrl += "/";
            RootUrl = rootUrl;

            _username = username;
            _password = password;
        }

        async Task<Token> CheckGenerateToken()
        {
            if (String.IsNullOrWhiteSpace(_username) && String.IsNullOrWhiteSpace(_password)) return null;
            if (Token != null && !Token.IsExpired) return Token;

            Token = null;
            var parameters = Token.DefaultParameters;
            parameters.Add("username", _username);
            parameters.Add("password", _password);
            return await Post<Token>("generateToken", parameters);
        }

        public string RootUrl { get; private set; }

        public Token Token { get; private set; }

        /// <summary>
        /// Perform a GET operation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <remarks>need to use <see cref="PortalResponse"/> as the response returns 200 even when an error occurs</remarks>
        /// <returns></returns>
        protected async Task<T> Get<T>(String url) where T : PortalResponse
        {
            var token = await CheckGenerateToken();

            if (token != null && !String.IsNullOrWhiteSpace(token.Value))
                url += "&token=" + token.Value;

            if (!url.StartsWith(RootUrl)) url = RootUrl + url;

            using (var handler = new HttpClientHandler())
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                using (var httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(@"application/json"));
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    var result = ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(await response.Content.ReadAsStringAsync());
                    if (result.Error != null)
                        throw new InvalidOperationException(result.Error.ToString());

                    return result;
                }
            }
        }

        protected async Task<T> Post<T>(String url, Dictionary<String, String> parameters) where T : PortalResponse
        {
            if (!parameters.ContainsKey("f"))
                parameters.Add("f", "json");
            if (!parameters.ContainsKey("token") && Token != null && !String.IsNullOrWhiteSpace(Token.Value))
                parameters.Add("token", Token.Value);

            if (!url.StartsWith(RootUrl)) url = RootUrl + url;

            HttpContent content = new FormUrlEncodedContent(parameters);
            using (var handler = new HttpClientHandler())
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                using (var httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(@"application/json"));
                    HttpResponseMessage response = await httpClient.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                    var result = ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(await response.Content.ReadAsStringAsync());
                    if (result.Error != null)
                        throw new InvalidOperationException(result.Error.ToString());

                    return result;
                }
            }
        }
    }    
}
