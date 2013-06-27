ArcGIS.PCL
==========

Use ArcGIS Server REST resources without an official SDK.

Typical use case would be the need to call some ArcGIS REST resource from server .NET code or maybe a console app. Rather than having to fudge a dependency to an existing SDK you can use this. 
Should work with .NET for Windows Store apps, .NET framework 4.5, Silverlight 4 and higher, Windows Phone 7.5 and higher

Since the serialization is specific to your implementation you will need to create an ISerializer to use in your gateway. The test project has ServiceStack.Text and Json.NET [example serializers](https://github.com/davetimmins/ArcGIS.PCL/blob/dev/ArcGIS.Test/ISerializer.cs) 

Supports the following as typed operations:

 - Generate Token (automatically if credentials are specified in gateway)
 - Query
 - Apply Edits
 - Single Input Geocode
 - Reverse Geocode

See some of the [tests](https://github.com/davetimmins/ArcGIS.PCL/blob/dev/ArcGIS.Test/ArcGISGatewayTests.cs) for some example calls.

###Gateway with really bad serializer example  
```csharp
public class ArcGISGateway : PortalGateway
{
    public ArcGISGateway()
        : base(@"http://sampleserver3.arcgisonline.com/ArcGIS/")
    {
        Serializer = new Serializer();
    }
}

/// <summary>
/// Whatever you do, do not use this implementation. Ever. OK!
/// </summary>
public class Serializer : ISerializer
{
    public Dictionary<string, string> AsDictionary<T>(T objectToConvert) where T : CommonParameters
    {
        return new Dictionary<String, String>();
    }

    public T AsPortalResponse<T>(string dataToConvert) where T : PortalResponse
    {
        return (T)new PortalResponse();
    }
}
```
###Calling the above
```csharp
public class ArcGISGatewayExample
{
    public async Task CanPingServer()
    {
        await new ArcGISGateway().Ping(new ArcGISServerEndpoint("/"));
    }
}
```
### Download
If you have [NuGet](http://nuget.org) installed, the easiest way to get started is to install via NuGet:

    PM> Install-Package ArcGIS.PCL -Pre

or you can get the code from here.
