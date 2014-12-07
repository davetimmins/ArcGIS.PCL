namespace ArcGIS.ServiceModel
{
    public class ArcGISOnlineFederatedTokenProvider : FederatedTokenProvider
    {
        public ArcGISOnlineFederatedTokenProvider(ITokenProvider tokenProvider, string serverUrl, ISerializer serializer = null, string referer = "https://www.arcgis.com")
            : base(tokenProvider, PortalGateway.AGOPortalUrl, serverUrl, serializer, referer)
        { }
    }
}
