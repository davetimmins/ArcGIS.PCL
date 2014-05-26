#![Icon](https://raw.githubusercontent.com/davetimmins/ArcGIS.PCL/master/gateway.png) ArcGIS.PCL

[![NuGet Status](http://img.shields.io/badge/NuGet-3.2.1-blue.svg?style=flat)](https://www.nuget.org/packages/ArcGIS.PCL/) [![NuGet Status](http://img.shields.io/badge/Xamarin-3.2.1-blue.svg?style=flat)](https://components.xamarin.com/view/arcgis.pcl)

Use ArcGIS Server REST resources without an official SDK [more information](http://davetimmins.com/2013/July/ArcGIS-PCL/).

It can also be used for just working with types and as well as some ArcGIS Server types you can also use GeoJSON FeatureCollections with the ability to convert GeoJSON <-> ArcGIS Features.

Typical use case would be the need to call some ArcGIS REST resource from server .NET code or maybe a console app. Rather than having to fudge a dependency to an existing SDK you can use this. 
Works with .NET for Windows Store apps, .NET framework 4 and higher, Silverlight 5, Windows Phone 8 and higher and Xamarin iOS and Android.

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

 - [Describe site] (https://arcgissitedescriptor.azurewebsites.net/) - [code](https://github.com/davetimmins/ArcGIS.PCL-Sample-Projects/tree/master/ArcGIS%20Server%20Site%20Describer)
 - [Convert between GeoJSON and ArcGIS Features] (http://arcgisgeojson.azurewebsites.net/) - [code](https://github.com/davetimmins/ArcGIS.PCL-Sample-Projects/blob/master/Converter.Web/Interface/ConverterService.cs)
 - [Server side geometry operations] (http://eqnz.azurewebsites.net/) - [code](https://github.com/davetimmins/ArcGIS.PCL-Sample-Projects/tree/master/Earthquakes/Earthquakes.Web)
 - [Server side geocode] (http://loc8.azurewebsites.net/map?text=wellington, new zealand) - [code](https://github.com/davetimmins/ArcGIS.PCL-Sample-Projects/tree/master/ArcGISLocationMapper/ArcGISLocationMapper.Web)
 
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

### Installation
If you have [NuGet](http://nuget.org) installed, the easiest way to get started is to install via NuGet:

    PM> Install-Package ArcGIS.PCL

On Xamarin you can add the [ArcGIS.PCL component](http://components.xamarin.com/view/ArcGIS.PCL) from the component store or use the [NuGet addin](https://github.com/mrward/monodevelop-nuget-addin) and add it from there.

Of course you can also get the code from this site.

### Icon

Icon made by [Freepik](http://www.freepik.com) from [www.flaticon.com](http://www.flaticon.com/free-icon/triangle-of-triangles_32915)
                
