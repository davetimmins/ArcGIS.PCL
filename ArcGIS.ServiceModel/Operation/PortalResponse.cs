using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ArcGIS.ServiceModel.Operation
{
    public interface IPortalResponse
    {
        [DataMember(Name = "error")]
        ArcGISError Error { get; set; }

        [DataMember(Name = "_links")]
        List<Link> Links { get; set; }
    }

    /// <summary>
    /// Common response object from an ArcGIS Server REST call
    /// </summary>
    /// <remarks>Would be nice if the correct HTTP response code was returned but currently if an error occurs
    /// a 200 is returned and the <see cref="ArcGISError" />property is populated with more details.
    /// Sometimes the code is an internal COM error code too.</remarks>
    [DataContract]
    public class PortalResponse : IPortalResponse
    {
        [DataMember(Name = "error")]
        public ArcGISError Error { get; set; }

        [DataMember(Name = "_links")]
        public List<Link> Links { get; set; }
    }

    [DataContract]
    public class ArcGISError
    {
        [DataMember(Name = "code")]
        public int Code { get; set; }

        [DataMember(Name = "message")]
        public String Message { get; set; }

        [DataMember(Name = "details")]
        public String[] Details { get; set; }

        public override String ToString()
        {
            return String.Format("Code {0}: {1}. {2}", Code, Message, String.Join(" ", Details));
        }
    }

    public class Link
    {
        public Link(String href, String relation = "self")
        {
            Href = href;
            Relation = relation;
            Method = "GET";
        }

        public Link(String href, CommonParameters data, String relation = "self")
        {
            Href = href;
            Relation = relation;
            Method = "POST";
            Data = data;
        }

        [DataMember(Name = "rel")]
        public String Relation { get; private set; }

        [DataMember(Name = "href")]
        public String Href { get; private set; }

        [DataMember(Name = "method")]
        public String Method { get; private set; }

        [DataMember(Name = "data")]
        public CommonParameters Data { get; private set; }
    }
}
