//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ptvag.Dawn.Controls.SilverMap.Core;
using SilverMap.XrouteService;

namespace SilverMap.UseCases.Routing
{
    public partial class RoutingControl : UserControl
    {
        StackPanel workspace;
        Map map;
        RoutePolygonLayer routingLayer;
        RouteStationsLayer stationsLayer;
        ObservableCollection<PlainPoint> stations = new ObservableCollection<PlainPoint>();
        PlainPoint tmpPoint = null;

        public RoutingControl(StackPanel workspace, Map map)
        {
            InitializeComponent();

            this.workspace = workspace;
            this.map = map;

            map.MouseRightButtonDown +=new MouseButtonEventHandler(map_MouseRightButtonDown);
            ContextMenuService.SetContextMenu(map, cm);

            stationsLayer = new RouteStationsLayer(this.map) { RemoveStation = this.RemoveStation, Points = stations };

            // insert stations Karlsruhe-Berlin
            stations.Add(new PlainPoint { x = 934448.8, y = 6269219.7 });
            stations.Add(new PlainPoint { x = 1491097.3, y = 6888163.5 });

            CalculateRoute();
        }

        public void CalculateRoute()
        {
            // reset context menu
            ContextMenuService.SetContextMenu(map, null);

            // build xserver wayPoints array from stations
            var wayPoints = stations.Select(p => new WaypointDesc
            {
                wrappedCoords = new XrouteService.Point[] {new XrouteService.Point {point = p}}
            }).ToArray();

            // to call an xServer directly, you have to put a cross-domain policy-file on your xServer machine
            // see http://msdn.microsoft.com/en-us/library/cc197955(VS.95).aspx 
        //    XRouteWS xRoute = new XRouteWSClient();
            XRouteWS xRoute = new XRouteWSClient(new BasicHttpBinding { MaxReceivedMessageSize = 2147483647 },
                new EndpointAddress(App.BaseUrl + "/XServerProxy.ashx?type=xroute"));

                xRoute.BegincalculateRoute(new calculateRouteRequest
                {
                    ArrayOfWaypointDesc_1 = wayPoints,
                    ResultListOptions_4 = new ResultListOptions {polygon = true},
                    //CallerContext_5 = new CallerContext // also an option: use wkb encoding and parse with NetTopologySuite
                    //{
                    //    wrappedProperties = new CallerContextProperty[]{
                    //    new CallerContextProperty{key = "ResponseGeometry", value = GeometryEncoding.WKB.ToString()}}
                    //}
                }, new AsyncCallback(Invoke), xRoute);
        }

        void map_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point p = map.CanvasToPtvMercator(map, e.GetPosition(map));
            tmpPoint = new PlainPoint { x = p.X, y = p.Y };

            e.Handled = false;
        }   

        public void Invoke(IAsyncResult result)
        { 
            try
            {
                // not the UI thread!
                calculateRouteResponse response = (result.AsyncState as XRouteWS).EndcalculateRoute(result);

                Dispatcher.BeginInvoke(new Action<Route>(InitializeUI), response.result);
            }
            catch (Exception ex)
            {    
                // Note: you must register the Server for ClientHttp-Protocol to get the real exception, see
                // http://blogs.microsoft.co.il/blogs/idof/archive/2009/12/08/handling-soap-faults-in-silverlight.aspx
                Dispatcher.BeginInvoke(new Action<string>(DisplayError), ex.Message);
            }
        }

        public void DisplayError(string errorMessage)
        {
            ContextMenuService.SetContextMenu(map, cm);

            MessageBox.Show(errorMessage);
        }

        bool disposed;
        private void InitializeUI(Route route)
        {
            ContextMenuService.SetContextMenu(map, cm);

            // already disposed
            if (disposed)
                return;

            if (routingLayer != null)
                routingLayer.Remove();

            routingLayer = new RoutePolygonLayer(this.map, route);
        }

        public void Remove()
        {
            workspace.Children.Remove(this);

            if (routingLayer != null)
                routingLayer.Remove();

            if (stationsLayer != null)
                stationsLayer.Remove();

            map.MouseRightButtonDown -= new MouseButtonEventHandler(map_MouseRightButtonDown);
            ContextMenuService.SetContextMenu(map, null);

            disposed = true;
        }

        private void SetStart_Click(object sender, RoutedEventArgs e)
        {
            stations.Insert(0, tmpPoint);
        }

        private void SetEnd_Click(object sender, RoutedEventArgs e)
        {
            stations.Add(tmpPoint);
        }

        private void CalcRoute_Click(object sender, RoutedEventArgs e)
        {
            CalculateRoute();
        }

        public void RemoveStation(PlainPoint p)
        {
            stations.Remove(p);
        }
    }
}
