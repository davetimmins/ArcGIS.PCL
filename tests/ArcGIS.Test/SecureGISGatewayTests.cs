using ArcGIS.ServiceModel;
using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel.Operation;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArcGIS.Test
{
    public class SecureGISGatewayTests : TestsFixture
    {
        public SecureGISGatewayTests()
        {
            CryptoProviderFactory.Disabled = true;
        }

        [Theory]
        [InlineData("http://serverapps10.esri.com/arcgis", "user1", "pass.word1")]
        public async Task CanGenerateToken(string rootUrl, string username, string password)
        {
            var tokenProvider = new TokenProvider(rootUrl, username, password);

            var token = await tokenProvider.CheckGenerateToken(CancellationToken.None);

            Assert.NotNull(token);
            Assert.NotNull(token.Value);
            Assert.False(token.IsExpired);
            Assert.Null(token.Error);
        }

        [Theory]
        [InlineData("http://serverapps10.esri.com/arcgis", "user1", "pass.word1")]
        public async Task CanDescribeSecureSite(string rootUrl, string username, string password)
        {
            var gateway = new SecurePortalGateway(rootUrl, username, password);

            var response = await gateway.DescribeSite();

            Assert.NotNull(response);
            Assert.True(response.Version > 0);

            foreach (var resource in response.ArcGISServerEndpoints)
            {
                var ping = await gateway.Ping(resource);
                Assert.Null(ping.Error);
            }
        }

        [Fact]
        public void TokenIsExpiredHasCorrectValue()
        {
            var expiry = DateTime.UtcNow.AddSeconds(1).ToUnixTime();
            var token = new ArcGIS.ServiceModel.Operation.Token { Value = "blah", Expiry = expiry };

            Assert.NotNull(token);
            Assert.NotNull(token.Value);
            Assert.False(token.IsExpired);
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Assert.True(token.IsExpired);
        }

        [Theory]
        [InlineData("http://serverapps10.esri.com/arcgis", "Oil/MapServer")]
        public async Task CannotAccessSecureResourceWithoutToken(string rootUrl, string relativeUrl)
        {
            var gateway = new PortalGateway(rootUrl);
            var endpoint = new ArcGISServerEndpoint(relativeUrl);

            var exception = await ThrowsAsync<InvalidOperationException>(async () => await gateway.Ping(endpoint));

            Assert.NotNull(exception);
            Assert.Contains("Unauthorized access", exception.Message);
        }

        [Theory]
        [InlineData("http://serverapps10.esri.com/arcgis", "user1", "pass.word1", "Oil/MapServer")]
        public async Task InvalidTokenReported(string rootUrl, string username, string password, string relativeUrl)
        {
            var tokenProvider = new TokenProvider(rootUrl, username, password);
            var gateway = new PortalGateway(rootUrl, tokenProvider: tokenProvider);
            var endpoint = new ArcGISServerEndpoint(relativeUrl);

            var token = await tokenProvider.CheckGenerateToken(CancellationToken.None);

            Assert.NotNull(token);
            Assert.NotNull(token.Value);
            Assert.False(token.IsExpired);
            Assert.Null(token.Error);

            token.Value += "chuff";
            var query = new Query(endpoint)
            {
                Token = token.Value
            };

            var exception = await ThrowsAsync<InvalidOperationException>(async () => await gateway.Query<Point>(query));
            Assert.NotNull(exception);
            Assert.Contains("Invalid token", exception.Message);
        }

        // [Fact]
        public async Task CanStopAndStartService()
        {
            var gateway = new SecurePortalGateway("", "", "");
            var folder = ""; // set this to only get a specific folder
            var site = await gateway.SiteReport(folder);
            Assert.NotNull(site);
            var services = site.ServiceReports.Where(s => s.Type == "MapServer");
            Assert.NotNull(services);
            Assert.True(services.Any());
            foreach (var service in services)
            {
                var sd = service.AsServiceDescription();
                if (string.Equals("STARTED", service.Status.Actual, StringComparison.OrdinalIgnoreCase))
                {
                    var stoppedResult = await gateway.StopService(sd);
                    Assert.NotNull(stoppedResult);
                    Assert.Null(stoppedResult.Error);
                    Assert.Equal("success", stoppedResult.Status);

                    var startedResult = await gateway.StartService(sd);
                    Assert.NotNull(startedResult);
                    Assert.Null(startedResult.Error);
                    Assert.Equal("success", startedResult.Status);
                }
                else
                {
                    var startedResult = await gateway.StartService(sd);
                    Assert.NotNull(startedResult);
                    Assert.Null(startedResult.Error);
                    Assert.Equal("success", startedResult.Status);

                    var stoppedResult = await gateway.StopService(sd);
                    Assert.NotNull(stoppedResult);
                    Assert.Null(stoppedResult.Error);
                    Assert.Equal("success", stoppedResult.Status);
                }
            }
        }

        public static async Task<TException> ThrowsAsync<TException>(Func<Task> func) where TException : Exception
        {
            var expected = typeof(TException);
            Type actual = null;
            TException result = null;
            try
            {
                await func();
            }
            catch (TException e)
            {
                actual = e.GetType();
                result = e;
            }

            Assert.Equal(expected, actual);
            return result;
        }
    }
}
