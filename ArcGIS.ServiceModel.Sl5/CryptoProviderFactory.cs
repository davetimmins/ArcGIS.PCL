using System;

namespace ArcGIS.ServiceModel
{
    public static class CryptoProviderFactory
    {
        public static Func<ICryptoProvider> Get { get; set; }

        public static bool Disabled { get; set; }

        static CryptoProviderFactory()
        {
            Get = (() => { return null; });
        }
    }
}
