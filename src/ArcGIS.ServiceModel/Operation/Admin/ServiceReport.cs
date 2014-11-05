using ArcGIS.ServiceModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

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
        public ServiceReport(String path)
        {
            Endpoint = new ArcGISServerAdminEndpoint(String.Format(Operations.ServiceReport, path.Replace("/", "")).Replace("//", "/"));
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
        public String FolderName { get; set; }

        [DataMember(Name = "serviceName")]
        public String ServiceName { get; set; }

        [DataMember(Name = "type")]
        public String Type { get; set; }

        [DataMember(Name = "description")]
        public String Description { get; set; }

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
