using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ArcGIS.ServiceModel.Common
{
    /// <summary>
    /// JSON feature object as returned by the REST API. Typically used to represent a client side graphic
    /// </summary>
    /// <typeparam name="T">Type of geometry that the feature represents.</typeparam>
    /// <remarks>All properties are optional.</remarks>
    public class Feature<T> : IEquatable<Feature<T>> where T : IGeometry
    {
        public Feature()
        {
            Attributes = new Dictionary<String, object>();
        }

        [DataMember(Name = "geometry")]
        public T Geometry { get; set; }

        /// <summary>
        /// A JSON object that contains a dictionary of name-value pairs. 
        /// The names are the feature field names. 
        /// The values are the field values and they can be any of the standard JSON types - string, number and boolean.
        /// </summary>
        /// <remarks>Date values are encoded as numbers. The number represents the number of milliseconds since epoch (January 1, 1970) in UTC.</remarks>
        [DataMember(Name = "attributes")]
        public Dictionary<String, object> Attributes { get; set; }

        public bool Equals(Feature<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<T>.Default.Equals(Geometry, other.Geometry) 
                && (Attributes.Count == other.Attributes.Count) && !(Attributes.Except(other.Attributes)).Any();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(Geometry) * 397) ^ (Attributes != null ? Attributes.GetHashCode() : 0);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as Feature<T>;
            return other != null && Equals(other);
        }
    }   
}
