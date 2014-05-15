using System;
using ArcGIS.ServiceModel;
using ArcGIS.ServiceModel.Operation;
using System.Threading.Tasks;

namespace ArcGIS.Portable.iOS
{
	public class GeocodeGateway : PortalGateway
	{
		public GeocodeGateway()
			: base("http://geocode.arcgis.com/arcgis")
		{ }

		public Task<SingleInputGeocodeResponse> Geocode(SingleInputGeocode geocode)
		{
			return Get<SingleInputGeocodeResponse, SingleInputGeocode>(geocode);
		}
	}
}