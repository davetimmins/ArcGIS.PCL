using System;
using System.Drawing;

using Foundation;
using UIKit;
using ArcGIS.ServiceModel;
using ArcGIS.ServiceModel.Operation;
using System.Linq;

namespace ArcGIS.Portable.iOSUnified
{
	public partial class ArcGIS_Portable_iOSUnifiedViewController : UIViewController
	{
		UIButton button;
		float buttonWidth = 200;
		float buttonHeight = 50;
		UITextView textView;

		public ArcGIS_Portable_iOSUnifiedViewController (IntPtr handle) : base (handle)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		#region View lifecycle

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.
			View.Frame = UIScreen.MainScreen.Bounds;
			View.BackgroundColor = UIColor.White;
			View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

			button = UIButton.FromType(UIButtonType.RoundedRect);

			button.Frame = new RectangleF(
				(float)(View.Frame.Width / 2 - buttonWidth / 2),
				buttonHeight,
				buttonWidth,
				buttonHeight);

			button.SetTitle("GO", UIControlState.Normal);

			textView = new UITextView(new RectangleF(
				(float)(View.Frame.Width / 2 - buttonWidth / 2),
				(float)((View.Frame.Height / 2 - buttonHeight / 2) - buttonHeight),
				buttonWidth,
				(float)((View.Frame.Height / 2 - buttonHeight / 2) - buttonHeight)));

			ArcGIS.ServiceModel.Serializers.JsonDotNetSerializer.Init();

			var locator = new PortalGateway("http://geocode.arcgis.com/arcgis");
			var geocode = new SingleInputGeocode("/World/GeocodeServer/".AsEndpoint())
			{
				Text = "Wellington",
				SourceCountry = "NZL"
			};

			var gateway = new PortalGateway("http://sampleserver3.arcgisonline.com/ArcGIS/");
			var queryPoint = new Query(@"Earthquakes/EarthquakesFromLastSevenDays/MapServer/0".AsEndpoint())
			{
				ReturnGeometry = false,
				Where = "MAGNITUDE > 4.5"
			};

			button.TouchUpInside += async (object sender, EventArgs e) =>
			{
				var geocodeResult = await locator.Geocode(geocode);

				var resultPoint = await gateway.Query<ArcGIS.ServiceModel.Common.Point>(queryPoint);

				textView.Text = string.Format("Query for earthquakes in last 7 days where magnitidue is more than 4.5, {0} features found. Geocode result for Wellington, NZ x:{1}, y:{2}",
					resultPoint.Features.Count(),
					geocodeResult.Results.First().Feature.Geometry.X,
					geocodeResult.Results.First().Feature.Geometry.Y);
			};

			button.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin |
				UIViewAutoresizing.FlexibleBottomMargin;

			View.AddSubview(button);
			View.Add(textView);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
		}

		#endregion
	}
}

