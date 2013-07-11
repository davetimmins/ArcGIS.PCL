using System;
using System.Threading;
using System.Threading.Tasks;
using ArcGIS.ServiceModel.Common;
using Xunit;
using ArcGIS.ServiceModel.Logic;

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

    public class NonSecureGISGateway : PortalGateway
    {
        public NonSecureGISGateway()
            : base(@"http://serverapps10.esri.com/arcgis")
        {
            Serializer = new ServiceStackSerializer();
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
        public async Task CannotAccessSecureResourceWithoutToken()
        {
            var gateway = new NonSecureGISGateway();

            var endpoint = new ArcGISServerEndpoint("Oil/MapServer");

            var exception = await ThrowsAsync<InvalidOperationException>(async () => await gateway.Ping(endpoint));
            Assert.NotNull(exception);
            Assert.Contains("Unauthorized access", exception.Message);
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

            var exception = await ThrowsAsync<InvalidOperationException>(async () => await gateway.Ping(endpoint));
            Assert.NotNull(exception);
            Assert.Contains("Invalid token", exception.Message);
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
