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
            Assert.Throws<ArgumentNullException>(() => new ArcGISServerEndpoint(null));
            Assert.Throws<ArgumentNullException>(() => new ArcGISServerEndpoint(string.Empty));
            Assert.Throws<ArgumentNullException>(() => new ArcGISServerEndpoint(""));

            var endpoint = new ArcGISServerEndpoint("/rest/Services/REST/services/rest/services/");
            Assert.Equal("rest/services/", endpoint.RelativeUrl);

            endpoint = new ArcGISServerEndpoint("/rest/services/rest/services/rest/services/");
            Assert.Equal("rest/services/", endpoint.RelativeUrl);

            endpoint = new ArcGISServerEndpoint("/rest/services/");
            Assert.Equal("rest/services/", endpoint.RelativeUrl);

            endpoint = new ArcGISServerEndpoint("rest/services/");
            Assert.Equal("rest/services/", endpoint.RelativeUrl);

            endpoint = new ArcGISServerEndpoint("rest/services");
            Assert.Equal("rest/services/", endpoint.RelativeUrl);

            endpoint = new ArcGISServerEndpoint("/rest/services");
            Assert.Equal("rest/services/", endpoint.RelativeUrl);

            endpoint = new ArcGISServerEndpoint("/");
            Assert.Equal("rest/services/", endpoint.RelativeUrl);

            endpoint = new ArcGISServerEndpoint("something/MapServer");
            Assert.Equal("rest/services/something/MapServer", endpoint.RelativeUrl);

            endpoint = new ArcGISServerEndpoint("http://www.google.co.nz/rest/services");
            Assert.Equal("rest/services/", endpoint.RelativeUrl);

            endpoint = new ArcGISServerEndpoint("http://www.google.co.nz");
            Assert.Equal("rest/services/", endpoint.RelativeUrl);

            endpoint = new ArcGISServerEndpoint("http://www.google.co.nz/");
            Assert.Equal("rest/services/", endpoint.RelativeUrl);
        }

        [Fact]
        public void ArcGISServerAdminEndpointHasCorrectFormat()
        {
            Assert.Throws<ArgumentNullException>(() => new ArcGISServerAdminEndpoint(null));
            Assert.Throws<ArgumentNullException>(() => new ArcGISServerAdminEndpoint(string.Empty));
            Assert.Throws<ArgumentNullException>(() => new ArcGISServerAdminEndpoint(""));

            var endpoint = new ArcGISServerAdminEndpoint("/admin/admin/admin/");
            Assert.Equal("admin/", endpoint.RelativeUrl);

            endpoint = new ArcGISServerAdminEndpoint("/admin/ADmin/admin/");
            Assert.Equal("admin/", endpoint.RelativeUrl);

            endpoint = new ArcGISServerAdminEndpoint("/admin/");
            Assert.Equal("admin/", endpoint.RelativeUrl);

            endpoint = new ArcGISServerAdminEndpoint("admin/");
            Assert.Equal("admin/", endpoint.RelativeUrl);

            endpoint = new ArcGISServerAdminEndpoint("admin");
            Assert.Equal("admin/", endpoint.RelativeUrl);

            endpoint = new ArcGISServerAdminEndpoint("/admin");
            Assert.Equal("admin/", endpoint.RelativeUrl);

            endpoint = new ArcGISServerAdminEndpoint("/");
            Assert.Equal("admin/", endpoint.RelativeUrl);

            endpoint = new ArcGISServerAdminEndpoint("something/MapServer");
            Assert.Equal("admin/something/MapServer", endpoint.RelativeUrl);

            endpoint = new ArcGISServerAdminEndpoint("http://www.google.co.nz/admin");
            Assert.Equal("admin/", endpoint.RelativeUrl);

            endpoint = new ArcGISServerAdminEndpoint("http://www.google.co.nz");
            Assert.Equal("admin/", endpoint.RelativeUrl);

            endpoint = new ArcGISServerAdminEndpoint("http://www.google.co.nz/");
            Assert.Equal("admin/", endpoint.RelativeUrl);
        }
    }
}
