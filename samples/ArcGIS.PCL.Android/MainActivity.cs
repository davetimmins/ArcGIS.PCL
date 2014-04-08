using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace ArcGIS.PCL.Android
{
	[Activity (Label = "ArcGIS.PCL.Android", MainLauncher = true)]
	public class MainActivity : Activity
	{
		int count = 1;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);
			
			button.Click += delegate {
                var gateway = new ArcGISGateway(new JsonDotNetSerializer());
                var site = gateway.DescribeSite().Result;

				button.Text = string.Format ("{0} services", site.Resources.Count);

                ListView listView = FindViewById<ListView>(Resource.Id.listView1);
			};
		}
	}
}


