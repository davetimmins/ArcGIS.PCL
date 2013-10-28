To get started first create an ISerializer implementation

ServiceStack.Text example

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
        return ServiceStack.Text.TypeSerializer.ToStringDictionary<T>(objectToConvert);
    }

    public T AsPortalResponse<T>(String dataToConvert) where T : IPortalResponse
    {
        return ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(dataToConvert);
    }
}

Json.NET example

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

Gateway use cases

ArcGIS Server with non secure resources

public class ArcGISGateway : PortalGateway
{
    public ArcGISGateway(ISerializer serializer)
        : base(@"http://sampleserver3.arcgisonline.com/ArcGIS/", serializer)
    { }
}

... new ArcGISGateway(serializer);

ArcGIS Server with secure resources

public class SecureGISGateway : SecureArcGISServerGateway
{
    public SecureGISGateway(ISerializer serializer)
        : base(@"http://serverapps10.esri.com/arcgis", "user1", "pass.word1", serializer)
    { }
}

... new SecureGISGateway(serializer);

ArcGIS Server with secure resources and token service at different location

public class SecureTokenProvider : TokenProvider
{
    public SecureTokenProvider(ISerializer serializer)
        : base("http://serverapps10.esri.com/arcgis", "user1", "pass.word1", serializer)
    { }
}
 
public class SecureGISGateway : PortalGateway
{
    public SecureGISGateway(ISerializer serializer, ITokenProvider tokenProvider)
        : base("http://serverapps10.esri.com/arcgis", serializer, tokenProvider)
    { }
}
 
... new SecureGISGateway(serializer, new SecureTokenProvider(serializer));

ArcGIS Online either secure or non secure

... new ArcGISOnlineGateway(serializer);
 
... new ArcGISOnlineGateway(serializer, new ArcGISOnlineTokenProvider("user", "pass", serializer));



