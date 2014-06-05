using System;
using ArcGIS.ServiceModel.Common;

namespace ArcGIS.ServiceModel.Operation
{
    /// <summary>
    /// Base class for calls to an ArcGIS Server operation
    /// </summary>
    public abstract class ArcGISServerOperation : CommonParameters, IEndpoint
    {
        protected IEndpoint Endpoint;

        public String RelativeUrl { get { return Endpoint.RelativeUrl; } }

        public String BuildAbsoluteUrl(String rootUrl)
        {
            return Endpoint.BuildAbsoluteUrl(rootUrl);
        }
    }
}
