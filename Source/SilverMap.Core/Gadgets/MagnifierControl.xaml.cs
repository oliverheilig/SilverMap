//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ptvag.Dawn.Controls.SilverMap.Core.Gadgets
{
    /// <summary>
    /// Interaction logic for MagnifierControl.xaml
    /// </summary>
    public partial class MagnifierControl : MapGadget
    {
        public MagnifierControl()
        {
            InitializeComponent();
        }

        protected override void Initialize()
        {
            Map.KeyDown += new KeyEventHandler(map_KeyDown);
            Map.MouseMove += new MouseEventHandler(parentMap_MouseMove);
            magnifierMap.MouseMove += new MouseEventHandler(parentMap_MouseMove);
            Map.KeyUp += new KeyEventHandler(parentMap_KeyUp);

            magnifierMap.ParentMap = Map;
        }

        void parentMap_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.M && Visibility == Visibility.Visible)
            {
                Visibility = System.Windows.Visibility.Collapsed;
                magnifierMap.IsEnabled = false;
            }
        }

        Point m_CurrentMousePosition;
        Point m_CurrentMapPosition;
        void parentMap_MouseMove(object sender, MouseEventArgs e)
        {
            m_CurrentMousePosition = e.GetPosition(Map);
            m_CurrentMapPosition = e.GetPosition(Map.GeoCanvas);

            if (Visibility == System.Windows.Visibility.Visible)
            {
                Canvas.SetLeft(this, m_CurrentMousePosition.X - Width / 2);
                Canvas.SetTop(this, m_CurrentMousePosition.Y - Height / 2);
            }
        }
        
        void map_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.M && Visibility == Visibility.Collapsed)
            {
                magnifierMap.ZoomDelta = 2;
                magnifierMap.MaxZoom = magnifierMap.ParentMap.MaxZoom + (int)magnifierMap.ZoomDelta;

                double mapSize = Math.Min(Map.ActualHeight, Map.ActualWidth);
                mapSize = Math.Min(300, mapSize / 2);

                Clip = new EllipseGeometry{RadiusX = mapSize / 2, RadiusY = mapSize / 2, 
                    Center = new Point{X = mapSize / 2, Y = mapSize / 2}};
                
                this.Width = this.Height = mapSize;

                Canvas.SetLeft(this, m_CurrentMousePosition.X - Width / 2);
                Canvas.SetTop(this, m_CurrentMousePosition.Y - Height / 2);

                if (!Map.UseAnimation)
                {
                    magnifierMap.SetPosition(m_CurrentMapPosition, Map.Zoom + magnifierMap.ZoomDelta, false);
                }
                else
                {
                    magnifierMap.SetPosition(m_CurrentMapPosition, Map.Zoom - magnifierMap.ZoomDelta, false);
                    magnifierMap.ViewportEndChanged += new EventHandler(magnifierMap_ViewportEndChanged);
                }

                this.Visibility = Visibility.Visible;
                magnifierMap.IsEnabled = true;
            }
        }

        void magnifierMap_ViewportEndChanged(object sender, EventArgs e)
        {
            magnifierMap.ViewportEndChanged -= new EventHandler(magnifierMap_ViewportEndChanged);
            magnifierMap.SetPosition(m_CurrentMapPosition, Map.Zoom + magnifierMap.ZoomDelta, true);
        }
    }
}
