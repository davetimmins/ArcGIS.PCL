namespace ArcGIS.Test.Integration
{
    using Polly;
    using Serilog;
    using Serilog.Events;
    using ServiceModel;
    using ServiceModel.Serializers;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Xunit.Abstractions;

    public class IntegrationTestFixture : IDisposable
    {
        public static Policy TestPolicy { get; private set; }
        IDisposable _logCapture;

        static IntegrationTestFixture()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

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

            TestPolicy = Policy
                .Handle<InvalidOperationException>()
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.ColoredConsole(restrictedToMinimumLevel: LogEventLevel.Warning)
                .WriteTo.XunitTestOutput()
                .CreateLogger();
        }

        public void SetTestOutputHelper(ITestOutputHelper output)
        {
            _logCapture = XUnitTestOutputSink.Capture(output);
        }

        public void Dispose()
        {
            _logCapture.Dispose();
        }
    }
}
