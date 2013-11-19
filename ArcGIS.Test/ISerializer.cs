using System;
using System.Collections.Generic;
using ArcGIS.ServiceModel;
using ArcGIS.ServiceModel.Operation;
using System.Diagnostics;
using MethodTimer;

namespace ArcGIS.Test
{
    public class ServiceStackSerializer : ISerializer
    {
        public ServiceStackSerializer()
        {
            ServiceStack.Text.JsConfig.EmitCamelCaseNames = true;
            ServiceStack.Text.JsConfig.IncludeTypeInfo = false;
            ServiceStack.Text.JsConfig.ConvertObjectTypesIntoStringDictionary = true;
            ServiceStack.Text.JsConfig.IncludeNullValues = false;
        }

        [Time]
        public Dictionary<String, String> AsDictionary<T>(T objectToConvert) where T : CommonParameters
        {            
            return ServiceStack.Text.TypeSerializer.ToStringDictionary<T>(objectToConvert);
        }

        [Time]
        public T AsPortalResponse<T>(String dataToConvert) where T : IPortalResponse
        {
            return ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(dataToConvert);
        }
    }

    public class JsonDotNetSerializer : ISerializer
    {
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
        
        [Time]
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

        [Time]
        public T AsPortalResponse<T>(String dataToConvert) where T : IPortalResponse
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dataToConvert, _settings);           
        }
    }
}
