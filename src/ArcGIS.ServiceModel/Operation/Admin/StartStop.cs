using ArcGIS.ServiceModel.Common;
using System.Runtime.Serialization;

namespace ArcGIS.ServiceModel.Operation.Admin
{
    [DataContract]
    public class StartService : ArcGISServerOperation
    {
        public StartService(ServiceDescription serviceDescription)
        {
            Guard.AgainstNullArgument("serviceDescription", serviceDescription);
            Endpoint = new ArcGISServerAdminEndpoint(string.Format(Operations.StartService, serviceDescription.Name, serviceDescription.Type));
        }
    }

    [DataContract]
    public class StopService : ArcGISServerOperation
    {
        public StopService(ServiceDescription serviceDescription)
        {
            Guard.AgainstNullArgument("serviceDescription", serviceDescription);
            Endpoint = new ArcGISServerAdminEndpoint(string.Format(Operations.StopService, serviceDescription.Name, serviceDescription.Type));
        }
    }

    [DataContract]
    public class StartStopServiceResponse : PortalResponse
    {
        [DataMember(Name = "status")]
        public string Status { get; set; }
    }
}
