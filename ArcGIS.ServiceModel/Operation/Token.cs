using System;
using System.Runtime.Serialization;
using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel.Extensions;

namespace ArcGIS.ServiceModel.Operation
{
    [DataContract]
    public class GenerateToken : CommonParameters, IEndpoint
    {
        public GenerateToken()
        {
            Client = "referer";
            Referer = "requestip";
        }

        [DataMember(Name = "token")]
        public String Client { get; set; }

        [DataMember(Name = "token")]
        public String Referer { get; set; }

        [DataMember(Name = "username")]
        public String Username { get; set; }

        [DataMember(Name = "password")]
        public String Password { get; set; }

        public string RelativeUrl
        {
            get { return "/tokens/generateToken"; }
        }
    }

    [DataContract]
    public class Token : PortalResponse
    {
        [DataMember(Name = "token")]
        public String Value { get; set; }

        /// <summary>
        /// The expiration time of the token in milliseconds since Jan 1st, 1970.
        /// </summary>
        [DataMember(Name = "expires")]
        public long Expiry { get; set; }

        /// <summary>
        /// If we have a token value then check if it has expired
        /// </summary>
        [IgnoreDataMember]
        public bool IsExpired
        {
            get { return !String.IsNullOrWhiteSpace(Value) && Expiry > 0 && DateTime.Compare(Expiry.FromUnixTime(), DateTime.UtcNow) > 0; }
        }

        /// <summary>
        /// True if the token must always pass over ssl.
        /// </summary>
        [DataMember(Name = "ssl")]
        public String AlwaysUseSsl { get; set; }
    }
}
