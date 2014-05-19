using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using ArcGIS.ServiceModel;
using ArcGIS.ServiceModel.Operation;
using System.Linq;
using ArcGIS.ServiceModel.Common;
using System.Net.Http;
using ModernHttpClient;

namespace ArcGIS.Portable.Android
{
	[Activity (Label = "ArcGIS.Portable.Android", MainLauncher = true)]
	public class MainActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);
			TextView textview = FindViewById<TextView> (Resource.Id.textView1);

			ArcGIS.ServiceModel.Serializers.JsonDotNetSerializer.Init ();

			var locator = new GeocodeGateway();
			var geocode = new SingleInputGeocode("/World/GeocodeServer/".AsEndpoint())
			{
				Text = "Wellington",
				SourceCountry = "NZL"
			};

			var gateway = new ArcGISGateway();
						
			button.Click += async delegate {

				var geocodeResult = await locator.Geocode(geocode);

				var queryPoint = new Query(@"Earthquakes/EarthquakesFromLastSevenDays/MapServer/0".AsEndpoint()) 
				{
					ReturnGeometry = false,
					Where = "MAGNITUDE > 4.5"
				};
				var resultPoint = await gateway.Query<Point>(queryPoint);

				textview.Text = string.Format ("Query for earthquakes in last 7 days where magnitidue is more than 4.5, {0} features found. Geocode result for Wellington, NZ x:{1}, y:{2}", 
					resultPoint.Features.Count(),
					geocodeResult.Results.First().Feature.Geometry.X,
					geocodeResult.Results.First().Feature.Geometry.Y);
			};
		}
	}
}


