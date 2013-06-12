using System;
using System.Collections.Generic;
using ArcGIS.ServiceModel.Logic;
using ArcGIS.ServiceModel.Operation;
using Newtonsoft.Json.Linq;

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

        public Dictionary<String, String> AsDictionary<T>(T objectToConvert) where T : CommonParameters
        {
            return objectToConvert == null ?
                null :
                ServiceStack.Text.JsonSerializer.DeserializeFromString<Dictionary<String, String>>(ServiceStack.Text.JsonSerializer.SerializeToString(objectToConvert));
        }

        public T AsPortalResponse<T>(String dataToConvert) where T : PortalResponse
        {
            return String.IsNullOrWhiteSpace(dataToConvert)
                ? null
                : ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(dataToConvert);
        }
    }

    public class JsonDotNetSerializer : ISerializer
    {
        readonly Newtonsoft.Json.JsonSerializerSettings _settings;

        public JsonDotNetSerializer()
        {
            _settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            };
        }

        public Dictionary<String, String> AsDictionary<T>(T objectToConvert) where T : CommonParameters
        {
            var stringValue = JObject.FromObject(objectToConvert).ToString();
            var jobject = JObject.Parse(stringValue);
            var dict = new Dictionary<String, String>();
            foreach (var item in jobject)
            {
                dict.Add(item.Key, item.Value.ToString());
            }
            return dict;
        }

        public T AsPortalResponse<T>(String dataToConvert) where T : PortalResponse
        {
            return String.IsNullOrWhiteSpace(dataToConvert)
                ? null
                : Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dataToConvert, _settings);
        }
    }
}
