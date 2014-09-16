using ArcGIS.ServiceModel.Common;
using System;
using System.Runtime.Serialization;

namespace ArcGIS.ServiceModel.Operation.Admin
{
    [DataContract]
    public class StartService : ArcGISServerOperation
    {
        public StartService(ServiceDescription serviceDescription)
        {
            Endpoint = new ArcGISServerAdminEndpoint(String.Format(Operations.StartService, serviceDescription.Name, serviceDescription.Type));
        }
    }

    [DataContract]
    public class StopService : ArcGISServerOperation
    {
        public StopService(ServiceDescription serviceDescription)
        {
            Endpoint = new ArcGISServerAdminEndpoint(String.Format(Operations.StopService, serviceDescription.Name, serviceDescription.Type));
        }
    }

    [DataContract]
    public class StartStopServiceResponse : PortalResponse
    {
        [DataMember(Name="status")]
        public String Status { get; set; }
    }
}
