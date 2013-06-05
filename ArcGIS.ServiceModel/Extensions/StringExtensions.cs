using System;
using ArcGIS.ServiceModel.Common;

namespace ArcGIS.ServiceModel.Extensions
{
    public static class StringExtensions
    {
        public static ArcGISServerEndpoint AsEndpoint(this String relativeUrl)
        {
            return new ArcGISServerEndpoint(relativeUrl);
        }
    }
}
