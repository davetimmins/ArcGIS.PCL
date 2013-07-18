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
            Assert.Throws<ArgumentNullException>(() => new ArcGISServerEndpoint(""));

            var endpoint2 = new ArcGISServerEndpoint("/rest/services/rest/services/rest/services/");
            Assert.True(endpoint2.RelativeUrl.StartsWith("rest/services/", StringComparison.InvariantCultureIgnoreCase));

            var endpoint3 = new ArcGISServerEndpoint("something/MapServer");
            Assert.True(endpoint3.RelativeUrl.StartsWith("rest/services/", StringComparison.InvariantCultureIgnoreCase));

            var endpoint4 = new ArcGISServerEndpoint("/rest/services/");
            Assert.True(endpoint4.RelativeUrl.StartsWith("rest/services/", StringComparison.InvariantCultureIgnoreCase));

            var endpoint5 = new ArcGISServerEndpoint("rest/services/");
            Assert.True(endpoint5.RelativeUrl.StartsWith("rest/services/", StringComparison.InvariantCultureIgnoreCase));

            var endpoint6 = new ArcGISServerEndpoint("rest/services");
            Assert.True(endpoint6.RelativeUrl.StartsWith("rest/services/", StringComparison.InvariantCultureIgnoreCase));

            var endpoint7 = new ArcGISServerEndpoint("/rest/services");
            Assert.True(endpoint7.RelativeUrl.StartsWith("rest/services/", StringComparison.InvariantCultureIgnoreCase));

            var endpoint8 = new ArcGISServerEndpoint("/");
            Assert.True(endpoint8.RelativeUrl.StartsWith("rest/services/", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
