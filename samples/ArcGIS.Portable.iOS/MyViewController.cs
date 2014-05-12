using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Linq;
using ArcGIS.ServiceModel;
using ArcGIS.ServiceModel.Operation;
using System.Net.Http;
using ModernHttpClient;

namespace ArcGIS.Portable.iOS
{
    public class MyViewController : UIViewController
    {
        UIButton button;
        int numClicks = 0;
        float buttonWidth = 200;
        float buttonHeight = 50;
        UILabel label1;

        public MyViewController()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.Frame = UIScreen.MainScreen.Bounds;
            View.BackgroundColor = UIColor.White;
            View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

            button = UIButton.FromType(UIButtonType.RoundedRect);

            button.Frame = new RectangleF(
                View.Frame.Width / 2 - buttonWidth / 2,
                View.Frame.Height / 2 - buttonHeight / 2,
                buttonWidth,
                buttonHeight);

            button.SetTitle("GO", UIControlState.Normal);

            label1 = new UILabel(View.Frame);     

            HttpClientFactory.Get = (() => new HttpClient(new AFNetworkHandler()));

            var serializer = new JsonDotNetSerializer();
            var locator = new GeocodeGateway(serializer);
            var geocode = new SingleInputGeocode("/World/GeocodeServer/".AsEndpoint())
            {
                Text = "Wellington",
                SourceCountry = "NZL"
            };

            var gateway = new ArcGISGateway(serializer);

            button.TouchUpInside += async (object sender, EventArgs e) =>
            {
                var geocodeResult = await locator.Geocode(geocode);

                var queryPoint = new Query(@"Earthquakes/EarthquakesFromLastSevenDays/MapServer/0".AsEndpoint())
                {
                    ReturnGeometry = false,
                    Where = "MAGNITUDE > 4.5"
                };
                var resultPoint = await gateway.Query<ArcGIS.ServiceModel.Common.Point>(queryPoint);

                label1.Text = string.Format("Query for earthquakes in last 7 days where magnitidue is more than 4.5, {0} features found. Geocode result for Wellington, NZ x:{1}, y:{2}",
                    resultPoint.Features.Count(),
                    geocodeResult.Results.First().Feature.Geometry.X,
                    geocodeResult.Results.First().Feature.Geometry.Y);
            };

            button.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin |
                UIViewAutoresizing.FlexibleBottomMargin;

            View.AddSubview(button);
            View.Add(label1);
        }

    }
}

