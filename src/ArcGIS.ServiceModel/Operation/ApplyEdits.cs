using System.Collections.Generic;
using System.Runtime.Serialization;
using ArcGIS.ServiceModel.Common;

namespace ArcGIS.ServiceModel.Operation
{
    /// <summary>
    /// This operation adds, updates and deletes features to the associated feature layer or table in a single call (POST only). 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public class ApplyEdits<T> : ArcGISServerOperation where T : IGeometry
    {
        public ApplyEdits(ArcGISServerEndpoint endpoint)
        {
            Guard.AgainstNullArgument("endpoint", endpoint);
            Endpoint = new ArcGISServerEndpoint(endpoint.RelativeUrl.Trim('/') + "/" + Operations.ApplyEdits);
        }

        /// <summary>
        /// The array of features to be added.
        /// </summary>
        [DataMember(Name = "adds")]
        public List<Feature<T>> Adds { get; set; }

        /// <summary>
        /// The array of features to be updated.
        /// </summary>
        [DataMember(Name = "updates")]
        public List<Feature<T>> Updates { get; set; }

        /// <summary>
        ///  The object IDs of this layer / table to be deleted.
        /// </summary>
        [IgnoreDataMember]
        public List<long> Deletes { get; set; }

        [DataMember(Name = "deletes")]
        public string DeleteIds { get { return Deletes == null ? string.Empty : string.Join(",", Deletes); } }
    }

    /// <summary>
    ///  Results of the apply edits. The results are grouped by type of edit.
    /// </summary>
    [DataContract]
    public class ApplyEditsResponse : PortalResponse
    {
        [DataMember(Name = "addResults")]
        public List<ApplyEditResponse> Adds { get; set; }

        [DataMember(Name = "updateResults")]
        public List<ApplyEditResponse> Updates { get; set; }

        [DataMember(Name = "deleteResults")]
        public List<ApplyEditResponse> Deletes { get; set; }
    }

    /// <summary>
    /// Identifies a single feature and indicates if the edit was successful or not.
    /// </summary>
    [DataContract]
    public class ApplyEditResponse : PortalResponse
    {
        [DataMember(Name = "objectId")]
        public long ObjectId { get; set; }

        [DataMember(Name = "globalId")]
        public string GlobalId { get; set; }

        [DataMember(Name = "success")]
        public bool Success { get; set; }
    }
}
