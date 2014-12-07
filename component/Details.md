ArcGIS.PCL can be used to call ArcGIS Server resources, including those from Portal for ArcGIS and ArcGIS Online. The resources can be secure or unsecure and the ArcGIS Online token service and OAuth token service are supported.

Available operations available are:

 - `CheckGenerateToken` - create a token automatically via an `ITokenProvider`
 - `Query<T>` - query a layer by attribute and / or spatial filters
 - `QueryForCount` - only return the number of results for the query operation
 - `QueryForIds` - only return the ObjectIds for the results of the query operation
 - `Find` - search across n layers and fields in a service
 - `ApplyEdits<T>` - post adds, updates and deletes to a feature service layer
 - `Geocode` - single line of input to perform a geocode usning a custom locator or the Esri world locator
 - `Suggest` - lightweight geocode operation that only returns text results, commonly used for predictive searching
 - `ReverseGeocode` - find location candidates for a input point location
 - `Simplify<T>` - alter geometries to be topologically consistent
 - `Project<T>` - convert geometries to a different spatial reference
 - `Buffer<T>` - buffers geometries by the distance requested
 - `DescribeSite` - returns a url for every service discovered
 - `Ping` - verify that the server can be accessed

REST admin operations:

  - `PublicKey` - admin operation to get public key used for encryption of token requests
  - `ServiceStatus` - admin operation to get the configured and actual status of a service
  - `ServiceReport` - admin operation to get the service report
  - `StartService` - admin operation to start a service
  - `StopService` - admin operation to stop a service

In addition to these you can use it to convert between GeoJSON and ArcGIS JSON features.

To get started with ArcGIS.PCL first create an ISerializer implementation. There is a Json.NET implementation packaged with the component that will be used by default if you initialise it

### Json.NET ISerializer initialisation

```csharp
ArcGIS.ServiceModel.Serializers.JsonDotNetSerializer.Init();

```

To call ArcGIS Server resources you can create a gateway. You pass in the root url of the ArcGIS Server that you want to call operations against. There are a mixture of secure, non secure and ArcGIS Online base classes available.

### ArcGIS Server gateway

```csharp
// ArcGIS Server with non secure resources
var gateway = new PortalGateway("http://sampleserver3.arcgisonline.com/ArcGIS/");

// ArcGIS Server with secure resources
var secureGateway = new SecurePortalGateway("http://serverapps10.esri.com/arcgis", "user1", "pass.word1");

// ArcGIS Server with secure resources and token service at different location
var otherSecureGateway = new PortalGateway("http://sampleserver3.arcgisonline.com/ArcGIS/", tokenProvider: new TokenProvider("http://serverapps10.esri.com/arcgis", "user1", "pass.word1"));

// ArcGIS Online either secure or non secure
var arcgisOnlineGateway = new ArcGISOnlineGateway();

var secureArcGISOnlineGateway = new ArcGISOnlineGateway(tokenProvider: new ArcGISOnlineTokenProvider("user", "pass"));

var secureArcGISOnlineGatewayOAuth = new ArcGISOnlineGateway(tokenProvider: new ArcGISOnlineAppLoginOAuthProvider("clientId", "clientSecret"));
```

Once you have a gateway you can call operations on it, for example to query an endpoint

```csharp
var queryPoint = new Query(@"Earthquakes/EarthquakesFromLastSevenDays/MapServer/0".AsEndpoint());

var resultPoint = await gateway.Query<Point>(queryPoint);
```

### Icon

Icon made by [Freepik](http://www.freepik.com) from [www.flaticon.com](http://www.flaticon.com/free-icon/triangle-of-triangles_32915)
