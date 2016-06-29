using ArcGIS.ServiceModel;
using ArcGIS.ServiceModel.Serializers;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ArcGIS.Test
{
    public class TestsFixture : IDisposable
    {
        static TestsFixture()
        {
            ServiceStackSerializer.Init();

            HttpClientFactory.Get = (() =>
            {
                var httpClientHandler = new HttpClientHandler();
                if (httpClientHandler.SupportsAutomaticDecompression)
                    httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                var httpClient = new HttpClient(httpClientHandler) { Timeout = TimeSpan.FromMinutes(3) };
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jsonp"));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

                return httpClient;
            });
        }

        public void Dispose()
        {
            // Do "global" teardown here; Only called once.
        }
    }
}
