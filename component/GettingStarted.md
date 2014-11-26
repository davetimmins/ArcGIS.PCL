To get started with ArcGIS.PCL first create an ISerializer implementation. There is a Json.NET implementation packaged with the component that will be used by default if you initialise it

### Json.NET ISerializer initialisation

```csharp
ArcGIS.ServiceModel.Serializers.JsonDotNetSerializer.Init();
```
To call ArcGIS Server resources you can create a gateway. There are a mixture of secure, non secure and ArcGIS Online base classes available.

### ArcGIS Server gateway

```csharp
// ArcGIS Server with non secure resources
var gateway = new PortalGateway("http://sampleserver3.arcgisonline.com/ArcGIS/");
```

Once you have a gateway you can call operations on it, for example to query an endpoint

```csharp
var queryPoint = new Query(@"Earthquakes/EarthquakesFromLastSevenDays/MapServer/0".AsEndpoint());

var resultPoint = await gateway.Query<Point>(queryPoint);
```
