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
    public class BufferGeometry<T> : ArcGISServerOperation where T : IGeometry
    {
        public BufferGeometry(ArcGISServerEndpoint endpoint, List<Feature<T>> features, SpatialReference inputSpatialReference, double distance)
            : base(endpoint, Operations.Buffer)
        {
            if (features.Any())
            {
                Geometries = new GeometryCollection<T>();
                Geometries.Geometries = features.Select(f => f.Geometry).ToList();
            }
            InputSpatialReference = inputSpatialReference;
            OutputSpatialReference = inputSpatialReference;
            BufferSpatialReference = inputSpatialReference;
            Distances = new List<double>{distance};
        }
        
        [DataMember(Name = "geometries")]
        public GeometryCollection<T> Geometries { get; set; }

        [DataMember(Name = "inSR")]
        public SpatialReference InputSpatialReference { get; set; }

        [DataMember(Name = "outSR")]
        public SpatialReference OutputSpatialReference { get; set; }

        [DataMember(Name = "bufferSR")]
        public SpatialReference BufferSpatialReference { get; set; }

        public List<double> Distances { get; set; }

        [DataMember(Name = "distances")]
        public string DistancesCSV
        {
            get
            {
                string strDistances = "";
                foreach (double distance in Distances)
                {
                    strDistances += distance.ToString("0.000") + ", ";
                }

                if (strDistances.Length >= 2)
                {
                    strDistances = strDistances.Substring(0, strDistances.Length - 2);
                }

                return strDistances;
            }
        }

        /// <summary>
        /// See http://resources.esri.com/help/9.3/ArcGISDesktop/ArcObjects/esriGeometry/esriSRUnitType.htm and http://resources.esri.com/help/9.3/ArcGISDesktop/ArcObjects/esriGeometry/esriSRUnit2Type.htm
        /// If not specified, derived from bufferSR, or inSR.
        /// </summary>
        [DataMember(Name = "unit")]
        public string Unit { get; set; }

        [DataMember(Name = "unionResults")]
        public bool UnionResults { get; set; }
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
