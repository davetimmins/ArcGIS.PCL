using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel.GeoJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGIS.ServiceModel.Extensions
{
    public static class FeatureCollectionExtensions
    {
        static Dictionary<String, Func<Type>> _typeMap = new Dictionary<String, Func<Type>>
            {
                { "Point", () => typeof(Point) },
                { "MultiPoint", () => typeof(MultiPoint) },
                { "LineString", () => typeof(Polyline) },
                { "MultiLineString", () => typeof(Polyline) },
                { "Polygon", () => typeof(Polygon) },
                { "MultiPolygon", () => typeof(Polygon) } 
            };

        public static List<Feature<IGeometry>> ToFeatures<TGeometry>(this FeatureCollection<TGeometry> featureCollection)
            where TGeometry : IGeoJsonGeometry
        {
            if (featureCollection == null || featureCollection.Features == null || !featureCollection.Features.Any()) return null;

            var features = new List<Feature<IGeometry>>();

            foreach (var geoJson in featureCollection.Features)
            {
                var geometry = geoJson.Geometry.ToGeometry(_typeMap[geoJson.Geometry.Type]());
                if (geometry == null) continue;

                features.Add(new Feature<IGeometry> { Geometry = geometry, Attributes = geoJson.Properties });
            }
            return features;
        }

        public static FeatureCollection<IGeoJsonGeometry> ToFeatureCollection<TGeometry>(this List<Feature<TGeometry>> features)
            where TGeometry : IGeometry
        {
            if (features == null || !features.Any()) return null;

            var featureCollection = new FeatureCollection<IGeoJsonGeometry> { Features = new List<GeoJsonFeature<IGeoJsonGeometry>>() };
            if (features.First().Geometry.SpatialReference != null)
                featureCollection.CoordinateReferenceSystem = new Crs
                {
                    Type = "EPSG",
                    Properties = new CrsProperties { Wkid = features.First().Geometry.SpatialReference.Wkid }
                };

            foreach (var feature in features)
            {
                var geoJsonGeometry = feature.Geometry.ToGeoJson();
                if (geoJsonGeometry == null) continue;
                featureCollection.Features.Add(new GeoJsonFeature<IGeoJsonGeometry>
                {
                    Type = "Feature",
                    Geometry = geoJsonGeometry,
                    Properties = feature.Attributes
                });
            }
            return featureCollection;
        }
    }
}
