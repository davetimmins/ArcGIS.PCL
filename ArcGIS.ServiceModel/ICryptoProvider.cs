using ArcGIS.ServiceModel.Operation;
using System;

namespace ArcGIS.ServiceModel
{
    public interface ICryptoProvider
    {
        byte[] Exponent { get; set; }

        byte[] Modulus { get; set; }

        GenerateToken Encrypt(GenerateToken tokenRequest);
    }
}
