using ArcGIS.ServiceModel.Common;

namespace ArcGIS.ServiceModel.Operation
{
    /// <summary>
    /// Base class for calls to an ArcGIS Server operation
    /// </summary>
    public abstract class ArcGISServerOperation : CommonParameters, IEndpoint
    {
        protected IEndpoint Endpoint;

        public string RelativeUrl { get { return Endpoint.RelativeUrl; } }

        public string BuildAbsoluteUrl(string rootUrl)
        {
            return Endpoint.BuildAbsoluteUrl(rootUrl);
        }
    }
}
