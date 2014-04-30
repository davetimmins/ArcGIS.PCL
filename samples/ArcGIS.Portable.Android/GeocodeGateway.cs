using System;
using ArcGIS.ServiceModel;
using ArcGIS.ServiceModel.Operation;
using System.Threading.Tasks;

namespace ArcGIS.Portable.Android
{
	public class GeocodeGateway : PortalGateway
	{
		public GeocodeGateway(ISerializer serializer)
			: base("http://geocode.arcgis.com/arcgis", serializer, null)
		{ }

		public Task<SingleInputGeocodeResponse> Geocode(SingleInputGeocode geocode)
		{
			return Get<SingleInputGeocodeResponse, SingleInputGeocode>(geocode);
		}
	}
}