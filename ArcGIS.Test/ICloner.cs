using ArcGIS.ServiceModel;
using System;

namespace ArcGIS.Test
{
    public class ServiceStackCloner<T> : ICloner<T>
    {
        public T DeepCopy(T objectToDeepCopy)
        {
            String json = ServiceStack.Text.TypeSerializer.SerializeToString<T>(objectToDeepCopy);
            return ServiceStack.Text.TypeSerializer.DeserializeFromString<T>(json); 
        }
    }

    public class JsonDotNetCloner<T> : ICloner<T>
    {
        public T DeepCopy(T objectToDeepCopy)
        {
            String json = Newtonsoft.Json.JsonConvert.SerializeObject(objectToDeepCopy);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }
    }
}
