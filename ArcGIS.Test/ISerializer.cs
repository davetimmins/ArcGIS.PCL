using System;
using System.Collections.Generic;
using ArcGIS.ServiceModel;
using ArcGIS.ServiceModel.Operation;
using System.Diagnostics;

namespace ArcGIS.Test
{
    public class ServiceStackSerializer : ISerializer
    {
        long _totalAsDictionary = 0;
        long _totalAsPortalResponse = 0;

        public ServiceStackSerializer()
        {
            ServiceStack.Text.JsConfig.EmitCamelCaseNames = true;
            ServiceStack.Text.JsConfig.IncludeTypeInfo = false;
            ServiceStack.Text.JsConfig.ConvertObjectTypesIntoStringDictionary = true;
            ServiceStack.Text.JsConfig.IncludeNullValues = false;
        }

        public long AsDictionaryElapsedMilliseconds { get { return _totalAsDictionary; } }

        public long AsPortalResponseElapsedMilliseconds { get { return _totalAsPortalResponse; } }

        public Dictionary<String, String> AsDictionary<T>(T objectToConvert) where T : CommonParameters
        {
            var stopWatch = Stopwatch.StartNew();
            var res = ServiceStack.Text.TypeSerializer.ToStringDictionary<T>(objectToConvert);
            stopWatch.Stop();
            _totalAsDictionary += stopWatch.ElapsedMilliseconds;
            System.Diagnostics.Trace.TraceInformation("ServiceStack AsDictionary {0}", stopWatch.ElapsedMilliseconds);
            return res;
        }

        public T AsPortalResponse<T>(String dataToConvert) where T : IPortalResponse
        {
            var stopWatch = Stopwatch.StartNew();
            var result = ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(dataToConvert);
            stopWatch.Stop();
            _totalAsPortalResponse += stopWatch.ElapsedMilliseconds;
            System.Diagnostics.Trace.TraceInformation("ServiceStack AsPortalResponse {0}", stopWatch.ElapsedMilliseconds);
            return result;
        }
    }

    public class JsonDotNetSerializer : ISerializer
    {
        long _totalAsDictionary = 0;
        long _totalAsPortalResponse = 0;
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

        public long AsDictionaryElapsedMilliseconds { get { return _totalAsDictionary; } }

        public long AsPortalResponseElapsedMilliseconds { get { return _totalAsPortalResponse; } }

        public Dictionary<String, String> AsDictionary<T>(T objectToConvert) where T : CommonParameters
        {
            var stopWatch = Stopwatch.StartNew();
            var stringValue = Newtonsoft.Json.JsonConvert.SerializeObject(objectToConvert, _settings);
         
            var jobject = Newtonsoft.Json.Linq.JObject.Parse(stringValue);
            var dict = new Dictionary<String, String>();
            foreach (var item in jobject)
            {
                dict.Add(item.Key, item.Value.ToString());
            }
            stopWatch.Stop();
            _totalAsDictionary += stopWatch.ElapsedMilliseconds;
            System.Diagnostics.Trace.TraceInformation("Json.NET AsDictionary {0}", stopWatch.ElapsedMilliseconds);
            return dict;
        }

        public T AsPortalResponse<T>(String dataToConvert) where T : IPortalResponse
        {
            var stopWatch = Stopwatch.StartNew();
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dataToConvert, _settings);
            stopWatch.Stop();
            _totalAsPortalResponse += stopWatch.ElapsedMilliseconds;
            System.Diagnostics.Trace.TraceInformation("Json.NET AsPortalResponse {0}", stopWatch.ElapsedMilliseconds);
            return result;
        }
    }
}
