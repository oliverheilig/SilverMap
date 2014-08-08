//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Controls;

namespace SilverMap
{
    public partial class MainPage : UserControl
    {
        string[] themes = new string[]
        {
            "ShinyBlue", "ExpressionDark", "BubbleCreme", /*"BureauBlack",*/ "BureauBlue", "ExpressionLight", 
            /*"RainierOrange", "RainierPurple",*/ "ShinyRed", //"SystemColors",
            "TwilightBlue", "WhistlerBlue"
        };

        public MainPage()
        {
            InitializeComponent();

            this.MapPage.button3.Click += new RoutedEventHandler(button3_Click);
            
        }

        int themeidx = 0;
        void button3_Click(object sender, RoutedEventArgs e)
        {
            themeidx = (++themeidx) % themes.Length;

            ThemeContainer.ThemeUri = new Uri("/System.Windows.Controls.Theming." + themes[themeidx] + ";component/Theme.xaml", UriKind.RelativeOrAbsolute);
        }
    }
}
