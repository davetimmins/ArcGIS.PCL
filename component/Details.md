ArcGIS.PCL can be used to call ArcGIS Server resources, including those from Portal for ArcGIS and ArcGIS Online.

Available typed operations available are:

 - Generate Token (automatically if credentials are specified in gateway)
 - Query (attributes, spatial, count, objects Ids)
 - Find
 - Apply Edits
 - Single Input Geocode
 - Suggest
 - Reverse Geocode
 - Describe site (returns a url for every service)
 - Simplify
 - Project
 - Buffer

In addition to these you can use it to convert between GeoJSON and ArcGIS JSON features.

To get started with ArcGIS.PCL first create an ISerializer implementation. There is a Json.NET implementation packaged with the component that will be used by default if you initialise it

### Json.NET ISerializer initialisation

```csharp
ArcGIS.ServiceModel.Serializers.JsonDotNetSerializer.Init();
```
To call ArcGIS Server resources you can create a gateway. You pass in the root url of the ArcGIS Server that you want to call operations against. There are a mixture of secure, non secure and ArcGIS Online base classes available.

### ArcGIS Server gateway with non secure resources

```csharp
public class ArcGISGateway : PortalGateway
{
	public ArcGISGateway()
		: base(@"http://sampleserver3.arcgisonline.com/ArcGIS/")
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
var gateway = new ArcGISGateway();

var queryPoint = new Query(@"Earthquakes/EarthquakesFromLastSevenDays/MapServer/0".AsEndpoint());

var resultPoint = await gateway.Query<Point>(queryPoint);
```
