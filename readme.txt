To get started with ArcGIS.PCL first create an ISerializer implementation

The easiest way is to add a reference to either ArcGIS.PCL.JsonDotNetSerializer or ArcGIS.PCL.ServiceStackV3Serializer via NuGet.

Now you can have this ISerializer used by your gateway and token providers automatically by initializing it

========================= Json.NET example ==============================

ArcGIS.ServiceModel.Serializers.JsonDotNetSerializer.Init();

==================== ServiceStack.Text example ==========================

ArcGIS.ServiceModel.Serializers.ServiceStackSerializer.Init();

Now you can go ahead and create your gateway classes

========================== Gateway use cases ============================

ArcGIS Server with non secure resources

public class ArcGISGateway : PortalGateway
{
    public ArcGISGateway()
        : base(@"http://sampleserver3.arcgisonline.com/ArcGIS/")
    { }
}

... new ArcGISGateway();

ArcGIS Server with secure resources

public class SecureGISGateway : SecureArcGISServerGateway
{
    public SecureGISGateway()
        : base(@"http://serverapps10.esri.com/arcgis", "user1", "pass.word1")
    { }
}

... new SecureGISGateway();

ArcGIS Server with secure resources and token service at different location

public class SecureTokenProvider : TokenProvider
{
    public SecureTokenProvider()
        : base("http://serverapps10.esri.com/arcgis", "user1", "pass.word1")
    { }
}
 
public class SecureGISGateway : PortalGateway
{
    public SecureGISGateway(ITokenProvider tokenProvider)
        : base("http://serverapps10.esri.com/arcgis", tokenProvider)
    { }
}
 
... new SecureGISGateway(new SecureTokenProvider());

ArcGIS Online either secure or non secure

... new ArcGISOnlineGateway();
 
... new ArcGISOnlineGateway(new ArcGISOnlineTokenProvider("user", "pass"));


Once you have a gateway you can add operations to it, for example to query an endpoint add the following to your gateway

public Task<QueryResponse<T>> Query<T>(Query queryOptions) where T : IGeometry
{
    return Get<QueryResponse<T>, Query>(queryOptions);
}

then call it from your code

var gateway = new ArcGISGateway(_serializer);

var queryPoint = new Query(@"Earthquakes/EarthquakesFromLastSevenDays/MapServer/0".AsEndpoint()) 
{ 
    ReturnGeometry = false 
};
var resultPoint = await gateway.Query<Point>(queryPoint);

