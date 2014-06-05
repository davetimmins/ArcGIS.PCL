using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel.Operation;
using System.Net.Http.Headers;
using System.Threading;
using ArcGIS.ServiceModel.Operation.Admin;

namespace ArcGIS.ServiceModel
{
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
            : base(PortalGatewayBase.AGOPortalUrl, serializer, tokenProvider)
        { }
    }

    /// <summary>
    /// Provides a secure ArcGIS Server gateway where the token service is at the same root url
    /// </summary>
    public class SecureArcGISServerGateway : PortalGateway
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootUrl">The root url of ArcGIS Server and the token service</param>
        /// <param name="username">ArcGIS Server user name</param>
        /// <param name="password">ArcGIS Server user password</param>
        /// <param name="serializer">Used to (de)serialize requests and responses</param>
        public SecureArcGISServerGateway(String rootUrl, String username, String password, ISerializer serializer = null)
            : base(rootUrl, serializer, new TokenProvider(rootUrl, username, password, serializer))
        { }
    }

    /// <summary>
    /// ArcGIS Server gateway
    /// </summary>
    public class PortalGateway : PortalGatewayBase
    {
        public PortalGateway(String rootUrl, ISerializer serializer = null, ITokenProvider tokenProvider = null)
            : base(rootUrl, serializer, tokenProvider)
        { }

        /// <summary>
        /// Recursively parses an ArcGIS Server site and discovers the resources available
        /// </summary>
        /// <param name="cts">Optional cancellation token to cancel pending request</param>
        /// <returns>An ArcGIS Server site hierarchy</returns>
        public virtual async Task<SiteDescription> DescribeSite(CancellationTokenSource cts = null)
        {
            var result = new SiteDescription();

            result.Resources.AddRange(await DescribeEndpoint("/".AsEndpoint(), cts));
                      
            return result;
        }

        async Task<List<SiteFolderDescription>> DescribeEndpoint(IEndpoint endpoint, CancellationTokenSource cts = null)
        {
            SiteFolderDescription folderDescription = null;
            var result = new List<SiteFolderDescription>();
            try
            {
                folderDescription = await Get<SiteFolderDescription>(endpoint, cts);
            }
            catch (HttpRequestException ex)
            {
                // don't have access to the folder
                System.Diagnostics.Debug.WriteLine("HttpRequestException for Get SiteFolderDescription at path " + endpoint.RelativeUrl);
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return result;
            }
            if (cts != null && cts.IsCancellationRequested) return result;

            folderDescription.Path = endpoint.RelativeUrl;
            result.Add(folderDescription);

            if (folderDescription.Folders != null)
                foreach (var folder in folderDescription.Folders)
                {
                    result.AddRange(await DescribeEndpoint((endpoint.RelativeUrl + folder).AsEndpoint(), cts));
                }

            return result;
        }

        /// <summary>
        /// Returns the expected and actual status of a service
        /// </summary>
        /// <param name="serviceDescription">Service description usually generated from a previous call to DescribeSite</param>
        /// <param name="cts">Optional cancellation token to cancel pending request</param>
        /// <returns>The expected and actual status of the service</returns>
        public virtual Task<ServiceStatusResponse> ServiceStatus(ServiceDescription serviceDescription, CancellationTokenSource cts = null)
        {
            return Get<ServiceStatusResponse>(new ServiceStatus(serviceDescription), cts);
        }

        /// <summary>
        /// Call the reverse geocode operation. 
        /// </summary>
        /// <param name="reverseGeocode"></param>
        /// <param name="cts">Optional cancellation token to cancel pending request</param>
        /// <returns></returns>
        public virtual Task<ReverseGeocodeResponse> ReverseGeocode(ReverseGeocode reverseGeocode, CancellationTokenSource cts = null)
        {
            return Get<ReverseGeocodeResponse, ReverseGeocode>(reverseGeocode, cts);
        }

        /// <summary>
        /// Call the single line geocode operation.
        /// </summary>
        /// <param name="geocode"></param>
        /// <param name="cts">Optional cancellation token to cancel pending request</param>
        /// <returns></returns>
        public virtual Task<SingleInputGeocodeResponse> Geocode(SingleInputGeocode geocode, CancellationTokenSource cts = null)
        {
            return Get<SingleInputGeocodeResponse, SingleInputGeocode>(geocode, cts);
        }

        /// <summary>
        /// Call the suggest geocode operation.
        /// </summary>
        /// <param name="suggestGeocode"></param>
        /// <param name="cts">Optional cancellation token to cancel pending request</param>
        /// <returns></returns>
        public virtual Task<SuggestGeocodeResponse> Suggest(SuggestGeocode suggestGeocode, CancellationTokenSource cts = null)
        {
            return Get<SuggestGeocodeResponse, SuggestGeocode>(suggestGeocode, cts);
        }

        /// <summary>
        /// Call the find operation, note that since this can return more than one geometry type you will need to deserialize
        /// the geometry string on the result set e.g.
        /// foreach (var result in response.Results.Where(r => r.Geometry != null))
        /// {
        ///     result.Geometry = ServiceStack.Text.JsonSerializer.DeserializeFromString(result.Geometry.SerializeToString(), TypeMap[result.GeometryType]());
        /// }
        /// </summary>
        /// <param name="findOptions"></param>
        /// <param name="cts">Optional cancellation token to cancel pending request</param>
        /// <returns></returns>
        public virtual Task<FindResponse> Find(Find findOptions, CancellationTokenSource cts = null)
        {
            return Get<FindResponse, Find>(findOptions, cts);
        }
    }

    /// <summary>
    /// ArcGIS Server gateway base. Contains code to make HTTP(S) calls and operations available to all gateway types
    /// </summary>
    public class PortalGatewayBase : IPortalGateway, IDisposable
    {
        internal const String AGOPortalUrl = "http://www.arcgis.com/sharing/rest/";
        protected const String GeometryServerUrlRelative = "/Utilities/Geometry/GeometryServer";
        protected const String GeometryServerUrl = "https://utility.arcgisonline.com/arcgis/rest/services/Geometry/GeometryServer";
        IEndpoint _geometryServiceEndpoint;
        HttpClient _httpClient;

        /// <summary>
        /// Create an ArcGIS Server gateway to access secure resources
        /// </summary>
        /// <param name="rootUrl">Made up of scheme://host:port/site</param>
        /// <param name="serializer">Used to (de)serialize requests and responses</param>
        /// <param name="tokenProvider">Provide access to a token for secure resources</param>
        public PortalGatewayBase(String rootUrl, ISerializer serializer = null, ITokenProvider tokenProvider = null)
        {
            RootUrl = rootUrl.AsRootUrl();
            TokenProvider = tokenProvider;
            Serializer = serializer ?? SerializerFactory.Get();
            if (Serializer == null) throw new ArgumentNullException("serializer", "Serializer has not been set.");

            _httpClient = HttpClientFactory.Get();
            System.Diagnostics.Debug.WriteLine("Created PortalGateway for " + RootUrl);
        }

#if DEBUG
        ~PortalGatewayBase()
        {
            Dispose(false);
        }
#endif

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
#if DEBUG
            GC.SuppressFinalize(this);
#endif
        }

        public string RootUrl { get; private set; }

        public ITokenProvider TokenProvider { get; private set; }

        public ISerializer Serializer { get; private set; }

        void CreateGeometryServiceEndpoint()
        {
            if (_geometryServiceEndpoint != null) return;
            _geometryServiceEndpoint = String.Equals(RootUrl, AGOPortalUrl, StringComparison.OrdinalIgnoreCase)
                ? (IEndpoint)GeometryServerUrl.AsAbsoluteEndpoint()
                : (IEndpoint)GeometryServerUrlRelative.AsEndpoint();
        }

        /// <summary>
        /// Pings the endpoint specified
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="cts">Optional cancellation token to cancel pending request</param>
        /// <returns>HTTP error if there is a problem with the request, otherwise an
        /// empty <see cref="IPortalResponse"/> object if successful otherwise the Error property is populated</returns>
        public virtual Task<PortalResponse> Ping(IEndpoint endpoint, CancellationTokenSource cts = null)
        {
            return Get<PortalResponse>(endpoint, cts);
        }
        
        /// <summary>
        /// Call the query operation
        /// </summary>
        /// <typeparam name="T">The geometry type for the result set</typeparam>
        /// <param name="queryOptions">Query filter parameters</param>
        /// <param name="cts">Optional cancellation token to cancel pending request</param>
        /// <returns>The matching features for the query</returns>
        public virtual Task<QueryResponse<T>> Query<T>(Query queryOptions, CancellationTokenSource cts = null) where T : IGeometry
        {
            return Get<QueryResponse<T>, Query>(queryOptions, cts);
        }

        /// <summary>
        /// Call the count operation for the query resource.
        /// </summary>
        /// <param name="queryOptions">Query filter parameters</param>
        /// <param name="cts">Optional cancellation token to cancel pending request</param>
        /// <returns>The number of results that match the query</returns>
        public virtual Task<QueryForCountResponse> QueryForCount(QueryForCount queryOptions, CancellationTokenSource cts = null)
        {
            return Get<QueryForCountResponse, QueryForCount>(queryOptions, cts);
        }

        /// <summary>
        /// Call the object Ids query for the query resource
        /// </summary>
        /// <param name="queryOptions">Query filter parameters</param>
        /// <param name="cts">Optional cancellation token to cancel pending request</param>
        /// <returns>The Object IDs for the features that match the query</returns>
        public virtual Task<QueryForIdsResponse> QueryForIds(QueryForIds queryOptions, CancellationTokenSource cts = null)
        {
            return Get<QueryForIdsResponse, QueryForIds>(queryOptions, cts);
        }

        /// <summary>
        /// Call the apply edits operation for a feature service layer
        /// </summary>
        /// <typeparam name="T">The geometry type for the input set</typeparam>
        /// <param name="edits">The edits to perform</param>
        /// <param name="cts">Optional cancellation token to cancel pending request</param>
        /// <returns>A collection of add, update and delete results</returns>
        public virtual Task<ApplyEditsResponse> ApplyEdits<T>(ApplyEdits<T> edits, CancellationTokenSource cts = null) where T : IGeometry
        {
            return Post<ApplyEditsResponse, ApplyEdits<T>>(edits, cts);
        }
                
        /// <summary>
        /// Projects the list of geometries passed in using the GeometryServer
        /// </summary>
        /// <typeparam name="T">The type of the geometries</typeparam>
        /// <param name="features">A collection of features which will have their geometries projected</param>
        /// <param name="outputSpatialReference">The spatial reference you want the result set to be</param>
        /// <param name="cts">Optional cancellation token to cancel pending request</param>
        /// <returns>The corresponding features with the newly projected geometries</returns>
        public virtual async Task<List<Feature<T>>> Project<T>(List<Feature<T>> features, SpatialReference outputSpatialReference, CancellationTokenSource cts = null) where T : IGeometry
        {
            if (_geometryServiceEndpoint == null) CreateGeometryServiceEndpoint();

            var op = new ProjectGeometry<T>(_geometryServiceEndpoint, features, outputSpatialReference);
            var projected = await Post<GeometryOperationResponse<T>, ProjectGeometry<T>>(op, cts);

            if (cts != null && cts.IsCancellationRequested) return null;

            var result = features.UpdateGeometries<T>(projected.Geometries);
            if (result.First().Geometry.SpatialReference == null) result.First().Geometry.SpatialReference = outputSpatialReference;
            return result;
        }

        /// <summary>
        /// Buffer the list of geometries passed in using the GeometryServer
        /// </summary>
        /// <typeparam name="T">The type of the geometries</typeparam>
        /// <param name="features">A collection of features which will have their geometries buffered</param>
        /// <param name="spatialReference">The spatial reference of the geometries</param>
        /// <param name="distance">Distance in meters to buffer the geometries by</param>
        /// <param name="cts">Optional cancellation token to cancel pending request</param>
        /// <returns>The corresponding features with the newly buffered geometries</returns>
        public virtual async Task<List<Feature<T>>> Buffer<T>(List<Feature<T>> features, SpatialReference spatialReference, double distance, CancellationTokenSource cts = null) where T : IGeometry
        {
            if (_geometryServiceEndpoint == null) CreateGeometryServiceEndpoint();

            var op = new BufferGeometry<T>(_geometryServiceEndpoint, features, spatialReference, distance);
            var buffered = await Post<GeometryOperationResponse<T>, BufferGeometry<T>>(op, cts);

            if (cts != null && cts.IsCancellationRequested) return null;

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
        /// <param name="cts">Optional cancellation token to cancel pending request</param>
        /// <returns>The corresponding features with the newly simplified geometries</returns>
        public virtual async Task<List<Feature<T>>> Simplify<T>(List<Feature<T>> features, SpatialReference spatialReference, CancellationTokenSource cts = null) where T : IGeometry
        {
            if (_geometryServiceEndpoint == null) CreateGeometryServiceEndpoint();

            var op = new SimplifyGeometry<T>(_geometryServiceEndpoint, features, spatialReference);
            var simplified = await Post<GeometryOperationResponse<T>, SimplifyGeometry<T>>(op, cts);

            if (cts != null && cts.IsCancellationRequested) return null;

            var result = features.UpdateGeometries<T>(simplified.Geometries);
            if (result.First().Geometry.SpatialReference == null) result.First().Geometry.SpatialReference = spatialReference;
            return result;
        }
                
        async Task<Token> CheckGenerateToken(CancellationTokenSource cts = null)
        {
            if (TokenProvider == null) return null;

            var token = await TokenProvider.CheckGenerateToken(cts);

            if (token != null) CheckRefererHeader(token.Referer);
            return token;
        }

        void CheckRefererHeader(String referrer)
        {
            if (_httpClient == null || String.IsNullOrWhiteSpace(referrer)) return;

            Uri referer;
            bool validReferrerUrl = Uri.TryCreate(referrer, UriKind.Absolute, out referer);
            if (!validReferrerUrl)
                throw new HttpRequestException(String.Format("Not a valid url for referrer: {0}", referrer));
            _httpClient.DefaultRequestHeaders.Referrer = referer;
        }

        protected Task<T> Get<T, TRequest>(TRequest requestObject, CancellationTokenSource cts = null)
            where TRequest : CommonParameters, IEndpoint
            where T : IPortalResponse
        {
            var url = requestObject.BuildAbsoluteUrl(RootUrl) + AsRequestQueryString(Serializer, requestObject);

            if (url.Length > 2000)
                return Post<T>(requestObject, url.ParseQueryString());

            return Get<T>(url, cts);
        }

        protected Task<T> Get<T>(IEndpoint endpoint, CancellationTokenSource cts = null) where T : IPortalResponse
        {
            return Get<T>(endpoint.BuildAbsoluteUrl(RootUrl), cts);
        }

        protected async Task<T> Get<T>(String url, CancellationTokenSource cts = null) where T : IPortalResponse
        {
            var token = await CheckGenerateToken(cts);
            if (cts != null && cts.IsCancellationRequested) return default(T);

            if (!url.Contains("f=")) url += (url.Contains("?") ? "&" : "?") + "f=json";
            if (token != null && !String.IsNullOrWhiteSpace(token.Value) && !url.Contains("token="))
            {
                url += (url.Contains("?") ? "&" : "?") + "token=" + token.Value;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token.Value);
                if (token.AlwaysUseSsl) url = url.Replace("http:", "https:");
            }

            Uri uri;
            bool validUrl = Uri.TryCreate(url, UriKind.Absolute, out uri);
            if (!validUrl)
                throw new HttpRequestException(String.Format("Not a valid url: {0}", url));

            _httpClient.CancelPendingRequests();

            System.Diagnostics.Debug.WriteLine(uri);
            String resultString = String.Empty;
            try
            {
                HttpResponseMessage response = (cts == null) ? await _httpClient.GetAsync(uri) : await _httpClient.GetAsync(uri, cts.Token);
                response.EnsureSuccessStatusCode();

                resultString = await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException cex)
            {
                System.Diagnostics.Debug.WriteLine(cex.ToString());
                return default(T);
            }
            catch (HttpRequestException)
            {
                throw;
            }

            System.Diagnostics.Debug.WriteLine(resultString);
            var result = Serializer.AsPortalResponse<T>(resultString);
            if (result.Error != null) throw new InvalidOperationException(result.Error.ToString());

            return result;
        }
        
        protected Task<T> Post<T, TRequest>(TRequest requestObject, CancellationTokenSource cts = null)
            where TRequest : CommonParameters, IEndpoint
            where T : IPortalResponse
        {
            return Post<T>(requestObject, Serializer.AsDictionary(requestObject), cts);
        }

        protected async Task<T> Post<T>(IEndpoint endpoint, Dictionary<String, String> parameters, CancellationTokenSource cts = null) where T : IPortalResponse
        {
            var url = endpoint.BuildAbsoluteUrl(RootUrl).Split('?').FirstOrDefault();

            var token = await CheckGenerateToken(cts);
            if (cts != null && cts.IsCancellationRequested) return default(T);

            // these should have already been added
            if (!parameters.ContainsKey("f")) parameters.Add("f", "json");
            if (!parameters.ContainsKey("token") && token != null && !String.IsNullOrWhiteSpace(token.Value))
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
            catch (FormatException)
            {
                var tempContent = new MultipartFormDataContent();
                foreach (var keyValuePair in parameters)
                {
                    tempContent.Add(new StringContent(keyValuePair.Value), keyValuePair.Key);
                }
                content = tempContent;
            }
            _httpClient.CancelPendingRequests();

            Uri uri;
            bool validUrl = Uri.TryCreate(url, UriKind.Absolute, out uri);
            if (!validUrl)
                throw new HttpRequestException(String.Format("Not a valid url: {0}", url));

            System.Diagnostics.Debug.WriteLine(uri);
            String resultString = String.Empty;
            try
            {
                HttpResponseMessage response = cts == null ? await _httpClient.PostAsync(uri, content) : await _httpClient.PostAsync(uri, content, cts.Token);
                response.EnsureSuccessStatusCode();

                resultString = await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException cex)
            {
                System.Diagnostics.Debug.WriteLine(cex.ToString());
                return default(T);
            }
            catch (HttpRequestException)
            {
                throw;
            }

            System.Diagnostics.Debug.WriteLine(resultString);
            var result = Serializer.AsPortalResponse<T>(resultString);

            if (result.Error != null) throw new InvalidOperationException(result.Error.ToString());

            return result;
        }

        internal static String AsRequestQueryString<T>(ISerializer serializer, T objectToConvert) where T : CommonParameters
        {
            var dictionary = serializer.AsDictionary(objectToConvert);

            return "?" + String.Join("&", dictionary.Keys.Select(k => String.Format("{0}={1}", k, dictionary[k].UrlEncode())));
        }
    }
}
