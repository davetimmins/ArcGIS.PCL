namespace ArcGIS.Test.Integration
{
    using ArcGIS.ServiceModel;
    using ArcGIS.ServiceModel.Common;
    using ArcGIS.ServiceModel.Operation;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;

    public class SecureGISGatewayTests : IClassFixture<IntegrationTestFixture>
    {
        public SecureGISGatewayTests(IntegrationTestFixture fixture, ITestOutputHelper output)
        {
            fixture.SetTestOutputHelper(output);
            CryptoProviderFactory.Disabled = true;
        }

        [Theory]
        [InlineData("https://serverapps10.esri.com/arcgis", "user1", "pass.word1")]
        public async Task CanGenerateToken(string rootUrl, string username, string password)
        {
            var tokenProvider = new TokenProvider(rootUrl, username, password);
            var token = await IntegrationTestFixture.TestPolicy.ExecuteAsync(() =>
            {
                return tokenProvider.CheckGenerateToken(CancellationToken.None);
            });

            Assert.NotNull(token);
            Assert.NotNull(token.Value);
            Assert.False(token.IsExpired);
            Assert.Null(token.Error);
        }

        [Theory]
        [InlineData("https://serverapps10.esri.com/arcgis", "user1", "pass.word1")]
        public async Task CanDescribeSecureSite(string rootUrl, string username, string password)
        {
            var gateway = new SecurePortalGateway(rootUrl, username, password);

            var response = await IntegrationTestFixture.TestPolicy.ExecuteAsync(() =>
            {
                return gateway.DescribeSite();
            });

            Assert.NotNull(response);
            Assert.True(response.Version > 0);

            foreach (var resource in response.ArcGISServerEndpoints)
            {
                var ping = await IntegrationTestFixture.TestPolicy.ExecuteAsync(() =>
                {
                    return gateway.Ping(resource);
                });
                Assert.Null(ping.Error);
            }
        }

        [Theory]
        [InlineData("https://serverapps10.esri.com/arcgis", "Oil/MapServer")]
        public async Task CannotAccessSecureResourceWithoutToken(string rootUrl, string relativeUrl)
        {
            var gateway = new PortalGateway(rootUrl);
            var endpoint = new ArcGISServerEndpoint(relativeUrl);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => gateway.Ping(endpoint));
            Assert.NotNull(exception);
            Assert.Contains("Unauthorized access", exception.Message);
        }

        [Theory]
        [InlineData("https://serverapps10.esri.com/arcgis", "user1", "pass.word1", "Oil/MapServer")]
        public async Task InvalidTokenReported(string rootUrl, string username, string password, string relativeUrl)
        {
            var tokenProvider = new TokenProvider(rootUrl, username, password);
            var gateway = new PortalGateway(rootUrl, tokenProvider: tokenProvider);
            var endpoint = new ArcGISServerEndpoint(relativeUrl);

            var token = await IntegrationTestFixture.TestPolicy.ExecuteAsync(() =>
            {
                return tokenProvider.CheckGenerateToken(CancellationToken.None);
            });

            Assert.NotNull(token);
            Assert.NotNull(token.Value);
            Assert.False(token.IsExpired);
            Assert.Null(token.Error);

            token.Value += "chuff";
            var query = new Query(endpoint)
            {
                Token = token.Value
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => gateway.Query<Point>(query));
            Assert.NotNull(exception);
            Assert.Contains("Invalid token", exception.Message);
        }
    }
}
