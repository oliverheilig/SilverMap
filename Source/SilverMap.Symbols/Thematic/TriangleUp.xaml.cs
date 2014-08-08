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
	/// <summary>
	/// Interaction logic for TriangleUp.xaml
	/// </summary>
	public partial class TriangleUp : UserControl
	{
		public TriangleUp()
		{
			this.InitializeComponent();
		}

		public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
		
        // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(TriangleUp), new PropertyMetadata(Colors.Blue));		
	}
}