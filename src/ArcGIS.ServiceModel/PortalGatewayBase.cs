namespace ArcGIS.ServiceModel
{
    using Common;
    using Logging;
    using Operation;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// ArcGIS Server gateway base. Contains code to make HTTP(S) calls and operations available to all gateway types
    /// </summary>
    public class PortalGatewayBase : IPortalGateway, IDisposable
    {
        internal const string AGOPortalUrl = "https://www.arcgis.com/sharing/rest/";
        protected const string GeometryServerUrlRelative = "/Utilities/Geometry/GeometryServer";
        protected const string GeometryServerUrl = "https://utility.arcgisonline.com/arcgis/rest/services/Geometry/GeometryServer";
        HttpClient _httpClient;
        protected IEndpoint GeometryServiceIEndpoint;
        readonly ILog _logger;

        /// <summary>
        /// Create an ArcGIS Server gateway to access secure resources
        /// </summary>
        /// <param name="rootUrl">Made up of scheme://host:port/site</param>
        /// <param name="serializer">Used to (de)serialize requests and responses</param>
        /// <param name="tokenProvider">Provide access to a token for secure resources</param>
        /// <param name="httpClientFunc">Function that resolves to a HTTP client used to send requests</param>
        public PortalGatewayBase(string rootUrl, ISerializer serializer = null, ITokenProvider tokenProvider = null, Func<HttpClient> httpClientFunc = null)
            : this(() => LogProvider.For<PortalGatewayBase>(), rootUrl, serializer, tokenProvider, httpClientFunc)
        { }

        internal PortalGatewayBase(Func<ILog> log, string rootUrl, ISerializer serializer = null, ITokenProvider tokenProvider = null, Func<HttpClient> httpClientFunc = null)
        {
            if (string.IsNullOrWhiteSpace(rootUrl)) throw new ArgumentNullException("rootUrl", "rootUrl is null.");

            RootUrl = rootUrl.AsRootUrl();
            TokenProvider = tokenProvider;
            Serializer = serializer ?? SerializerFactory.Get();
            Guard.AgainstNullArgument("Serializer", Serializer);
            var httpFunc = httpClientFunc ?? HttpClientFactory.Get;
            _httpClient = httpFunc();

            _logger = log() ?? LogProvider.For<PortalGatewayBase>();
            _logger.DebugFormat("Created new gateway for {0}", RootUrl);
        }

        ~PortalGatewayBase()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_httpClient != null)
                {
                    _httpClient.Dispose();
                    _httpClient = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool CancelPendingRequests { get; set; }

        public bool IncludeHypermediaWithResponse { get; set; }

        public string RootUrl { get; private set; }

        public ITokenProvider TokenProvider { get; private set; }

        public ISerializer Serializer { get; private set; }

        protected virtual IEndpoint GeometryServiceEndpoint
        {
            get { return GeometryServiceIEndpoint ?? (GeometryServiceIEndpoint = (IEndpoint)GeometryServerUrlRelative.AsEndpoint()); }
        }

        /// <summary>
        /// Set the timespan until the web requests timesout
        /// </summary>
        public TimeSpan HttpRequestTimeout
        {
            get { return _httpClient.Timeout; }
            set { if (_httpClient != null)
                {
                    _httpClient.Timeout = value;
                }
            }
        }

        /// <summary>
        /// Pings the endpoint specified
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="ct">Optional cancellation token to cancel pending request</param>
        /// <returns>HTTP error if there is a problem with the request, otherwise an
        /// empty <see cref="IPortalResponse"/> object if successful otherwise the Error property is populated</returns>
        public virtual Task<PortalResponse> Ping(IEndpoint endpoint, CancellationToken ct = default(CancellationToken))
        {
            return Get<PortalResponse>(endpoint, ct);
        }

        /// <summary>
        /// Request the server information
        /// </summary>
        /// <param name="ct">Optional cancellation token to cancel pending request</param>
        /// <returns>Information about the server configuration (version, authentication settings etc.)</returns>
        public virtual Task<ServerInfoResponse> Info(CancellationToken ct = default(CancellationToken))
        {
            return Get<ServerInfoResponse>(new ServerInfo(), ct);
        }

        /// <summary>
        /// Call the query operation
        /// </summary>
        /// <typeparam name="T">The geometry type for the result set</typeparam>
        /// <param name="queryOptions">Query filter parameters</param>
        /// <param name="ct">Optional cancellation token to cancel pending request</param>
        /// <returns>The matching features for the query</returns>
        public virtual Task<QueryResponse<T>> Query<T>(Query queryOptions, CancellationToken ct = default(CancellationToken)) where T : IGeometry
        {
            return Get<QueryResponse<T>, Query>(queryOptions, ct);
        }

        /// <summary>
        /// Call the count operation for the query resource.
        /// </summary>
        /// <param name="queryOptions">Query filter parameters</param>
        /// <param name="ct">Optional cancellation token to cancel pending request</param>
        /// <returns>The number of results that match the query</returns>
        public virtual Task<QueryForCountResponse> QueryForCount(QueryForCount queryOptions, CancellationToken ct = default(CancellationToken))
        {
            return Get<QueryForCountResponse, QueryForCount>(queryOptions, ct);
        }

        /// <summary>
        /// Call the extent operation for the query resource.
        /// </summary>
        /// <param name="queryOptions">Query filter parameters</param>
        /// <param name="ct">Optional cancellation token to cancel pending request</param>
        /// <returns>The number of results that match the query and the bounding extent</returns>
        public virtual Task<QueryForExtentResponse> QueryForExtent(QueryForExtent queryOptions, CancellationToken ct = default(CancellationToken))
        {
            return Get<QueryForExtentResponse, QueryForExtent>(queryOptions, ct);
        }

        /// <summary>
        /// Call the object Ids query for the query resource
        /// </summary>
        /// <param name="queryOptions">Query filter parameters</param>
        /// <param name="ct">Optional cancellation token to cancel pending request</param>
        /// <returns>The Object IDs for the features that match the query</returns>
        public virtual Task<QueryForIdsResponse> QueryForIds(QueryForIds queryOptions, CancellationToken ct = default(CancellationToken))
        {
            return Get<QueryForIdsResponse, QueryForIds>(queryOptions, ct);
        }

        /// <summary>
        /// Call the apply edits operation for a feature service layer
        /// </summary>
        /// <typeparam name="T">The geometry type for the input set</typeparam>
        /// <param name="edits">The edits to perform</param>
        /// <param name="ct">Optional cancellation token to cancel pending request</param>
        /// <returns>A collection of add, update and delete results</returns>
        public virtual async Task<ApplyEditsResponse> ApplyEdits<T>(ApplyEdits<T> edits, CancellationToken ct = default(CancellationToken)) where T : IGeometry
        {
            var result = await Post<ApplyEditsResponse, ApplyEdits<T>>(edits, ct);
            result.SetExpected(edits);
            return result;
        }

        /// <summary>
        /// Projects the list of geometries passed in using the GeometryServer
        /// </summary>
        /// <typeparam name="T">The type of the geometries</typeparam>
        /// <param name="features">A collection of features which will have their geometries projected</param>
        /// <param name="outputSpatialReference">The spatial reference you want the result set to be</param>
        /// <param name="ct">Optional cancellation token to cancel pending request</param>
        /// <returns>The corresponding features with the newly projected geometries</returns>
        public virtual async Task<List<Feature<T>>> Project<T>(List<Feature<T>> features, SpatialReference outputSpatialReference, CancellationToken ct = default(CancellationToken)) where T : IGeometry
        {
            var op = new ProjectGeometry<T>(GeometryServiceEndpoint, features, outputSpatialReference);
            var projected = await Post<GeometryOperationResponse<T>, ProjectGeometry<T>>(op, ct).ConfigureAwait(false);

            if (ct.IsCancellationRequested) return null;

            var result = features.UpdateGeometries<T>(projected.Geometries);
            if (result.First().Geometry.SpatialReference == null) result.First().Geometry.SpatialReference = outputSpatialReference;
            return result;
        }

        public virtual async Task<List<Feature<T>>> Project<T>(ProjectGeometry<T> operation, CancellationToken ct = default(CancellationToken)) where T : IGeometry
        {
            var projected = await Post<GeometryOperationResponse<T>, ProjectGeometry<T>>(operation, ct).ConfigureAwait(false);

            if (ct.IsCancellationRequested) return null;

            var result = operation.Features.UpdateGeometries<T>(projected.Geometries);
            if (result.First().Geometry.SpatialReference == null) result.First().Geometry.SpatialReference = operation.OutputSpatialReference;
            return result;
        }

        /// <summary>
        /// Buffer the list of geometries passed in using the GeometryServer
        /// </summary>
        /// <typeparam name="T">The type of the geometries</typeparam>
        /// <param name="features">A collection of features which will have their geometries buffered</param>
        /// <param name="spatialReference">The spatial reference of the geometries</param>
        /// <param name="distance">Distance in meters to buffer the geometries by</param>
        /// <param name="ct">Optional cancellation token to cancel pending request</param>
        /// <returns>The corresponding features with the newly buffered geometries</returns>
        public virtual async Task<List<Feature<T>>> Buffer<T>(List<Feature<T>> features, SpatialReference spatialReference, double distance, CancellationToken ct = default(CancellationToken)) where T : IGeometry
        {
            var op = new BufferGeometry<T>(GeometryServiceEndpoint, features, spatialReference, distance);
            var buffered = await Post<GeometryOperationResponse<T>, BufferGeometry<T>>(op, ct).ConfigureAwait(false);

            if (ct.IsCancellationRequested) return null;

            var result = features.UpdateGeometries<T>(buffered.Geometries);
            if (result.First().Geometry.SpatialReference == null) result.First().Geometry.SpatialReference = spatialReference;
            return result;
        }

        /// <summary>
        /// Simplify the list of geometries passed in using the GeometryServer. Simplify permanently alters the input geometry so that it becomes topologically consistent.
        /// </summary>
        /// <typeparam name="T">The type of the geometries</typeparam>
        /// <param name="features">A collection of features which will have their geometries buffered</param>
        /// <param name="spatialReference">The spatial reference of the geometries</param>
        /// <param name="ct">Optional cancellation token to cancel pending request</param>
        /// <returns>The corresponding features with the newly simplified geometries</returns>
        public virtual async Task<List<Feature<T>>> Simplify<T>(List<Feature<T>> features, SpatialReference spatialReference, CancellationToken ct = default(CancellationToken)) where T : IGeometry
        {
            var op = new SimplifyGeometry<T>(GeometryServiceEndpoint, features, spatialReference);
            var simplified = await Post<GeometryOperationResponse<T>, SimplifyGeometry<T>>(op, ct).ConfigureAwait(false);

            if (ct.IsCancellationRequested) return null;

            var result = features.UpdateGeometries<T>(simplified.Geometries);
            if (result.First().Geometry.SpatialReference == null) result.First().Geometry.SpatialReference = spatialReference;
            return result;
        }

        async Task<Token> CheckGenerateToken(CancellationToken ct)
        {
            if (TokenProvider == null) return null;

            var token = await TokenProvider.CheckGenerateToken(ct).ConfigureAwait(false);

            if (token != null) CheckRefererHeader(token.Referer);
            return token;
        }

        void CheckRefererHeader(string referrer)
        {
            if (_httpClient == null || string.IsNullOrWhiteSpace(referrer)) return;

            Uri referer;
            bool validReferrerUrl = Uri.TryCreate(referrer, UriKind.Absolute, out referer);
            if (!validReferrerUrl)
            {
                throw new HttpRequestException(string.Format("Not a valid url for referrer: {0}", referrer));
            }
            _httpClient.DefaultRequestHeaders.Referrer = referer;
        }

        protected Task<T> Get<T, TRequest>(TRequest requestObject, CancellationToken ct)
            where TRequest : CommonParameters, IEndpoint
            where T : IPortalResponse
        {
            Guard.AgainstNullArgument("requestObject", requestObject);

            var url = requestObject.BuildAbsoluteUrl(RootUrl) + AsRequestQueryString(Serializer, requestObject);

            if (url.Length > 2047)
            {
                return Post<T, TRequest>(requestObject, ct);
            }
            return Get<T>(url, ct);
        }

        protected Task<T> Get<T>(IEndpoint endpoint, CancellationToken ct) where T : IPortalResponse
        {
            Guard.AgainstNullArgument("endpoint", endpoint);

            return Get<T>(endpoint.BuildAbsoluteUrl(RootUrl), ct);
        }

        protected async Task<T> Get<T>(string url, CancellationToken ct) where T : IPortalResponse
        {
            Guard.AgainstNullArgument("url", url);

            var token = await CheckGenerateToken(ct).ConfigureAwait(false);
            if (ct.IsCancellationRequested) return default(T);

            if (!url.Contains("f=")) url += (url.Contains("?") ? "&" : "?") + "f=json";
            if (token != null && !string.IsNullOrWhiteSpace(token.Value) && !url.Contains("token="))
            {
                url += (url.Contains("?") ? "&" : "?") + "token=" + token.Value;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token.Value);
                if (token.AlwaysUseSsl) url = url.Replace("http:", "https:");
            }

            Uri uri;
            bool validUrl = Uri.TryCreate(url, UriKind.Absolute, out uri);
            if (!validUrl)
            {
                throw new HttpRequestException(string.Format("Not a valid url: {0}", url));
            }
            _logger.DebugFormat("GET {0}", uri.AbsoluteUri);

            if (CancelPendingRequests)
            {
                _httpClient.CancelPendingRequests();
            }

            string resultString = string.Empty;
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(uri, ct).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                resultString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (TaskCanceledException tce)
            {
                _logger.WarnException("GET cancelled (exception swallowed)", tce);
                return default(T);
            }

            var result = Serializer.AsPortalResponse<T>(resultString);
            if (result.Error != null)
            {
                throw new InvalidOperationException(result.Error.ToString());
            }

            if (IncludeHypermediaWithResponse) result.Links = new List<Link> { new Link(uri.AbsoluteUri) };
            return result;
        }

        protected async Task<T> Post<T, TRequest>(TRequest requestObject, CancellationToken ct)
            where TRequest : CommonParameters, IEndpoint
            where T : IPortalResponse
        {
            Guard.AgainstNullArgument("requestObject", requestObject);

            var endpoint = requestObject;
            var parameters = Serializer.AsDictionary(requestObject);

            var url = endpoint.BuildAbsoluteUrl(RootUrl).Split('?').FirstOrDefault();

            var token = await CheckGenerateToken(ct).ConfigureAwait(false);
            if (ct.IsCancellationRequested) return default(T);

            // these should have already been added
            if (!parameters.ContainsKey("f")) parameters.Add("f", "json");
            if (!parameters.ContainsKey("token") && token != null && !string.IsNullOrWhiteSpace(token.Value))
            {
                parameters.Add("token", token.Value);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token.Value);
                if (token.AlwaysUseSsl) url = url.Replace("http:", "https:");
            }

            HttpContent content = null;
            try
            {
                content = new FormUrlEncodedContent(parameters);
            }
            catch (FormatException fex)
            {
                _logger.WarnException("POST format exception (exception swallowed)", fex);
                var tempContent = new MultipartFormDataContent();
                foreach (var keyValuePair in parameters)
                {
                    tempContent.Add(new StringContent(keyValuePair.Value), keyValuePair.Key);
                }
                content = tempContent;
            }

            if (CancelPendingRequests)
            {
                _httpClient.CancelPendingRequests();
            }

            Uri uri;
            bool validUrl = Uri.TryCreate(url, UriKind.Absolute, out uri);
            if (!validUrl)
            {
                throw new HttpRequestException(string.Format("Not a valid url: {0}", url));
            }
            _logger.DebugFormat("POST {0}", uri.AbsoluteUri);

            string resultString = string.Empty;
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(uri, content, ct).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                resultString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (TaskCanceledException tce)
            {
                _logger.WarnException("POST cancelled (exception swallowed)", tce);
                return default(T);
            }

            var result = Serializer.AsPortalResponse<T>(resultString);
            if (result.Error != null)
            {
                throw new InvalidOperationException(result.Error.ToString());
            }

            if (IncludeHypermediaWithResponse) result.Links = new List<Link> { new Link(uri.AbsoluteUri, requestObject) };
            return result;
        }

        internal static string AsRequestQueryString<T>(ISerializer serializer, T objectToConvert) where T : CommonParameters
        {
            Guard.AgainstNullArgument("serializer", serializer);
            Guard.AgainstNullArgument("objectToConvert", objectToConvert);

            var dictionary = serializer.AsDictionary(objectToConvert);

            return "?" + string.Join("&", dictionary.Keys.Select(k => string.Format("{0}={1}", k, dictionary[k].UrlEncode())));
        }
    }
}
