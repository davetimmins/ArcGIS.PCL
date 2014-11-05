using ArcGIS.ServiceModel.Operation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcGIS.ServiceModel.Serializers
{
    public class SimpleJsonSerializer : ISerializer
    {
        static ISerializer _serializer = null;

        public static void Init()
        {
            _serializer = new SimpleJsonSerializer();
            SerializerFactory.Get = (() => _serializer ?? new SimpleJsonSerializer());
        }

        public Dictionary<String, String> AsDictionary<T>(T objectToConvert) where T : CommonParameters
        {
            var stringValue = SimpleJson.SerializeObject(objectToConvert, SimpleJson.DataContractJsonSerializerStrategy);

            var result = SimpleJson.DeserializeObject<Dictionary<String, object>>(stringValue, SimpleJson.DataContractJsonSerializerStrategy);

            return result.Where(i => i.Value != null).ToDictionary(k => k.Key, k => k.Value.ToString().Replace("\"", ""));
        }

        public T AsPortalResponse<T>(String dataToConvert) where T : IPortalResponse
        {
            return SimpleJson.DeserializeObject<T>(dataToConvert, SimpleJson.DataContractJsonSerializerStrategy);
        }
    }
}
