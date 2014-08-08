//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Windows.Media;

namespace Ptvag.Dawn.Controls.SilverMap.Core
{
    /// <summary>
    /// Spatial reference systems which can directly used for Siver Map Xaml-elements
    /// Note: To visualize data with another spatial reference system (e.g. WGS84)
    /// you'll have to convert the coordinates to mercator before adding Xaml elements 
    /// to the geo-canvas. You can use GeoTransform.WGSToPtvMercator to convert from WGS.
    /// </summary>
    public enum SpatialReference
    {
        /// <summary>
        /// PTV (Spheric) Mercator
        /// </summary>
        PtvMercator,
        /// <summary>
        /// PTV Mercator with interted Y-axis
        /// Corresponds to PTV mercator, but has the same orientation than screen coordinates. 
        /// </summary>
        PtvMercatorInvertedY,
        /// <summary>
        /// PTV Smart Units
        /// </summary>
        PtvSmartUnits,
        /// <summary>
        /// Ptv Smart Units with inverted Y-axis
        /// </summary>
        PtvSmartUnitsInvertedY,
        /// <summary>
        /// Google/MS Spheric Mercator (SRID 3785/900913)
        /// </summary>
        WebMercator,
        /// <summary>
        /// Google/MS Spheric Mercator with inverted Y-axis
        /// </summary>
        WebMercatorInvertedY,
    }

    /// <summary>
    /// The TransformFactory creates Wpf transformations which can be used 
    /// as RenderTransform for child canvases of the GeoCanvas object
    /// </summary>
    public class TransformFactory
    {
        /// <summary>
        /// Returns a Transform object for a spatial reference
        /// Which can be used as RenderTransform for a child of the GeoCanvas
        /// </summary>
        /// <param name="reference">The spatial reference</param>
        /// <returns>The resulting render transform</returns>
        public static Transform CreateTransform(SpatialReference reference)
        {
            switch (reference)
            {
                case SpatialReference.PtvMercator:
                    {
                        const double EARTH_RADIUS = 6371000.0;
                        const double mercatorSize = EARTH_RADIUS * 2.0 * Math.PI;

                        var translateTransform = new TranslateTransform { X = Map.ReferenceSize / 2, Y = Map.ReferenceSize / 2 };
                        var zoomTransform = new ScaleTransform { ScaleX = Map.ZoomAdjust * Map.ReferenceSize / mercatorSize, ScaleY = -Map.ZoomAdjust * Map.ReferenceSize / mercatorSize, CenterX = Map.ReferenceSize / 2, CenterY = Map.ReferenceSize / 2 };
                        var transformGroup = new TransformGroup();
                        transformGroup.Children.Add(translateTransform);
                        transformGroup.Children.Add(zoomTransform);
                        return transformGroup;
                    }
                case SpatialReference.PtvMercatorInvertedY:
                    {
                        const double EARTH_RADIUS = 6371000.0;
                        const double mercatorSize = EARTH_RADIUS * 2.0 * Math.PI;

                        var translateTransform = new TranslateTransform { X = Map.ReferenceSize / 2, Y = Map.ReferenceSize / 2 };
                        var zoomTransform = new ScaleTransform { ScaleX = Map.ZoomAdjust * Map.ReferenceSize / mercatorSize, ScaleY = Map.ZoomAdjust * Map.ReferenceSize / mercatorSize, CenterX = Map.ReferenceSize / 2, CenterY = Map.ReferenceSize / 2 };
                        var transformGroup = new TransformGroup();
                        transformGroup.Children.Add(translateTransform);
                        transformGroup.Children.Add(zoomTransform);
                        return transformGroup;
                    }
                case SpatialReference.PtvSmartUnits:
                    {
                        const double EARTH_RADIUS = 6371000.0;
                        const double mercatorSize = EARTH_RADIUS * 2.0 * Math.PI * 0.207919962457972;

                        var translateTransform = new TranslateTransform { X = Map.ReferenceSize / 2 - mercatorSize / 2, Y = Map.ReferenceSize / 2 - mercatorSize / 2 };
                        var zoomTransform = new ScaleTransform { ScaleX = Map.ZoomAdjust * Map.ReferenceSize / mercatorSize, ScaleY = -Map.ZoomAdjust * Map.ReferenceSize / mercatorSize, CenterX = Map.ReferenceSize / 2, CenterY = Map.ReferenceSize / 2 };
                        var transformGroup = new TransformGroup();
                        transformGroup.Children.Add(translateTransform);
                        transformGroup.Children.Add(zoomTransform);
                        return transformGroup;
                    }
                case SpatialReference.PtvSmartUnitsInvertedY:
                    {
                        const double EARTH_RADIUS = 6371000.0;
                        const double mercatorSize = EARTH_RADIUS * 2.0 * Math.PI * 0.207919962457972;

                        var translateTransform = new TranslateTransform { X = Map.ReferenceSize / 2 - mercatorSize / 2, Y = Map.ReferenceSize / 2 + mercatorSize / 2 };
                        var zoomTransform = new ScaleTransform { ScaleX = Map.ZoomAdjust * Map.ReferenceSize / mercatorSize, ScaleY = Map.ZoomAdjust * Map.ReferenceSize / mercatorSize, CenterX = Map.ReferenceSize / 2, CenterY = Map.ReferenceSize / 2 };
                        var transformGroup = new TransformGroup();
                        transformGroup.Children.Add(translateTransform);
                        transformGroup.Children.Add(zoomTransform);
                        return transformGroup;
                    }
                case SpatialReference.WebMercator:
                    {
                        const double EARTH_RADIUS = 6378137.0;
                        const double mercatorSize = EARTH_RADIUS * 2.0 * Math.PI;

                        var translateTransform = new TranslateTransform { X = Map.ReferenceSize / 2, Y = Map.ReferenceSize / 2 };
                        var zoomTransform = new ScaleTransform { ScaleX = Map.ZoomAdjust * Map.ReferenceSize / mercatorSize, ScaleY = -Map.ZoomAdjust * Map.ReferenceSize / mercatorSize, CenterX = Map.ReferenceSize / 2, CenterY = Map.ReferenceSize / 2 };
                        var transformGroup = new TransformGroup();
                        transformGroup.Children.Add(translateTransform);
                        transformGroup.Children.Add(zoomTransform);
                        return transformGroup;
                    }
                case SpatialReference.WebMercatorInvertedY:
                    {
                        const double EARTH_RADIUS = 6378137.0;
                        const double mercatorSize = EARTH_RADIUS * 2.0 * Math.PI;

                        var translateTransform = new TranslateTransform { X = Map.ReferenceSize / 2, Y = Map.ReferenceSize / 2 };
                        var zoomTransform = new ScaleTransform { ScaleX = Map.ZoomAdjust * Map.ReferenceSize / mercatorSize, ScaleY = Map.ZoomAdjust * Map.ReferenceSize / mercatorSize, CenterX = Map.ReferenceSize / 2, CenterY = Map.ReferenceSize / 2 };
                        var transformGroup = new TransformGroup();
                        transformGroup.Children.Add(translateTransform);
                        transformGroup.Children.Add(zoomTransform);
                        return transformGroup;
                    }
                default:
                    throw new ArgumentException("not supported");
            }
        }
    }
}
