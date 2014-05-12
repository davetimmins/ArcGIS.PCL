using System.Linq;
using System.Threading.Tasks;
using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel;
using ArcGIS.ServiceModel.Operation;
using Xunit;
using ArcGIS.ServiceModel.Serializers;

namespace ArcGIS.Test
{
    public class GeocodeGateway : PortalGateway
    {
        public GeocodeGateway(ISerializer serializer)
            : base("http://geocode.arcgis.com/arcgis", serializer, null)
        { }

        public Task<ReverseGeocodeResponse> ReverseGeocode(ReverseGeocode reverseGeocode)
        {
            return Get<ReverseGeocodeResponse, ReverseGeocode>(reverseGeocode);
        }

        public Task<SingleInputGeocodeResponse> Geocode(SingleInputGeocode geocode)
        {
            return Get<SingleInputGeocodeResponse, SingleInputGeocode>(geocode);
        }

        public Task<SuggestGeocodeResponse> Suggest(SuggestGeocode suggestGeocode)
        {
            return Get<SuggestGeocodeResponse, SuggestGeocode>(suggestGeocode);
        }
    }

    public class GeocodeTests
    {
        [Fact]
        public async Task CanGeocode()
        {
            var gateway = new GeocodeGateway(new ServiceStackSerializer());
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
            Assert.NotNull(result.Feature.Geometry);
        }

        [Fact]
        public async Task CanSuggest()
        {
            var gateway = new GeocodeGateway(new ServiceStackSerializer());
            var suggest = new SuggestGeocode("/World/GeocodeServer/".AsEndpoint())
            {
                Text = "100 Willis Street, Wellington"
            };
            var response = await gateway.Suggest(suggest);

            Assert.Null(response.Error);
            Assert.NotNull(response.Suggestions);
            Assert.True(response.Suggestions.Any());
            var result = response.Suggestions.First();
            Assert.True(!string.IsNullOrWhiteSpace(result.Text));
        }
        
        [Fact]
        public async Task CanReverseGeocodePoint()
        {
            var gateway = new GeocodeGateway(new ServiceStackSerializer());
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
