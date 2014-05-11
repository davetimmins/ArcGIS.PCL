using System;
using System.Collections.Generic;
using ArcGIS.ServiceModel;
using ArcGIS.ServiceModel.Operation;

namespace ArcGIS.ServiceModel.Serializers
{
    public class JsonDotNetSerializer : ISerializer
    {
        static ISerializer _serializer = null;

        public static void Init()
        {
            _serializer = new JsonDotNetSerializer();
            SerializerFactory.Get = (() => _serializer ?? new JsonDotNetSerializer());
        }

        readonly Newtonsoft.Json.JsonSerializerSettings _settings;

        public JsonDotNetSerializer()
        {
            _settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                MissingMemberHandling  = Newtonsoft.Json.MissingMemberHandling.Ignore,
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
