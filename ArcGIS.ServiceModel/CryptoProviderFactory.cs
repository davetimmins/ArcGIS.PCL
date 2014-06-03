using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGIS.ServiceModel
{
    public static class CryptoProviderFactory
    {
        public static Func<ICryptoProvider> Get { get; set; }

        static CryptoProviderFactory()
        {
            Get = (() => { return new RsaEncrypter(); });
        }
    }

    public class RsaEncrypter : ICryptoProvider
    {
        public Operation.GenerateToken Encrypt(Operation.GenerateToken tokenRequest, byte[] exponent, byte[] modulus)
        {
            if (exponent == null || modulus == null)
                throw new InvalidOperationException("Exponent and modulus must be set");

            using (var rsa = new System.Security.Cryptography.RSACryptoServiceProvider(512))
            {
                var rsaParms = new System.Security.Cryptography.RSAParameters
                {
                    Exponent = exponent,
                    Modulus = modulus
                };
                rsa.ImportParameters(rsaParms);

                var encryptedUsername = rsa.Encrypt(Encoding.UTF8.GetBytes(tokenRequest.Username), false).BytesToHex();
                var encryptedPassword = rsa.Encrypt(Encoding.UTF8.GetBytes(tokenRequest.Password), false).BytesToHex();
                var encryptedClient = String.IsNullOrWhiteSpace(tokenRequest.Client) ? "" : rsa.Encrypt(Encoding.UTF8.GetBytes(tokenRequest.Client), false).BytesToHex();
                var encryptedExpiration = rsa.Encrypt(Encoding.UTF8.GetBytes(tokenRequest.ExpirationInMinutes.ToString()), false).BytesToHex();

                tokenRequest.Encrypt(encryptedUsername, encryptedPassword, encryptedExpiration, encryptedClient);

                return tokenRequest;
            }
        }
    }
}
