using ArcGIS.ServiceModel.Operation;
using System;
using System.Threading.Tasks;

namespace ArcGIS.ServiceModel
{
    /// <summary>
    /// Used for generating a token which can then be appended to requests made through a gateway
    /// </summary>
    public interface ITokenProvider
    {
        /// <summary>
        /// Made up of scheme://host:port/site
        /// </summary>
        String RootUrl { get; }

        /// <summary>
        /// Used for (de)serializtion of requests and responses. 
        /// </summary>
        ISerializer Serializer { get; }

        /// <summary>
        /// Returns a valid token for the corresponding request
        /// </summary>
        /// <returns>A token that can be used for subsequent requests to secure resources</returns>
        Task<Token> CheckGenerateToken(System.Threading.CancellationTokenSource cts = null);
    }
}
