using System;
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
                var httpClient = new HttpClient(new ModernHttpClient.NativeMessageHandler());
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                return httpClient;
            });

        }
    }
}