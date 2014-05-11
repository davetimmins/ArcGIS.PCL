using ArcGIS.ServiceModel.Operation;
using System;
using System.Collections.Generic;

namespace ArcGIS.ServiceModel.Serializers
{
    public class ServiceStackSerializer : ISerializer
    {
        static ISerializer _serializer = null;

        public static void Init()
        {
            _serializer = new ServiceStackSerializer();
            SerializerFactory.Get = (() => _serializer ?? new ServiceStackSerializer());
        }

        public ServiceStackSerializer()
        {
            ServiceStack.Text.JsConfig.EmitCamelCaseNames = true;
            ServiceStack.Text.JsConfig.IncludeTypeInfo = false;
            ServiceStack.Text.JsConfig.ConvertObjectTypesIntoStringDictionary = true;
            ServiceStack.Text.JsConfig.IncludeNullValues = false;
        }

        public Dictionary<String, String> AsDictionary<T>(T objectToConvert) where T : CommonParameters
        {
            return ServiceStack.Text.TypeSerializer.ToStringDictionary<T>(objectToConvert);
        }

        public T AsPortalResponse<T>(String dataToConvert) where T : IPortalResponse
        {
            return ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(dataToConvert);
        }
    }
}
