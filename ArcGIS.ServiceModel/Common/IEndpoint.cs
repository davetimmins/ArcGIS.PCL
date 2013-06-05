using System;
using System.Runtime.Serialization;

namespace ArcGIS.ServiceModel.Common
{
    /// <summary>
    /// Represents a REST endpoint
    /// </summary>
    public interface IEndpoint
    {
        /// <summary>
        /// Relative url of the resource
        /// </summary>
        String RelativeUrl { get; }
    }

    /// <summary>
    /// Represents an ArcGIS Server REST endpoint
    /// </summary>
    public class ArcGISServerEndpoint : IEndpoint
    {
        /// <summary>
        /// Creates a new ArcGIS Server REST endpoint representation
        /// </summary>
        /// <param name="relativePath">Path of the endpoint relative to the root url of the ArcGIS Server</param>
        public ArcGISServerEndpoint(String relativePath)
        {
            RelativeUrl = relativePath.Trim('/');
        }
        
        public string RelativeUrl { get; private set; }
    }
}
