using System;
using System.Collections.Generic;

namespace ArcGIS.ServiceModel
{
    /// <summary>
    /// Used to broker calls to ArcGIS Server, ArcGIS Online or ArcGIS for Portal
    /// </summary>
    public interface IPortalGateway
    {
        /// <summary>
        /// Made up of scheme://host:port/site
        /// </summary>
        String RootUrl { get; }

        ITokenProvider TokenProvider { get; }

        ISerializer Serializer { get; }
    }
}
