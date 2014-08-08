//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ptvag.Dawn.Controls.WpfMap.Symbols
{
    public partial class Ball : UserControl
	{
		public Ball()
		{
			// Required to initialize variables
			InitializeComponent();

			SetValue(LightColorProperty, Color.Lighten(1.5f));
            SetValue(DarkColorProperty, Color.Lighten(0.5f));
		}
		
		public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

		private static void ColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
             d.SetValue(LightColorProperty, ((Color)e.NewValue).Lighten(1.5f));
             d.SetValue(DarkColorProperty, ((Color)e.NewValue).Lighten(0.5f));
		}
		
		// Using a DependencyProperty as the backing store for DarkColorProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DarkColorProperty =
            DependencyProperty.Register("DarkColor", typeof(Color), typeof(Ball), null);

        // Using a DependencyProperty as the backing store for LightColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LightColorProperty =
            DependencyProperty.Register("LightColor", typeof(Color), typeof(Ball), null);
		
        // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(Ball), new PropertyMetadata(Colors.Blue, ColorChanged));		
	}
}