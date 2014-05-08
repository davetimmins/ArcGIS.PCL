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

        ISerializer Serializer { get; }

        Task<Token> CheckGenerateToken();
    }
}
