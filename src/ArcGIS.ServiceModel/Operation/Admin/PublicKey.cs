using ArcGIS.ServiceModel.Common;
using System;
using System.Runtime.Serialization;

namespace ArcGIS.ServiceModel.Operation.Admin
{
    [DataContract]
    public class PublicKey : ArcGISServerOperation
    {
        public PublicKey()
        {
            Endpoint = new ArcGISServerAdminEndpoint(Operations.PublicKey); 
        }
    }

    [DataContract]
    public class PublicKeyResponse : PortalResponse
    {
        [DataMember(Name = "publicKey")]
        public String PublicKey { get; set; }

        [DataMember(Name = "modulus")]
        public String Mod { get; set; }

        [IgnoreDataMember]
        public byte[] Exponent { get { return PublicKey.HexToBytes(); } }

        [IgnoreDataMember]
        public byte[] Modulus { get { return Mod.HexToBytes(); } }
    }
}
