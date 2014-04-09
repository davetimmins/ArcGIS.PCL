using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ArcGIS.ServiceModel.Operation;
using ArcGIS.ServiceModel;

namespace ArcGIS.PCL.iOS
{
    public class ArcGISGateway : PortalGateway
    {
        public ArcGISGateway(ISerializer serializer)
            : base(@"http://sampleserver3.arcgisonline.com/ArcGIS/", serializer)
        { }
    }

    public class JsonDotNetSerializer : ISerializer
    {
        readonly Newtonsoft.Json.JsonSerializerSettings _settings;

        public JsonDotNetSerializer()
        {
            _settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
        }

        public Dictionary<String, String> AsDictionary<T>(T objectToConvert) where T : CommonParameters
        {
            var stringValue = Newtonsoft.Json.JsonConvert.SerializeObject(objectToConvert, _settings);

            var jobject = Newtonsoft.Json.Linq.JObject.Parse(stringValue);
            var dict = new Dictionary<String, String>();
            foreach (var item in jobject)
            {
                dict.Add(item.Key, item.Value.ToString());
            }
            return dict;
        }

        public T AsPortalResponse<T>(String dataToConvert) where T : IPortalResponse
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dataToConvert, _settings);
        }
    }
}