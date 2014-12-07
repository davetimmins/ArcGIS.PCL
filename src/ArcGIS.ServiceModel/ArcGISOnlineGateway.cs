namespace ArcGIS.ServiceModel
{
    using ArcGIS.ServiceModel.Common;
    using ArcGIS.ServiceModel.Operation;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// ArcGIS Online gateway
    /// </summary>
    public class ArcGISOnlineGateway : PortalGatewayBase
    {
        /// <summary>
        /// Create an ArcGIS Online gateway to access resources
        /// </summary>
        /// <param name="serializer">Used to (de)serialize requests and responses</param>
        /// <param name="tokenProvider">Provide access to a token for secure resources</param>
        public ArcGISOnlineGateway(ISerializer serializer = null, ITokenProvider tokenProvider = null)
            : this(PortalGatewayBase.AGOPortalUrl, serializer, tokenProvider)
        { }

        public ArcGISOnlineGateway(string rootUrl, ISerializer serializer = null, ITokenProvider tokenProvider = null)
            : base(rootUrl, serializer, tokenProvider)
        { }

        protected override IEndpoint GeometryServiceEndpoint
        {
            get { return GeometryServiceIEndpoint ?? (GeometryServiceIEndpoint = (IEndpoint)GeometryServerUrl.AsAbsoluteEndpoint()); }
        }

        /// <summary>
        /// Search for feature services on ArcGIS Online / Portal for the user specified
        /// </summary>
        /// <param name="username">User whose content to search for, if not specified then the user
        /// from the <see cref="ITokenProvider" />  for this gateway will be used.</param>
        /// <returns>The discovered hosted feature services</returns>
        public Task<SearchHostedFeatureServicesResponse> DescribeSite(CancellationToken ct, string username = "")
        {
            if (string.IsNullOrWhiteSpace(username) && TokenProvider != null)
                username = TokenProvider.UserName;

            var search = string.IsNullOrWhiteSpace(username)
                ? new SearchHostedFeatureServices()
                : new SearchHostedFeatureServices(username);
            return Get<SearchHostedFeatureServicesResponse, SearchHostedFeatureServices>(search, ct);
        }

        public Task<SearchHostedFeatureServicesResponse> DescribeSite(string username = "")
        {
            return DescribeSite(CancellationToken.None, username);
        }
    }
}
