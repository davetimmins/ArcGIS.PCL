ArcGIS.PCL
==========

Use ArcGIS Server REST resources without an official SDK [more information](http://davetimmins.wordpress.com/2013/07/11/arcgis-pclthe-what-why-how/).

Typical use case would be the need to call some ArcGIS REST resource from server .NET code or maybe a console app. Rather than having to fudge a dependency to an existing SDK you can use this. 
Should work with .NET for Windows Store apps, .NET framework 4.5, Silverlight 4 and higher, Windows Phone 7.5 and higher

Since the serialization is specific to your implementation you will need to create an ISerializer to use in your gateway. The test project has ServiceStack.Text and Json.NET [example serializers](https://github.com/davetimmins/ArcGIS.PCL/blob/dev/ArcGIS.Test/ISerializer.cs) 

Supports the following as typed operations:

 - Generate Token (automatically if credentials are specified in gateway)
 - Query
 - Apply Edits
 - Single Input Geocode
 - Reverse Geocode
 - Describe site (returns a url for every service)

See some of the [tests](https://github.com/davetimmins/ArcGIS.PCL/blob/dev/ArcGIS.Test/ArcGISGatewayTests.cs) for some example calls.

###Gateway with ServiceStack.Text Json serializer example  
```csharp
public class ArcGISGateway : PortalGateway
{
    public ArcGISGateway()
        : base(@"http://sampleserver3.arcgisonline.com/ArcGIS/")
    {
        Serializer = new ServiceStackSerializer();
    }
}

public class ServiceStackSerializer : ISerializer
{
    public ServiceStackSerializer()
    {
        JsConfig.EmitCamelCaseNames = true;
        JsConfig.IncludeTypeInfo = false;
        JsConfig.ConvertObjectTypesIntoStringDictionary = true;
        JsConfig.IncludeNullValues = false;
    }

    public Dictionary<String, String> AsDictionary<T>(T objectToConvert) where T : CommonParameters
    {
        return ServiceStack.Text.TypeSerializer.ToStringDictionary<T>(objectToConvert);
    }

    public T AsPortalResponse<T>(String dataToConvert) where T : IPortalResponse
    {
        return JsonSerializer.DeserializeFromString<T>(dataToConvert);
    }
}
```
###Calling the above
```csharp
public class ArcGISGatewayExample
{
    public async Task PingServer()
    {
        await new ArcGISGateway().Ping(new ArcGISServerEndpoint("/"));
    }
}
```
### Download
If you have [NuGet](http://nuget.org) installed, the easiest way to get started is to install via NuGet:

    PM> Install-Package ArcGIS.PCL

or you can get the code from here.
