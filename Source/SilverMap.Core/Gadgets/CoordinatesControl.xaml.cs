//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Input;

namespace Ptvag.Dawn.Controls.SilverMap.Core.Gadgets
{
    public partial class CoordinatesControl : MapGadget
    {
        public CoordinatesControl()
        {
            InitializeComponent();
        }

        bool isActive = false;
        protected override void Initialize()
        {
            this.Visibility = Visibility.Collapsed;

            Map.MouseMove += new MouseEventHandler(Map_MouseMove);
            Map.MouseLeave += new MouseEventHandler(Map_MouseLeave);
            Map.MouseEnter += new MouseEventHandler(Map_MouseEnter);
            Map.ViewportWhileChanged += new EventHandler(Map_ViewportWhileChanged);
        }

        void Map_MouseEnter(object sender, MouseEventArgs e)
        {
            isActive = true;

            pixelPoint = e.GetPosition(Map);

            UpdateText();
        }

        void Map_MouseLeave(object sender, MouseEventArgs e)
        {
            isActive = false;

            this.Visibility = Visibility.Collapsed;
        }

        void Map_ViewportWhileChanged(object sender, EventArgs e)
        {
            UpdateText();
        }

        Point pixelPoint;
        void Map_MouseMove(object sender, MouseEventArgs e)
        {
            pixelPoint = e.GetPosition(Map);

            UpdateText();
        }

        void UpdateText()
        {
            if(!isActive)
                return;

            Point wgsPoint = Map.CanvasToWgs(Map, pixelPoint);

            if (wgsPoint.Y < -90 || wgsPoint.Y > 90 || wgsPoint.X < -180 || wgsPoint.X > 180)
                this.Visibility = Visibility.Collapsed;
            else
            {
                this.Visibility = Visibility.Visible;
                CopyrightText.Text = GeoTransform.LatLonToString(wgsPoint.Y, wgsPoint.X);
            }
        }
    }
}
