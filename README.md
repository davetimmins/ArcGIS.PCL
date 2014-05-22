#![Icon](https://raw.githubusercontent.com/davetimmins/ArcGIS.PCL/master/gateway.png) ArcGIS.PCL

[![NuGet Status](http://img.shields.io/badge/NuGet-3.2.1-blue.svg?style=flat)](https://www.nuget.org/packages/ArcGIS.PCL/) [![NuGet Status](http://img.shields.io/badge/Xamarin-3.2.1-blue.svg?style=flat)](https://components.xamarin.com/view/arcgis.pcl)

Use ArcGIS Server REST resources without an official SDK [more information](http://davetimmins.com/2013/July/ArcGIS-PCL/).

It can also be used for just working with types and as well as some ArcGIS Server types you can also use GeoJSON FeatureCollections with the ability to convert GeoJSON <-> ArcGIS Features.

Typical use case would be the need to call some ArcGIS REST resource from server .NET code or maybe a console app. Rather than having to fudge a dependency to an existing SDK you can use this. 
Works with .NET for Windows Store apps, .NET framework 4.5, Silverlight 5, Windows Phone 8 and higher and Xamarin iOS and Android.

Since the serialization is specific to your implementation you will need to create an ISerializer to use in your gateway. There are NuGet packages created for 2 of these called `ArcGIS.PCL.JsonDotNetSerializer` and `ArcGIS.PCL.ServiceStackV3Serializer`. To use one of these add a reference using NuGet then call the static `Init()` method e.g. `ArcGIS.ServiceModel.Serializers.JsonDotNetSerializer.Init()`. This will create an `ISerializer` instance and override the `SerializerFactory.Get()` method so that it is returned when requested. This also means that you no longer have to pass the `ISerializer` to your gateway or token providers when initialising them, though you can still use this mechanism if you prefer.

Supports the following as typed operations:

 - `CheckGenerateToken` - create a token automatically via an `ITokenProvider`
 - `Query<T>` - query a layer by attribute and / or spatial filters
 - `QueryForCount` - only return the number of results for the query operation
 - `QueryForIds` - only return the ObjectIds for the results of the query operation
 - `Find` - search across n layers and fields in a service
 - `ApplyEdits<T>` - post adds, updates and deletes to a feature service layer
 - `Geocode` - single line of input to perform a geocode using a custom locator or the Esri world locator
 - `Suggest` - lightweight geocode operation that only returns text results, commonly used for predictive searching
 - `ReverseGeocode` - find location candidates for a input point location
 - `Simplify<T>` - alter geometries to be topologically consistent
 - `Project<T>` - convert geometries to a different spatial reference
 - `Buffer<T>` - buffers geometries by the distance requested
 - `DescribeSite` - returns a url for every service discovered
 - `Ping` - verify that the server can be accessed

Some examples of it in use for server side processing in web sites

 - [Describe site] (https://arcgissitedescriptor.azurewebsites.net/)
 - [Convert between GeoJSON and ArcGIS Features] (http://arcgisgeojson.azurewebsites.net/)
 - [Server side geometry operations] (http://eqnz.azurewebsites.net/)
 - [Server side geocode] (http://loc8.azurewebsites.net/map?text=wellington, new zealand)
 
The code for these can be seen at [ArcGIS.PCL-Sample-Projects](https://github.com/davetimmins/ArcGIS.PCL-Sample-Projects)

See some of the [tests](https://github.com/davetimmins/ArcGIS.PCL/blob/dev/ArcGIS.Test/ArcGISGatewayTests.cs) for some example calls.

###Gateway Use Cases

```csharp

// ArcGIS Server with non secure resources
var gateway = new PortalGateway("http://sampleserver3.arcgisonline.com/ArcGIS/");

// ArcGIS Server with secure resources
var secureGateway = new SecureArcGISServerGateway("http://serverapps10.esri.com/arcgis", "user1", "pass.word1");

// ArcGIS Server with secure resources and token service at different location
var otherSecureGateway = new PortalGateway("http://sampleserver3.arcgisonline.com/ArcGIS/", tokenProvider: new TokenProvider("http://serverapps10.esri.com/arcgis", "user1", "pass.word1"));

// ArcGIS Online either secure or non secure
var arcgisOnlineGateway = new ArcGISOnlineGateway();
 
var secureArcGISOnlineGateway = new ArcGISOnlineGateway(tokenProvider: new ArcGISOnlineTokenProvider("user", "pass"));

var secureArcGISOnlineGatewayOAuth = new ArcGISOnlineGateway(tokenProvider: new ArcGISOnlineAppLoginOAuthProvider("clientId", "clientSecret"));
```
### Converting between ArcGIS Feature Set from hosted FeatureService and GeoJSON FeatureCollection
```csharp
static Dictionary<String, Func<String, FeatureCollection<IGeoJsonGeometry>>> _funcMap = new Dictionary<String, Func<String, FeatureCollection<IGeoJsonGeometry>>>
{
    { GeometryTypes.Point, (uri) => new ProxyGateway(uri).GetGeoJson<Point>(uri) },
    { GeometryTypes.MultiPoint, (uri) => new ProxyGateway(uri).GetGeoJson<MultiPoint>(uri) },
    { GeometryTypes.Envelope, (uri) => new ProxyGateway(uri).GetGeoJson<Extent>(uri) },
    { GeometryTypes.Polygon, (uri) => new ProxyGateway(uri).GetGeoJson<Polygon>(uri) },
    { GeometryTypes.Polyline, (uri) => new ProxyGateway(uri).GetGeoJson<Polyline>(uri) }
};

...

var layer = new ProxyGateway(uri).GetAnything(uri.AsEndpoint());
if (layer == null || !layer.ContainsKey("geometryType")) throw new HttpException("You must enter a valid layer url.");
return _funcMap[layer["geometryType"]](uri);

...

public class AgsObject : JsonObject, IPortalResponse
{
    [System.Runtime.Serialization.DataMember(Name = "error")]
    public ArcGISError Error { get; set; }
}

public class ProxyGateway : PortalGateway
{
    public ProxyGateway(String rootUrl)
        : base(rootUrl)
    { }

    public Task<AgsObject> GetAnything(ArcGISServerEndpoint endpoint)
    {
        return Get<AgsObject>(endpoint);
    }

    public async Task<FeatureCollection<IGeoJsonGeometry>> GetGeoJson<T>(String uri) where T : IGeometry
    {
        var result = await Query<T>(new Query(uri.AsEndpoint()));
        result.Features.First().Geometry.SpatialReference = result.SpatialReference;
        var features = result.Features.ToList();
        if (result.SpatialReference.Wkid != SpatialReference.WGS84.Wkid)
            features = Project<T>(features, SpatialReference.WGS84);
        return features.ToFeatureCollection();
    }
}

```
### Converting between GeoJSON FeatureCollection and ArcGIS Feature Set
```csharp
static Dictionary<String, Func<String, List<Feature<IGeometry>>>> _funcMap = new Dictionary<String, Func<String, List<Feature<IGeometry>>>>
{
    { "Point", (data) => JsonSerializer.DeserializeFromString<FeatureCollection<GeoJsonPoint>>(data).ToFeatures<GeoJsonPoint>() },
    { "MultiPoint", (data) => JsonSerializer.DeserializeFromString<FeatureCollection<GeoJsonLineString>>(data).ToFeatures<GeoJsonLineString>() },
    { "LineString", (data) => JsonSerializer.DeserializeFromString<FeatureCollection<GeoJsonLineString>>(data).ToFeatures<GeoJsonLineString>() },
    { "MultiLineString", (data) => JsonSerializer.DeserializeFromString<FeatureCollection<GeoJsonLineString>>(data).ToFeatures<GeoJsonLineString>() },
    { "Polygon", (data) => JsonSerializer.DeserializeFromString<FeatureCollection<GeoJsonPolygon>>(data).ToFeatures<GeoJsonPolygon>() },
    { "MultiPolygon", (data) => JsonSerializer.DeserializeFromString<FeatureCollection<GeoJsonMultiPolygon>>(data).ToFeatures<GeoJsonMultiPolygon>() }
};

...

return _funcMap["Point"](data);
```

### Usage
If you have [NuGet](http://nuget.org) installed, the easiest way to get started is to install via NuGet:

    PM> Install-Package ArcGIS.PCL

On Xamarin you can add the [ArcGIS.PCL component](http://components.xamarin.com/view/ArcGIS.PCL) from the component store or use the [NuGet addin](https://github.com/mrward/monodevelop-nuget-addin) and add it from there.

Of course you can also get the code from this site.

### Icon

Icon made by [Freepik](http://www.freepik.com) from [www.flaticon.com](http://www.flaticon.com/free-icon/triangle-of-triangles_32915)
                
