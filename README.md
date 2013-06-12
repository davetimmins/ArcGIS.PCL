ArcGIS.PCL
==========

Use ArcGIS Server REST resources without an official SDK.

Typical use case would be the need to call some ArcGIS REST resource from server .NET code or maybe a console app. Rather than having to fudge a dependency to an existing SDK you can use this. 
Should work with .NET for Windows Store apps, .NET framework 4.5, Silverlight 4 and higher, Windows Phone 7.5 and higher

See some of the [tests](https://github.com/davetimmins/ArcGIS.PCL/blob/dev/ArcGIS.Test/ArcGISGatewayTests.cs) for some example calls 

or this really bad example.
  
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
    
    public class ArcGISGatewayExample
    {
        public async Task CanPingServer()
        {
            var gateway = new ArcGISGateway();
    
            var endpoint = new ArcGISServerEndpoint("/");
    
            await gateway.Ping(endpoint);
        }
    }
