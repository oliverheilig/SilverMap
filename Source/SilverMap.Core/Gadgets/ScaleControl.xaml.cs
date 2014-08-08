//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Linq;

namespace Ptvag.Dawn.Controls.SilverMap.Core.Gadgets
{
    /// <summary>
    /// Interaction logic for ScaleControl.xaml
    /// </summary>
    public partial class ScaleControl : MapGadget
    {
        public double MaxLength = 120;

        ScaleInfo[] scales = new ScaleInfo[]
            {
                new ScaleInfo {Dimension = 1000000, Text = "1000 km"},
                new ScaleInfo {Dimension = 500000, Text = "500 km"},
                new ScaleInfo {Dimension = 250000, Text = "250 km"},
                new ScaleInfo {Dimension = 100000, Text = "100 km"},
                new ScaleInfo {Dimension = 50000, Text = "50 km"},
                new ScaleInfo {Dimension = 25000, Text = "25 km"},
                new ScaleInfo {Dimension = 10000, Text = "10 km"},
                new ScaleInfo {Dimension = 5000, Text = "5 km"},
                new ScaleInfo {Dimension = 2500, Text = "2,5 km"},
                new ScaleInfo {Dimension = 1000, Text = "1000 m"},
                new ScaleInfo {Dimension = 500, Text = "500 m"},
                new ScaleInfo {Dimension = 250, Text = "250 m"},
                new ScaleInfo {Dimension = 100, Text = "100 m"},
                new ScaleInfo {Dimension = 50, Text = "50 m"},
                new ScaleInfo {Dimension = 25, Text = "25 m"},
                new ScaleInfo {Dimension = 10, Text = "10 m"}
            };

        public ScaleControl()
        {
            InitializeComponent();
       }

       protected override void Initialize()
       {
            Map.ViewportWhileChanged += new EventHandler(Map_ViewportWhileChanged);

            UpdateScale();
        }

        private ScaleInfo FindBestScale(double metersPerPixel)
        {
            foreach (ScaleInfo scaleInfo in this.scales)
            {
                double length = scaleInfo.Dimension / metersPerPixel;
                if (length <= MaxLength)
                    return scaleInfo;
            }

            return scales.Last<ScaleInfo>();
        }

        private void Map_ViewportWhileChanged(object sender, EventArgs e)
        {
            UpdateScale();
        }

        private void UpdateScale()
        {
            // fade out if scale is too large
            Opacity = Map.CurrentScale > 2500 ? Math.Max(0, 1 - 0.0001 * (Map.CurrentScale - 2500)) : 1;

            // calculate meters per pixel considering the mercator projection
            double cosB = Math.Cos((Math.Atan(Math.Exp(Map.CurrentY / 6371000.0)) - (Math.PI / 4)) / 0.5);
            double metersPerPixel = Map.CurrentScale * cosB;

            ScaleInfo scaleInfo = FindBestScale(metersPerPixel);
            double length = scaleInfo.Dimension / metersPerPixel;

            this.ScaleCanvas.Width = length;
            this.Text.Text = scaleInfo.Text;
        }
    }

    public class ScaleInfo
    {
        public string Text { get; set; }
        public double Dimension { get; set; }
    }
}
