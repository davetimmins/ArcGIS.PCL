using ArcGIS.ServiceModel.Common;
using System;
using System.Runtime.Serialization;

namespace ArcGIS.ServiceModel.Operation.Admin
{
    [DataContract]
    public class ServiceStatus : ArcGISServerOperation
    {
        public ServiceStatus(ServiceDescription serviceDescription)
        {
            Endpoint = new ArcGISServerAdminEndpoint(String.Format(Operations.ServiceStatus, serviceDescription.Name, serviceDescription.Type));
        }
    }

    [DataContract]
    public class ServiceStatusResponse : PortalResponse
    {
        [DataMember(Name = "configuredState")]
        public String Expected { get; set; }

        [DataMember(Name = "realTimeState")]
        public String Actual { get; set; }
    }
}
