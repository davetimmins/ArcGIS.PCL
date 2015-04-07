namespace ArcGIS.Test.Integration
{
    using ArcGIS.ServiceModel;
    using ArcGIS.ServiceModel.Serializers;
    using Serilog;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;

    public class TestsFixture : IDisposable
    {
        public TestsFixture()
        {
            JsonDotNetSerializer.Init();

            HttpClientFactory.Get = (() =>
            {
                var httpClientHandler = new HttpClientHandler();
                if (httpClientHandler.SupportsAutomaticDecompression)
                    httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                var httpClient = new HttpClient(httpClientHandler);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jsonp"));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

                return httpClient;
            });

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.ColoredConsole()
                .CreateLogger();
        }

        public void Dispose()
        {
            // Do "global" teardown here; Only called once.
        }
    }
}
