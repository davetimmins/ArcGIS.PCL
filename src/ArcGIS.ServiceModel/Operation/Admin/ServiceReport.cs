using ArcGIS.ServiceModel.Common;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ArcGIS.ServiceModel.Operation.Admin
{
    public class SiteReportResponse
    {
        public SiteReportResponse()
        {
            Resources = new List<FolderReportResponse>();
        }

        /// <summary>
        /// Collection of discovered REST resources
        /// </summary>
        public List<FolderReportResponse> Resources { get; set; }

        public IEnumerable<ServiceReportResponse> ServiceReports
        {
            get
            {
                foreach (var folder in Resources)
                {
                    foreach (var report in folder.Reports)
                    {
                        yield return report;
                    }
                }
            }
        }
    }

    public class ServiceReport : ArcGISServerOperation
    {
        public ServiceReport(string path)
        {
            Endpoint = new ArcGISServerAdminEndpoint(string.Format(Operations.ServiceReport, path.Replace("/", "")).Replace("//", "/"));
        }
    }

    [DataContract]
    public class FolderReportResponse : PortalResponse
    {
        [DataMember(Name = "reports")]
        public ServiceReportResponse[] Reports { get; set; }
    }

    [DataContract]
    public class ServiceReportResponse
    {
        [DataMember(Name = "folderName")]
        public string FolderName { get; set; }

        [DataMember(Name = "serviceName")]
        public string ServiceName { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "status")]
        public ServiceStatusResponse Status { get; set; }

        public ServiceDescription AsServiceDescription()
        {
            return new ServiceDescription
            {
                Name = (FolderName + "/" + ServiceName).Trim('/'),
                Type = Type
            };
        }
    }
}
