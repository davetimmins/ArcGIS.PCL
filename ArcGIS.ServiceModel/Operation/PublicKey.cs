using ArcGIS.ServiceModel.Common;
using System;
using System.Runtime.Serialization;

namespace ArcGIS.ServiceModel.Operation
{
    [DataContract]
    public class PublicKey : CommonParameters, ArcGIS.ServiceModel.Common.IEndpoint
    {
        ArcGISServerAdminEndpoint _endpoint;

        public PublicKey()
        {
            _endpoint = "publicKey".AsAdminEndpoint();
        }

        public String RelativeUrl
        {
            get { return _endpoint.RelativeUrl; }
        }

        public String BuildAbsoluteUrl(String rootUrl)
        {
            return _endpoint.BuildAbsoluteUrl(rootUrl);
        }
    }

    [DataContract]
    public class PublicKeyResponse : PortalResponse
    {
        [DataMember(Name = "publicKey")]
        public string PublicKey { get; set; }

        [DataMember(Name = "modulus")]
        public string Mod { get; set; }

        [IgnoreDataMember]
        public byte[] Exponent { get { return PublicKey.HexToBytes(); } }

        [IgnoreDataMember]
        public byte[] Modulus { get { return Mod.HexToBytes(); } }
    }
}
