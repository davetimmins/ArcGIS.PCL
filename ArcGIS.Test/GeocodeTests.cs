using System.Linq;
using System.Threading.Tasks;
using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel.Extensions;
using ArcGIS.ServiceModel.Logic;
using ArcGIS.ServiceModel.Operation;
using Xunit;

namespace ArcGIS.Test
{
    public class GeocodeGateway : PortalGateway
    {
        public GeocodeGateway()
            : base("http://geocode.arcgis.com/arcgis")
        {
            Serializer = new ServiceStackSerializer();
        }

        public Task<ReverseGeocodeResponse> ReverseGeocode(ReverseGeocode reverseGeocode)
        {
            return Get<ReverseGeocodeResponse, ReverseGeocode>(reverseGeocode);
        }

        public Task<SingleInputGeocodeResponse> Geocode(SingleInputGeocode geocode)
        {
            return Get<SingleInputGeocodeResponse, SingleInputGeocode>(geocode);
        }
    }

    public class GeocodeTests
    {
        [Fact]
        public async Task CanGeocode()
        {
            var gateway = new GeocodeGateway();
            var geocode = new SingleInputGeocode("/World/GeocodeServer/".AsEndpoint())
            {
                Text = "100 Willis Street, Wellington",
                SourceCountry = "NZL"
            };
            var response = await gateway.Geocode(geocode);

            Assert.Null(response.Error);
            Assert.NotNull(response.SpatialReference);
            Assert.NotNull(response.Results);
            Assert.True(response.Results.Any());
            var result = response.Results.First();
            Assert.NotNull(result.Feature);
        }

        [Fact]
        public async Task CanReverseGeocodePoint()
        {
            var gateway = new GeocodeGateway();
            var reverseGeocode = new ReverseGeocode("/World/GeocodeServer/".AsEndpoint())
            {
                Location = new Point
                {
                    X = 174.775505,
                    Y = -41.290893,
                    SpatialReference = new SpatialReference { Wkid = SpatialReference.WGS84.LatestWkid }
                }
            };
            var response = await gateway.ReverseGeocode(reverseGeocode);

            Assert.Null(response.Error);
            Assert.NotNull(response.Address);
            Assert.NotNull(response.Location);
            Assert.Equal(response.Address.CountryCode, "NZL");
        }
    }
}
