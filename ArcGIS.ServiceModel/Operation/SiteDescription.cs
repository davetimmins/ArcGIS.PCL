using ArcGIS.ServiceModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ArcGIS.ServiceModel.Operation
{
    public class SiteDescription
    {
        public SiteDescription()
        {
            Resources = new List<ArcGISServerEndpoint>();
        }

        public double Version { get; set; }

        public List<ArcGISServerEndpoint> Resources { get; set; }
    }

    [DataContract]
    public class SiteFolderDescription : PortalResponse
    {
        [IgnoreDataMember]
        public String Path { get; set; }

        [DataMember(Name = "currentVersion")]
        public double Version { get; set; }

        [DataMember(Name = "folders")]
        public String[] Folders { get; set; }

        [DataMember(Name = "services")]
        public ServiceDescription[] Services { get; set; }
    }

    [DataContract]
    public class ServiceDescription
    {
        [DataMember(Name = "name")]
        public String Name { get; set; }

        [DataMember(Name = "type")]
        public String Type { get; set; }
    }
}
