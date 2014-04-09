To get started with ArcGIS.PCL first create an ISerializer implementation

### Json.NET example

```csharp
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
```
### ArcGIS Server gateway with non secure resources

```csharp
public class ArcGISGateway : PortalGateway
{
    public ArcGISGateway(ISerializer serializer)
        : base(@"http://sampleserver3.arcgisonline.com/ArcGIS/", serializer)
    { }
}

```

Once you have a gateway you can add operations to it, for example to query an endpoint add the following to your gateway

```csharp
public Task<QueryResponse<T>> Query<T>(Query queryOptions) where T : IGeometry
{
    return Get<QueryResponse<T>, Query>(queryOptions);
}
```

then call it from your code

```csharp
var gateway = new ArcGISGateway(new JsonDotNetSerializer());

var queryPoint = new Query(@"Earthquakes/EarthquakesFromLastSevenDays/MapServer/0".AsEndpoint());

var resultPoint = await gateway.QueryAsGet<Point>(queryPoint);
```
