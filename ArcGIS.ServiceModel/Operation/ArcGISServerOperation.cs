using System;
using ArcGIS.ServiceModel.Common;

namespace ArcGIS.ServiceModel.Operation
{
    /// <summary>
    /// Base class for calls to an ArcGIS Server operation
    /// </summary>
    public abstract class ArcGISServerOperation : CommonParameters, IEndpoint
    {
        readonly String _relativeUrl;

        protected ArcGISServerOperation(ArcGISServerEndpoint endpoint, String operationPath)
        {
            _relativeUrl = endpoint.RelativeUrl.Trim('/') + "/" + operationPath.Trim('/');
        }

        public string RelativeUrl { get { return _relativeUrl; } }

        public string BuildAbsoluteUrl(string rootUrl)
        {
            return !RelativeUrl.Contains(rootUrl.Substring(6)) && !RelativeUrl.Contains(rootUrl.Substring(6))
                       ? rootUrl + RelativeUrl
                       : RelativeUrl;
        }
    }
}
