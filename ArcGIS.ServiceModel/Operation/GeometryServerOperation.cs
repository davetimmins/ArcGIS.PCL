using ArcGIS.ServiceModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ArcGIS.ServiceModel.Operation
{
    [DataContract]
    public class SimplifyGeometry<T> : ArcGISServerOperation where T : IGeometry
    {
        public SimplifyGeometry(ArcGISServerEndpoint endpoint)
            : base(endpoint, Operations.Simplify)
        {
            Geometries = new GeometryCollection<T>();
        }

        [DataMember(Name = "geometries")]
        public GeometryCollection<T> Geometries { get; set; }

        [DataMember(Name = "sr")]
        public SpatialReference SpatialReference { get; set; }
    }

    [DataContract]
    public class GeometryOperationResponse<T> : PortalResponse where T : IGeometry
    {
        [DataMember(Name = "geometries")]
        public List<T> Geometries { get; set; }
    }

    [DataContract]
    public class ProjectGeometry<T> : ArcGISServerOperation where T : IGeometry
    {
        public ProjectGeometry(ArcGISServerEndpoint endpoint, List<Feature<T>> features, SpatialReference outputSpatialReference)
            : base(endpoint, Operations.Project)
        {
            if (features.Any())
            {
                Geometries = new GeometryCollection<T>();
                Geometries.Geometries = features.Select(f => f.Geometry).ToList();
                Geometries.Geometries.First().SpatialReference = outputSpatialReference;
            }
            OutputSpatialReference = outputSpatialReference;
        }

        [DataMember(Name = "geometries")]
        public GeometryCollection<T> Geometries { get; private set; }

        [DataMember(Name = "inSR")]
        public SpatialReference InputSpatialReference { get { return Geometries.Geometries.First().SpatialReference ?? SpatialReference.WGS84; } }

        /// <summary>
        /// The spatial reference of the returned geometry. 
        /// If not specified, the geometry is returned in the spatial reference of the input.
        /// </summary>
        [DataMember(Name = "outSR")]
        public SpatialReference OutputSpatialReference { get; private set; }
    }

    [DataContract]
    public class GeometryCollection<T> where T : IGeometry
    {
        [DataMember(Name = "geometryType")]
        public String GeometryType
        {
            get
            {
                return Geometries == null
                    ? String.Empty
                    : GeometryTypes.TypeMap[Geometries.First().GetType()]();
            }
        }

        [DataMember(Name = "geometries")]
        public List<T> Geometries { get; set; }
    }
}
