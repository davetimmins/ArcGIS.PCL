using System;
using ArcGIS.ServiceModel.Common;
using Xunit;

namespace ArcGIS.Test
{
    public class EndpointTests
    {
        [Fact]
        public void ArcGISServerEndpointHasCorrectFormat()
        {
            var endpoint = new ArcGISServerEndpoint("");
            Assert.True(endpoint.RelativeUrl.StartsWith("rest/services/", StringComparison.InvariantCultureIgnoreCase));

            var endpoint2 = new ArcGISServerEndpoint("/rest/services/rest/services/rest/services/");
            Assert.True(endpoint2.RelativeUrl.StartsWith("rest/services/", StringComparison.InvariantCultureIgnoreCase));

            var endpoint3 = new ArcGISServerEndpoint("something/MapServer");
            Assert.True(endpoint3.RelativeUrl.StartsWith("rest/services/", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
