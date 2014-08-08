//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Windows.Controls;
using System.Windows;
using System;

namespace SilverMap.UseCases.SharpMap
{
    public partial class Legend : UserControl
    {
        public Legend()
        {
            InitializeComponent();

            buttonBing.NavigateUri = new Uri(App.BaseUrl + "/MMBing.html");
            buttonLeaflet.NavigateUri = new Uri(App.BaseUrl + "/MMLeaflet.html");
        }
    }
}