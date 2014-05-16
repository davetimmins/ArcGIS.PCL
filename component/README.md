ArcGIS.PCL
==========

Use ArcGIS Server REST resources without an official SDK [more information](http://davetimmins.com/2013/July/ArcGIS-PCL/).

It can also be used for just working with types and as well as some ArcGIS Server types you can also use GeoJSON FeatureCollections with the ability to convert GeoJSON <-> ArcGIS Features.

Typical use case would be the need to call some ArcGIS REST resource from server .NET code or maybe a console app. Rather than having to fudge a dependency to an existing SDK you can use this.
Should work with .NET for Windows Store apps, .NET framework 4.5, Silverlight 4 and higher, Windows Phone 7.5 and higher

Supports the following as typed operations:

 - Generate Token (automatically if using a token provider)
 - Query (attributes, spatial, count, objects Ids)
 - Find
 - Apply Edits
 - Single Input Geocode (custom locator or Esri world locator)
 - Suggest Geocode
 - Reverse Geocode
 - Describe site (returns a url for every service)
 - Simplify
 - Project
 - Buffer

Some example of it in use for server side processing in web sites

 - [Describe site] (https://arcgissitedescriptor.azurewebsites.net/)
 - [Convert between GeoJSON and ArcGIS Features] (http://arcgisgeojson.azurewebsites.net/)
 - [Server side geometry operations] (http://eqnz.azurewebsites.net/)
 - [Server side geocode] (http://loc8.azurewebsites.net/map?text=wellington, new zealand)

The code for these can be seen at [ArcGIS.PCL-Sample-Projects](https://github.com/davetimmins/ArcGIS.PCL-Sample-Projects)

See some of the [tests](https://github.com/davetimmins/ArcGIS.PCL/blob/dev/ArcGIS.Test/ArcGISGatewayTests.cs) for some example calls.

To get started with ArcGIS.PCL first create an ISerializer implementation. There is a Json.NET implementation packaged with the component that will be used by default if you initialise it

### Json.NET ISerializer initialisation

```csharp

ArcGIS.ServiceModel.Serializers.JsonDotNetSerializer.Init();

```

To call ArcGIS Server resources you can create a gateway. You pass in the root url of the ArcGIS Server that you want to call operations against. There are a mixture of secure, non secure and ArcGIS Online base classes available.

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
 
var secureArcGISOnlineGateway = new ArcGISOnlineGateway(new ArcGISOnlineTokenProvider("user", "pass"));
```

### Converting between ArcGIS Feature Set from hosted FeatureService and GeoJSON FeatureCollection
```csharp
static ISerializer _serializer = new ServiceStackSerializer();
static Dictionary<String, Func<String, FeatureCollection<IGeoJsonGeometry>>> _funcMap = new Dictionary<String, Func<String, FeatureCollection<IGeoJsonGeometry>>>
{
    { GeometryTypes.Point, (uri) => new ProxyGateway(uri, _serializer).GetGeoJson<Point>(uri) },
    { GeometryTypes.MultiPoint, (uri) => new ProxyGateway(uri, _serializer).GetGeoJson<MultiPoint>(uri) },
    { GeometryTypes.Envelope, (uri) => new ProxyGateway(uri, _serializer).GetGeoJson<Extent>(uri) },
    { GeometryTypes.Polygon, (uri) => new ProxyGateway(uri, _serializer).GetGeoJson<Polygon>(uri) },
    { GeometryTypes.Polyline, (uri) => new ProxyGateway(uri, _serializer).GetGeoJson<Polyline>(uri) }
};

...

var layer = new ProxyGateway(uri, _serializer).GetAnything(uri.AsEndpoint());
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

    public QueryResponse<T> Query<T>(Query queryOptions) where T : IGeometry
    {
        return Get<QueryResponse<T>, Query>(queryOptions).Result;
    }

    public AgsObject GetAnything(ArcGISServerEndpoint endpoint)
    {
        return Get<AgsObject>(endpoint).Result;
    }

    public FeatureCollection<IGeoJsonGeometry> GetGeoJson<T>(String uri) where T : IGeometry
    {
        var result = Query<T>(new Query(uri.AsEndpoint()));
        result.Features.First().Geometry.SpatialReference = result.SpatialReference;
        var features = result.Features.ToList();
        if (result.SpatialReference.Wkid != SpatialReference.WGS84.Wkid)
            features = new ProjectGateway(Serializer).Project<T>(features, SpatialReference.WGS84);
        return features.ToFeatureCollection();
    }
}

public class ProjectGateway : PortalGateway
{
    public ProjectGateway()
        : base(@"http://services.arcgisonline.co.nz/ArcGIS/")
    { }

    public List<Feature<T>> Project<T>(List<Feature<T>> features, SpatialReference outputSpatialReference) where T : IGeometry
    {
        var op = new ProjectGeometry<T>("/Utilities/Geometry/GeometryServer".AsEndpoint(), features, outputSpatialReference);
        var projected = await Post<GeometryOperationResponse<T>, ProjectGeometry<T>>(op).Result;

        var result = features.UpdateGeometries<T>(projected.Geometries);
        if (result.First().Geometry.SpatialReference == null) result.First().Geometry.SpatialReference = outputSpatialReference;
        return result;
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
### Icon

[Gateway](http://thenounproject.com/term/gateway/5477/) designed by [Piotr Gawiński](http://thenounproject.com/Piotrek/) from The Noun Project
