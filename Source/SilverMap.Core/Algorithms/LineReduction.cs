using System;
using System.Collections.Generic;
using System.Windows;

namespace Ptvag.Dawn.Controls.SilverMap.Core.Algorithms
{
    public class LineReduction
    {
        /// <summary>
        /// Clips the given polyline to the given clipping region. Using the specified viewport extents, this
        /// routine also eliminates duplicate points that would otherwise occurr after the viewport transformation.
        /// </summary>
        /// <typeparam name="P">output polyline type</typeparam>
        /// <typeparam name="T">input point type</typeparam>
        /// <param name="sz">viewport extents</param>
        /// <param name="rc">logical clipping rectangle</param>
        /// <param name="polyline">input polyline</param>
        /// <param name="convPnt">function converting an input point of type T to an System.Windows.Point</param>
        /// <param name="addPnt">procedure adding a System.Windows.Point to the resulting polyline of type P</param>
        /// <returns></returns>
        public static ICollection<P> ClipPolylineReducePoints<P, T>(System.Windows.Size sz, System.Windows.Rect rc, ICollection<T> polyline, Func<T, System.Windows.Point> convPnt, Action<P, System.Windows.Point> addPnt) where P : class, new()
        {
            return ClipPolylineReducePoints<P, T>(sz, rc, new ICollection<T>[] { polyline }, convPnt, addPnt);
        }

        /// <summary>
        /// Clips the given polylines to the given clipping region. Using the specified viewport extents, this
        /// routine also eliminates duplicate points that would otherwise occurr after the viewport transformation.
        /// </summary>
        /// <typeparam name="P">output polyline type</typeparam>
        /// <typeparam name="T">input point type</typeparam>
        /// <param name="sz">viewport extents</param>
        /// <param name="rc">logical clipping rectangle</param>
        /// <param name="polylines">input polylines</param>
        /// <param name="convPnt">function converting an input point of type T to System.Windows.Point</param>
        /// <param name="addPnt">procedure adding a System.Windows.Point to the resulting polyline of type P</param>
        /// <returns></returns>
        public static ICollection<P> ClipPolylineReducePoints<P, T>(System.Windows.Size sz, System.Windows.Rect rc, ICollection<ICollection<T>> polylines, Func<T, System.Windows.Point> convPnt, Action<P, System.Windows.Point> addPnt) where P : class, new()
        {
            // re-initialize rc, assuring left <= right and top <= bottom
            rc = new Rect(Math.Min(rc.Left, rc.Right), Math.Min(rc.Top, rc.Bottom), Math.Abs(rc.Width), Math.Abs(rc.Height));

            // create result object storing the clipped lines
            PolylineBuilder<P> polylineBuilder = new PolylineBuilder<P>(addPnt, new Size(rc.Width / sz.Width, rc.Height / sz.Height));

            // loop through given polylines
            if (polylines != null)
            {
                foreach (ICollection<T> polyline in polylines)
                {
                    // enumerator for accessing points
                    IEnumerator<T> e = polyline == null ? null : polyline.GetEnumerator();

                    // fetch first point
                    if (e != null && e.MoveNext())
                    {
                        // initialize starting point
                        System.Windows.Point p0 = convPnt(e.Current);

                        // number of points in current polyline
                        int lastPointIndex = polyline != null ? 0 : polyline.Count - 1, pointIndex = 0;

                        // loop through remaining points
                        while (e.MoveNext())
                        {
                            // update index
                            pointIndex++;

                            // fetch end point. p0 and p1 now mark the start 
                            // and end point of the current line.
                            System.Windows.Point p1 = convPnt(e.Current);

                            // clip the current line. CohenSutherland.clip returns 
                            // true, if any section of the current line is visible.
                            if (CohenSutherland.Clip(rc, ref p0, ref p1))
                            {
                                // append current line. Append also does the magic of point 
                                // reduction and line splitting polylines where necessary.
                                polylineBuilder.append(p0, pointIndex == 1, p1, pointIndex == lastPointIndex);
                            }

                            // current end point is the next starting point
                            p0 = convPnt(e.Current);
                        }
                    }
                }
            }

            // return the polyline
            return polylineBuilder.Polyline;
        }

        /// <summary>
        /// Class for building a polyline out of several single line snippets. 
        /// Internally used by the clipping algorithm, this class does all the work 
        /// of storing and reducing the points passed in from the clipping algorithm.
        /// </summary>
        /// <typeparam name="P">polyline type</typeparam>
        public class PolylineBuilder<P> where P : class, new()
        {
            /// <summary>
            /// the resulting polylines
            /// </summary>
            private List<P> polylineList = new List<P>();

            /// <summary>
            /// threshold used for point reduction
            /// </summary>
            private Size pointReductionThreshold = new Size();

            /// <summary>
            /// threshold used to check if two points differ
            /// </summary>
            private Size differThreshold = new Size(1e-4, 1e-4);

            /// <summary>
            /// the current polyline, initialized when the first line is appended
            /// </summary>
            private P polyline = null;

            /// <summary>
            /// delegate for adding a point to a polyline of type P
            /// </summary>
            private Action<P, System.Windows.Point> addPnt = null;

            /// <summary>
            /// end point of the last line appended
            /// </summary>
            private System.Windows.Point last_p1 = new System.Windows.Point();

            /// <summary>
            /// end point of current polyline
            /// </summary>
            private System.Windows.Point polylineEnd = new System.Windows.Point();

            /// <summary>
            /// c'tor
            /// </summary>
            /// <param name="addPnt">procedure for adding a point to a generic polyline of type P</param>
            /// <param name="pointReductionThreshold">threshold used for point reduction when adding points</param>
            public PolylineBuilder(Action<P, System.Windows.Point> addPnt, Size pointReductionThreshold)
            {
                this.addPnt = addPnt;
                this.pointReductionThreshold = pointReductionThreshold;
            }

            /// <summary>
            /// Appends the line specified by p0 and p1 to the polyline. The given points are added only if 
            /// necessary; that is, if their corresponding pixel coordinates differ from the pixel coordindates 
            /// of the tail of the polyline. Setting either p0First or p1Last to true forces the points to be 
            /// added without further checks.
            /// </summary>
            /// <param name="p0">start point</param>
            /// <param name="force_p0">specifies if p0 is to be added without further checks</param>
            /// <param name="p1">end point</param>
            /// <param name="force_p1">specifies if p1 is to be added without further checks</param>
            public void append(System.Windows.Point p0, bool force_p0, System.Windows.Point p1, bool force_p1)
            {
                // We need to start a new polyline if we have not created one yet or if p0 is not equal to the 
                // end point of the last line added.
                //
                // Otherwise we simply check if we need to insert either p0 or p1. A point will be inserted 
                // if its the first or the last point of an input polyline (force_p0, force_p1 set to accordingly), 
                // or if the point differs from the current end point by pointReductionThreshold.

                if (polyline == null || Math.Abs(last_p1.X - p0.X) >= differThreshold.Width || Math.Abs(last_p1.Y - p0.Y) >= differThreshold.Height)
                {
                    // due to the point reduction it may happen that the last end 
                    // point was not added. Do that now.

                    append(AppendCheck.EvaluateThreshold, differThreshold, last_p1);
                    append(AppendCheck.ForceAppendToNewPolyline, differThreshold, p0);
                }
                else
                {
                    // append p0, if necessary
                    append(force_p0 ? AppendCheck.ForceAppend : AppendCheck.EvaluateThreshold, pointReductionThreshold, p0);
                }

                // append p1, if necessary. Also update end point.
                append(force_p1 ? AppendCheck.ForceAppend : AppendCheck.EvaluateThreshold, pointReductionThreshold, last_p1 = p1);
            }

            /// <summary>
            /// Flags used for checkAppend. 
            /// </summary>
            private enum AppendCheck
            {
                /// <summary>
                /// evaluate a given threshold to decide if a point should be added to the polyline
                /// </summary>
                EvaluateThreshold,
                /// <summary>
                /// append a point without further checks 
                /// </summary>
                ForceAppend,
                /// <summary>
                /// begin a new polyline and append the point without further checks
                /// </summary>
                ForceAppendToNewPolyline
            };

            /// <summary>
            /// Appends the given point to the current polyline, also updating the end point of the current polyline. 
            /// Certain conditions apply before the point is added; see parameter description below.
            /// </summary>
            /// <param name="appendCheck">flag indicating if the given point is to be added without further 
            /// checks or if the given threshold is to be evalutated</param>
            /// <param name="threshold">if appendCheck is set to AppendCheck.EvaluateThreshold, the given point must
            /// differ from the current end point by this threshold before it is added to the polyline.</param>
            /// <param name="p">point to add</param>
            private void append(AppendCheck appendCheck, System.Windows.Size threshold, System.Windows.Point p)
            {
                if (appendCheck == AppendCheck.ForceAppendToNewPolyline)
                    polylineList.Add(polyline = new P());

                if (appendCheck != AppendCheck.EvaluateThreshold || Math.Abs(polylineEnd.X - p.X) >= threshold.Width || Math.Abs(polylineEnd.Y - p.Y) >= threshold.Height)
                    addPnt(polyline, polylineEnd = p);
            }

            /// <summary>
            /// the resulting polylines, after appending 
            /// several single lines using append
            /// </summary>
            public ICollection<P> Polyline
            {
                get
                {
                    return this.polylineList;
                }
            }
        }
    }
}
