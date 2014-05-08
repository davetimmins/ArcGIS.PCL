using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ArcGIS.ServiceModel
{
    public static class HttpClientFactory
    {
        public static Func<HttpClient> Get { get; set; }

        static HttpClientFactory()
        {
            Get = (() =>
            {
                var httpClientHandler = new HttpClientHandler();
                if (httpClientHandler.SupportsAutomaticDecompression)
                    httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                if (httpClientHandler.SupportsUseProxy()) httpClientHandler.UseProxy = true;
                if (httpClientHandler.SupportsAllowAutoRedirect()) httpClientHandler.AllowAutoRedirect = true;
                if (httpClientHandler.SupportsPreAuthenticate()) httpClientHandler.PreAuthenticate = true;

                var httpClient = new HttpClient(httpClientHandler);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jsonp"));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

                return httpClient;
            });
        }
    }
}