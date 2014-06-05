using ArcGIS.ServiceModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ArcGIS.ServiceModel.Operation
{
    /// <summary>
    /// Represents an ArcGIS Server site
    /// </summary>
    public class SiteDescription
    {
        public SiteDescription()
        {
            Resources = new List<SiteFolderDescription>();
        }

        /// <summary>
        /// Current version of ArcGIS Server
        /// </summary>
        public double Version { get { return Resources.Max(r => r.Version); } }

        /// <summary>
        /// Collection of discovered REST resources
        /// </summary>
        public List<SiteFolderDescription> Resources { get; set; }

        /// <summary>
        /// Collection of discovered REST resources as ArcGIS Server endpoints
        /// </summary>
        public IEnumerable<ServiceDescription> Services
        {
            get
            {
                foreach (var description in Resources)
                {
                    foreach (var service in description.Services)
                    {
                        yield return service;
                    }
                }
            }
        }

        /// <summary>
        /// Collection of discovered REST resources as ArcGIS Server endpoints
        /// </summary>
        public IEnumerable<ArcGISServerEndpoint> ArcGISServerEndpoints
        {
            get
            {
                foreach (var service in Services)
                {
                    yield return new ArcGISServerEndpoint(String.Format("{0}/{1}", service.Name, service.Type));
                }
                
            }
        }
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

        [IgnoreDataMember]
        public ArcGISServerEndpoint ArcGISServerEndpoint
        {
            get { return new ArcGISServerEndpoint(String.Format("{0}/{1}", Name, Type)); }
        }
    }
}
