//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Windows.Controls;

namespace Ptvag.Dawn.Controls.SilverMap.Core
{
    /// <summary>
    /// The default Map Control. The map control is just a container for the core map 
    /// and the gadgets. You can build your own map control just by copying and re-arringing
    /// the default container.
    /// </summary>
    public partial class MapControl : UserControl
    {
        public MapControl()
        {
            InitializeComponent();
        }

        public LayerManager LayerManager
        {
            get
            {
                return layerManagerElement.layerManager;
            }
        }
    }
}
