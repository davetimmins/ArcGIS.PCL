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

var gateway = new PortalGateway("http://sampleserver3.arcgisonline.com/ArcGIS/");

ArcGIS Server with secure resources

var secureGateway = new SecureArcGISServerGateway("http://serverapps10.esri.com/arcgis", "user1", "pass.word1");

ArcGIS Server with secure resources and token service at different location

var otherSecureGateway = new PortalGateway("http://sampleserver3.arcgisonline.com/ArcGIS/", tokenProvider: new TokenProvider("http://serverapps10.esri.com/arcgis", "user1", "pass.word1"));

ArcGIS Online either secure or non secure

var arcgisonlineGateway = new ArcGISOnlineGateway();
 
var secureArcGISOnlineGateway = new ArcGISOnlineGateway(tokenProvider: new ArcGISOnlineTokenProvider("user", "pass"));

var secureArcGISOnlineGatewayOAuth = new ArcGISOnlineGateway(tokenProvider: new ArcGISOnlineAppLoginOAuthProvider("clientId", "clientSecret"));

========================== Calling operations ===========================

Once you have a gateway you can call operations on it, for example to query an endpoint 

var queryPoint = new Query(@"Earthquakes/EarthquakesFromLastSevenDays/MapServer/0".AsEndpoint()) 
{ 
    ReturnGeometry = false 
};
var resultPoint = await gateway.Query<Point>(queryPoint);

