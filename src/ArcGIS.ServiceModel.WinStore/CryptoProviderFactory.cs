using System;
using System.IO;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace ArcGIS.ServiceModel
{
    public static class CryptoProviderFactory
    {
        public static Func<ICryptoProvider> Get { get; set; }

        public static bool Disabled { get; set; }

        static CryptoProviderFactory()
        {
            Get = (() => { return Disabled ? null : new RsaEncrypter(); });
        }
    }

    public class RsaEncrypter : ICryptoProvider
    {
        public Operation.GenerateToken Encrypt(Operation.GenerateToken tokenRequest, byte[] exponent, byte[] modulus)
        {
            if (exponent == null || modulus == null)
                throw new InvalidOperationException("Exponent and modulus must be set");

            byte[] bufferContent;

            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                writer.Write((byte)0x30); // SEQUENCE
                using (var innerStream = new MemoryStream())
                {
                    var innerWriter = new BinaryWriter(innerStream);
                    EncodeIntegerBigEndian(innerWriter, modulus);
                    EncodeIntegerBigEndian(innerWriter, exponent);
                    var length = (int)innerStream.Length;
                    EncodeLength(writer, length);
                    writer.Write(innerStream.ToArray(), 0, length);
                }

                bufferContent = stream.ToArray();
            }

            var encryptedUsername = Encrypt(bufferContent, tokenRequest.Username).BytesToHex();
            var encryptedPassword = Encrypt(bufferContent, tokenRequest.Password).BytesToHex();
            var encryptedClient = string.IsNullOrWhiteSpace(tokenRequest.Client) ? "" : Encrypt(bufferContent, tokenRequest.Client).BytesToHex();
            var encryptedExpiration = Encrypt(bufferContent, tokenRequest.ExpirationInMinutes.ToString()).BytesToHex();
            var encryptedReferer = string.IsNullOrWhiteSpace(tokenRequest.Referer) ? "" : Encrypt(bufferContent, tokenRequest.Referer).BytesToHex();

            tokenRequest.Encrypt(encryptedUsername, encryptedPassword, encryptedExpiration, encryptedClient, encryptedReferer);

            return tokenRequest;
        }

        static byte[] Encrypt(byte[] bufferContent, string data)
        {
            IBuffer keyBuffer = CryptographicBuffer.CreateFromByteArray(bufferContent);

            var asym = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaPkcs1);

            var key = asym.ImportPublicKey(keyBuffer, CryptographicPublicKeyBlobType.Pkcs1RsaPublicKey);

            IBuffer plainBuffer = CryptographicBuffer.ConvertStringToBinary(data, BinaryStringEncoding.Utf8);
            IBuffer encryptedBuffer = CryptographicEngine.Encrypt(key, plainBuffer, null);

            byte[] encryptedBytes;
            CryptographicBuffer.CopyToByteArray(encryptedBuffer, out encryptedBytes);

            return encryptedBytes;
        }

        static void EncodeLength(BinaryWriter stream, int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "Length must be non-negative");
            if (length < 0x80)
            {
                // Short form
                stream.Write((byte)length);
            }
            else
            {
                // Long form
                var temp = length;
                var bytesRequired = 0;
                while (temp > 0)
                {
                    temp >>= 8;
                    bytesRequired++;
                }
                stream.Write((byte)(bytesRequired | 0x80));
                for (var i = bytesRequired - 1; i >= 0; i--)
                {
                    stream.Write((byte)(length >> (8 * i) & 0xff));
                }
            }
        }

        static void EncodeIntegerBigEndian(BinaryWriter stream, byte[] value, bool forceUnsigned = true)
        {
            stream.Write((byte)0x02); // INTEGER
            var prefixZeros = 0;
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] != 0) break;
                prefixZeros++;
            }
            if (value.Length - prefixZeros == 0)
            {
                EncodeLength(stream, 1);
                stream.Write((byte)0);
            }
            else
            {
                if (forceUnsigned && value[prefixZeros] > 0x7f)
                {
                    // Add a prefix zero to force unsigned if the MSB is 1
                    EncodeLength(stream, value.Length - prefixZeros + 1);
                    stream.Write((byte)0);
                }
                else
                {
                    EncodeLength(stream, value.Length - prefixZeros);
                }
                for (var i = prefixZeros; i < value.Length; i++)
                {
                    stream.Write(value[i]);
                }
            }
        }
    }
}
