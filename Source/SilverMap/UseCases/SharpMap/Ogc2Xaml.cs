//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Windows;
using System.Windows.Media;
using GeoAPI.Geometries;
using System.Collections.Generic;

namespace SilverMap.UseCases.SharpMap
{
    /// <summary>
    /// This helper class creates a Xaml Geometry from an OGC polygon
    /// </summary>
    public class Ogc2Xaml
    {
        public static Geometry ConvertPolygon(IGeometry geometry)
        {
            var figures = new PathFigureCollection();

            if (geometry is IPolygon)
            {
                foreach (var figure in GetPathFigureCollection(geometry as IPolygon))
                    figures.Add(figure);
            }
            else if (geometry is IMultiPolygon)
            {
                var multiPoly = geometry as IMultiPolygon;

                foreach (IPolygon ogcPoly in multiPoly.Geometries)
                {
                    foreach (var figure in GetPathFigureCollection(ogcPoly))
                        figures.Add(figure);
                }
            }

            return new PathGeometry { Figures = figures };
        }

        public static IEnumerable<PathFigure> GetPathFigureCollection(IPolygon ogcPoly)
        {
            yield return BuildPathFigure(ogcPoly.ExteriorRing.Coordinates);

            foreach (GeoAPI.Geometries.ILineString hole in ogcPoly.InteriorRings)
            {
                yield return BuildPathFigure(hole.Coordinates);
            }
        }

        public static PathFigure BuildPathFigure(ICoordinate[] coordinates)
        {
            return new PathFigure
            {
               IsFilled = true,
               IsClosed = true,
               StartPoint = new Point {X = coordinates[0].X, Y = coordinates[0].Y},
               Segments = new PathSegmentCollection { new PolyLineSegment { Points = ToSegments(coordinates) } }
            };
        }

        public static PointCollection ToSegments(ICoordinate[] points)
        {
            var result = new PointCollection();

            foreach (ICoordinate point in points)
                result.Add(new Point { X = point.X, Y = point.Y });

            return result;
        }
    }
}
