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

        [Fact]
        public void ArcGISServerAdminEndpointHasCorrectFormat()
        {
            Assert.Throws<ArgumentNullException>(() => new ArcGISServerAdminEndpoint(null));
            Assert.Throws<ArgumentNullException>(() => new ArcGISServerAdminEndpoint(String.Empty));
            Assert.Throws<ArgumentNullException>(() => new ArcGISServerAdminEndpoint(""));

            var endpoint2 = new ArcGISServerAdminEndpoint("/admin/admin/admin/");
            Assert.True(endpoint2.RelativeUrl.StartsWith("admin/", StringComparison.InvariantCultureIgnoreCase));

            var endpoint3 = new ArcGISServerAdminEndpoint("something/MapServer");
            Assert.True(endpoint3.RelativeUrl.StartsWith("admin/", StringComparison.InvariantCultureIgnoreCase));

            var endpoint4 = new ArcGISServerAdminEndpoint("/admin/");
            Assert.True(endpoint4.RelativeUrl.StartsWith("admin/", StringComparison.InvariantCultureIgnoreCase));

            var endpoint5 = new ArcGISServerAdminEndpoint("admin/");
            Assert.True(endpoint5.RelativeUrl.StartsWith("admin/", StringComparison.InvariantCultureIgnoreCase));

            var endpoint6 = new ArcGISServerAdminEndpoint("admin");
            Assert.True(endpoint6.RelativeUrl.StartsWith("admin/", StringComparison.InvariantCultureIgnoreCase));

            var endpoint7 = new ArcGISServerAdminEndpoint("/admin");
            Assert.True(endpoint7.RelativeUrl.StartsWith("admin/", StringComparison.InvariantCultureIgnoreCase));

            var endpoint8 = new ArcGISServerAdminEndpoint("/");
            Assert.True(endpoint8.RelativeUrl.StartsWith("admin/", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
