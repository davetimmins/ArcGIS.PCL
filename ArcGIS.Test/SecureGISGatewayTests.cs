using System;
using System.Threading;
using System.Threading.Tasks;
using ArcGIS.ServiceModel.Common;
using Xunit;
using ArcGIS.ServiceModel;
using ArcGIS.ServiceModel.Operation;
using ArcGIS.ServiceModel.Serializers;

namespace ArcGIS.Test
{
    public class SecureTokenProvider : TokenProvider
    {
        public SecureTokenProvider(ISerializer serializer)
            : base(@"http://serverapps10.esri.com/arcgis", "user1", "pass.word1", serializer)
        // these credentials are from the Esri samples before you complain :)
        { }
    }

    public class SecureGISGateway : SecureArcGISServerGateway
    {
        public SecureGISGateway(ISerializer serializer)
            : base(@"http://serverapps10.esri.com/arcgis", "user1", "pass.word1", serializer)
        { }
    }

    public class SecureGISGateway2 : PortalGateway
    {
        public SecureGISGateway2(ISerializer serializer, ITokenProvider tokenProvider)
            : base(@"http://serverapps10.esri.com/arcgis", serializer, tokenProvider)
        { }
    }

    public class NonSecureGISGateway : PortalGateway
    {
        public NonSecureGISGateway(ISerializer serializer)
            : base(@"http://serverapps10.esri.com/arcgis", serializer, null)
        { }
    }

    public class SecureGISGatewayTests
    {
        ServiceStackSerializer _serviceStackSerializer;

        public SecureGISGatewayTests()
        {
            _serviceStackSerializer = new ServiceStackSerializer();
            CryptoProviderFactory.Disabled = true; 
        }

        [Fact]
        public async Task CanGenerateToken()
        {
            var tokenProvider = new SecureTokenProvider(_serviceStackSerializer);
            var gateway = new SecureGISGateway(_serviceStackSerializer);

            var endpoint = new ArcGISServerEndpoint("Oil/MapServer");

            var response = await gateway.Ping(endpoint);

            var token = await tokenProvider.CheckGenerateToken();

            Assert.NotNull(token);
            Assert.NotNull(token.Value);
            Assert.False(token.IsExpired);
            Assert.Null(response.Error);

            gateway = new SecureGISGateway(new JsonDotNetSerializer());

            response = await gateway.Ping(endpoint);

            token = await tokenProvider.CheckGenerateToken();

            Assert.NotNull(token);
            Assert.NotNull(token.Value);
            Assert.False(token.IsExpired);
            Assert.Null(response.Error);
        }

        [Fact]
        public async Task CanDescribeSite()
        {
            var gateway = new SecureGISGateway(_serviceStackSerializer);

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

        [Fact]
        public async Task CannotAccessSecureResourceWithoutToken()
        {
            var gateway = new NonSecureGISGateway(_serviceStackSerializer);

            var endpoint = new ArcGISServerEndpoint("Oil/MapServer");

            var exception = await ThrowsAsync<InvalidOperationException>(async () => await gateway.Ping(endpoint));
            Assert.NotNull(exception);
            Assert.Contains("Unauthorized access", exception.Message);
        }

        [Fact]
        public async Task InvalidTokenReported()
        {
            var tokenProvider = new SecureTokenProvider(_serviceStackSerializer);
            var gateway = new SecureGISGateway2(_serviceStackSerializer, tokenProvider);

            var endpoint = new ArcGISServerEndpoint("Oil/MapServer");

            var response = await gateway.Ping(endpoint);

            var token = await tokenProvider.CheckGenerateToken();

            Assert.NotNull(token);
            Assert.NotNull(token.Value);
            Assert.False(token.IsExpired);
            Assert.Null(token.Error);
            Assert.Null(response.Error);

            token.Value += "chuff";
            var query = new Query(@"/Earthquakes/EarthquakesFromLastSevenDays/MapServer/0".AsEndpoint())
            {
                Token = token.Value
            };

            var exception = await ThrowsAsync<InvalidOperationException>(async () => await gateway.Query<Point>(query));
            Assert.NotNull(exception);
            Assert.Contains("Invalid token", exception.Message);
        }

        //[Fact]
        //public async Task OAuthTokenCanBeGenerated()
        //{
        //    // Set your client Id and secret here
        //    var tokenProvider = new ArcGISOnlineAppLoginOAuthProvider("", "", _serviceStackSerializer);

        //    var token = await tokenProvider.CheckGenerateToken();

        //    Assert.NotNull(token);
        //    Assert.NotNull(token.Value);
        //    Assert.False(token.IsExpired);
        //    Assert.Null(token.Error);
        //}

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
