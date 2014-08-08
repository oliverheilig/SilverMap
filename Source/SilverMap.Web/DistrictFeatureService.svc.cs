//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using SharpMap.Data;
using Ptvag.Dawn.Controls.Map.MapMarket;

namespace Ptvag.Dawn.SilverMap.Web
{
    public class DistrictFeatureService : IDistrictFeatureService
    {
        /// <summary>
        /// This function returns the feature data for a polygon which contains a specific point.
        /// map&market regions are a coverage (i.e. non-overlapping), so the result is 0..1
        /// Since we have no spatial data base, we need a two-stage process:
        /// First query all polygons whose envelope contains the point with the map&market sharpmap provider,
        /// and then find the polygon which exactly contains the point using NTS
        /// </summary>
        /// <param name="layerName">the name of the layer</param>
        /// <param name="x">x-coordinate in PTV_Mercator</param>
        /// <param name="y">y-coordinate in PTV_Mercator</param>
        /// <returns>The feature data as mobile object</returns>
        public IEnumerable<DistrictFeature> GetDistrictFeaturePoint(string layerName, double x, double y)
        {
            // the point as NTS geometry, we need it later
            GeoAPI.Geometries.IPoint ntsPoint = new GisSharpBlog.NetTopologySuite.Geometries.Point(x, y);

            // use the map&market sharmap provider to execute the query
            MMProvider mmp = new MMProvider(
                @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DATADIRECTORY|\Districts.mdb",
                "KRE", "GID", "XMIN", "YMIN", "XMAX", "YMAX", "WKB_GEOMETRY");
            using (FeatureDataSet dataSet = new FeatureDataSet())
            {
                // the query returns all elements whose bounding box intersect the Rect(x,y,x,y)
                mmp.ExecuteIntersectionQuery(new SharpMap.Geometries.BoundingBox(x, y, x, y), dataSet);

                // Find the polygon which contains the point.
                foreach (FeatureDataRow row in dataSet.Tables[0])
                {
                    // We have to use nts here - sharpmap doesn't implement the OGC-functions properly.
                    // Convert to NTS geometry via WKB
                    GeoAPI.Geometries.IGeometry ntsPolygon = new GisSharpBlog.NetTopologySuite.IO.WKBReader().Read(row.Geometry.AsBinary());

                    if (ntsPolygon.Contains(ntsPoint))
                        yield return new DistrictFeature { Id = row["GID"].ToString(), Name = row["NAME"].ToString(), GeometryWkb = row.Geometry.AsBinary() };
                }
            }
        }

        public IEnumerable<DistrictFeature> GetDistrictFeaturePolygon(string layerName, List<PolyPoint> polygon)
        {
            // must be a valid polygon
            if (polygon.Count <= 3)
                yield break;

            // the polygon as NTS geometry, we need it later
            var selectPoly = new GisSharpBlog.NetTopologySuite.Geometries.Polygon(new GisSharpBlog.NetTopologySuite.Geometries.LinearRing(
                 (from p in polygon select new GisSharpBlog.NetTopologySuite.Geometries.Coordinate(p.X, p.Y)).ToArray()));

            // the bounding box of the polygon
            var bounds = (from point in polygon
                          select new SharpMap.Geometries.BoundingBox(polygon.Min(p => p.X), polygon.Min(p => p.Y),
                              polygon.Max(p => p.X), polygon.Max(p => p.Y))).FirstOrDefault();

            // use the map&market sharmap provider to execute the query
            MMProvider mmp = new MMProvider(
                 @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DATADIRECTORY|\Districts.mdb",
                 "KRE", "GID", "XMIN", "YMIN", "XMAX", "YMAX", "WKB_GEOMETRY");
            using (FeatureDataSet dataSet = new FeatureDataSet())
            {
                // the query returns all elements whose bounding box intersect bounds
                mmp.ExecuteIntersectionQuery(bounds, dataSet);

                // Find the polygons which intersect the query polygon
                foreach (FeatureDataRow row in dataSet.Tables[0])
                {
                    // We have to use nts here - sharpmap doesn't implement the OGC-functions properly.
                    // Convert to NTS geometry via WKB
                    GeoAPI.Geometries.IGeometry ntsPolygon = new GisSharpBlog.NetTopologySuite.IO.WKBReader().Read(row.Geometry.AsBinary());

                    if (selectPoly.Intersects(ntsPolygon))
                        yield return new DistrictFeature
                        {
                            Id = row["GID"].ToString(),
                            Name = row["NAME"].ToString(),
                            GeometryWkb = row.Geometry.AsBinary()
                        };
                }
            }
        }
    }
}
