using System;
using System.Threading;
using System.Threading.Tasks;
using ArcGIS.ServiceModel.Common;
using Xunit;

namespace ArcGIS.Test
{
    public class SecureGISGateway : ArcGISGateway
    {
        public SecureGISGateway()
            : base(@"http://serverapps10.esri.com/arcgis", "user1", "pass.word1")
        // these credentials are from the Esri samples before you complain :)
        { }

        public SecureGISGateway(int tokenExpiryInMinutes)
            : this()
        {
            TokenRequest.ExpirationInMinutes = tokenExpiryInMinutes;
        }
    }

    public class SecureGISGatewayTests
    {
        [Fact]
        public async Task CanGenerateToken()
        {
            var gateway = new SecureGISGateway();

            var endpoint = new ArcGISServerEndpoint("Oil/MapServer");

            var response = await gateway.Ping(endpoint);

            Assert.NotNull(gateway.Token);
            Assert.NotNull(gateway.Token.Value);
            Assert.False(gateway.Token.IsExpired);
            Assert.Null(response.Error);

            gateway.Serializer = new JsonDotNetSerializer();

            response = await gateway.Ping(endpoint);

            Assert.NotNull(gateway.Token);
            Assert.NotNull(gateway.Token.Value);
            Assert.False(gateway.Token.IsExpired);
            Assert.Null(response.Error);
        }

        [Fact]
        public async Task CanDescribeSite()
        {
            var gateway = new SecureGISGateway();

            var response = await gateway.DescribeSite();

            Assert.NotNull(response);
            Assert.True(response.Version > 0);

            foreach (var resource in response.Resources)
            {
                var ping = await gateway.Ping(resource);
                Assert.Null(ping.Error);
            }
        }

        [Fact]
        public async Task CanGenerateShortLivedToken()
        {
            var gateway = new SecureGISGateway(1); // 60 seconds

            var endpoint = new ArcGISServerEndpoint("Oil/MapServer");

            var response = await gateway.Ping(endpoint);

            Assert.NotNull(gateway.Token);
            Assert.NotNull(gateway.Token.Value);
            Thread.Sleep(TimeSpan.FromSeconds(61));
            Assert.True(gateway.Token.IsExpired); // this fails at the moment, need to test on later version of AGS
            Assert.Null(response.Error);
        }

        [Fact]
        public async Task InvalidTokenReported()
        {
            var gateway = new SecureGISGateway();

            var endpoint = new ArcGISServerEndpoint("Oil/MapServer");

            var response = await gateway.Ping(endpoint);

            Assert.NotNull(gateway.Token);
            Assert.NotNull(gateway.Token.Value);
            Assert.False(gateway.Token.IsExpired);
            Assert.Null(response.Error);

            gateway.Token.Value += "chuff";

            await ThrowsAsync<InvalidOperationException>(async () => await gateway.Ping(endpoint));
        }

        public static async Task ThrowsAsync<TException>(Func<Task> func)
        {
            var expected = typeof(TException);
            Type actual = null;
            try
            {
                await func();
            }
            catch (Exception e)
            {
                actual = e.GetType();
            }
            Assert.Equal(expected, actual);
        }
    }
}
