using ArcGIS.ServiceModel.Operation;
using System;

namespace ArcGIS.ServiceModel
{
    public interface ICryptoProvider
    {
        GenerateToken Encrypt(GenerateToken tokenRequest, byte[] exponent, byte[] modulus);
    }
}
