namespace ArcGIS.ServiceModel.Serializers
{
    using ArcGIS.ServiceModel;
    using ArcGIS.ServiceModel.Operation;
    using System.Collections.Generic;

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
            ServiceStack.Text.JsConfig.ExcludeTypeInfo = true;
            ServiceStack.Text.JsConfig.ConvertObjectTypesIntoStringDictionary = true;
            ServiceStack.Text.JsConfig.IncludeNullValues = false;
        }

        public Dictionary<string, string> AsDictionary<T>(T objectToConvert) where T : CommonParameters
        {
            return ServiceStack.Text.TypeSerializer.ToStringDictionary<T>(objectToConvert);
        }

        public T AsPortalResponse<T>(string dataToConvert) where T : IPortalResponse
        {
            return ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(dataToConvert);
        }
    }
}
