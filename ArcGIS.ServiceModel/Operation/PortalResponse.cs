using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ArcGIS.ServiceModel.Operation
{
    /// <summary>
    /// Common response object from an ArcGIS Server REST call
    /// </summary>
    /// <remarks>Would be nice if the correct HTTP response code was returned but currently if an error occurs
    /// a 200 is returned and the <see cref="ArcGISError" />property is populated with more details.
    /// Sometimes the code is an internal COM error code too.</remarks>
    [DataContract]
    public class PortalResponse
    {
        [DataMember(Name = "error")]
        public ArcGISError Error { get; set; }
    }
    
    [DataContract]
    public class ArcGISError
    {
        [DataMember(Name = "code")]
        public int Code { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "details")]
        public string[] Details { get; set; }

        public override string ToString()
        {
            return String.Format("Code {0}: {1}. {2}", Code, Message, String.Join(" ", Details));
        }
    }   
}
