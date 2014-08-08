//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Windows;

namespace Ptvag.Dawn.Controls.SilverMap.Core.Gadgets
{
    public class MagnifierMap : Map
    {
        public MagnifierMap()
        {
            IsEnabled = false;
        }

        public double ZoomDelta { get; set; }

        public double  WheelSpeed = .5;

        void MagnifierMap_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            double delta = (double)e.Delta * WheelSpeed / 120;
     
            ZoomDelta += delta;

            if (ZoomDelta + parentMap.Zoom > MaxZoom)
                ZoomDelta = (int)MaxZoom - (int)parentMap.Zoom;

            if (parentMap.Zoom + ZoomDelta < MinZoom)
                ZoomDelta = (int)MinZoom - (int)parentMap.Zoom;

            this.UseAnimation = parentMap.UseAnimation;
            ZoomF = parentMap.ZoomF + ZoomDelta;

            e.Handled = true;
        }

        private Map parentMap;
        public Map ParentMap
        {
            set
            {
                if (parentMap != null)
                {
                    MouseWheel -= MagnifierMap_MouseWheel;
                    MouseMove -= parentMap_MouseMove;
                 }

                parentMap = value;

                if (parentMap != null)
                {
                    MouseMove += parentMap_MouseMove;
                    MouseWheel += MagnifierMap_MouseWheel;
                }
            }
            get
            {
                return parentMap;
            }
        }

        public void SetPosition(Point mousePosition, double newZoom, bool animate)
        {
            double dxa = mousePosition.X - Map.ReferenceSize / 2;
            double dya = mousePosition.Y - Map.ReferenceSize / 2;

            double x = dxa / Map.ZoomAdjust * Map.LogicalSize / Map.ReferenceSize;
            double y = -dya / Map.ZoomAdjust * Map.LogicalSize / Map.ReferenceSize;

            SetXYZ(x, y, newZoom, animate);
        }

        void parentMap_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(parentMap.GeoCanvas);

            SetPosition(mousePosition, parentMap.ZoomF + ZoomDelta, parentMap.UseAnimation);
        }
    }
}
