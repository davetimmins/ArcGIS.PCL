using ArcGIS.ServiceModel.Operation;

namespace ArcGIS.ServiceModel
{
    public interface ICryptoProvider
    {
        GenerateToken Encrypt(GenerateToken tokenRequest, byte[] exponent, byte[] modulus);
    }
}
