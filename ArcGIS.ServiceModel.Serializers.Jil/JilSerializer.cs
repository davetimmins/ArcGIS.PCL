using ArcGIS.ServiceModel.Operation;
using Jil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcGIS.ServiceModel.Serializers.Jil
{
    public class JilSerializer : ISerializer
    {
        static ISerializer _serializer = null;
        Options _options;

        public static void Init()
        {
            _serializer = new JilSerializer();
            SerializerFactory.Get = (() => _serializer ?? new JilSerializer());
        }

        public JilSerializer()
        {
            _options = new Options(excludeNulls: true, includeInherited: true);
        }

        public Dictionary<String, String> AsDictionary<T>(T objectToConvert) where T : CommonParameters
        {
            var stringValue = JSON.Serialize<T>(objectToConvert, _options);

            var result = JSON.Deserialize<Dictionary<String, object>>(stringValue, _options);
            return result.ToDictionary(k => k.Key, k => k.Value.ToString().Replace("\"", ""));
        }

        public T AsPortalResponse<T>(String dataToConvert) where T : IPortalResponse
        {
            return JSON.Deserialize<T>(dataToConvert, _options);
        }
    }
}
