﻿#pragma checksum "C:\TFSProjects\WP\TVSeries80\EpisodePage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "BC789F510F36ECF1EA57848F0DF04C8F"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18051
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace TVSeries80 {
    
    
    public partial class EpisodePage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal Microsoft.Phone.Controls.Pivot PivotControl;
        
        internal Microsoft.Phone.Controls.LongListSelector CastList;
        
        internal Microsoft.Phone.Controls.LongListSelector LinksList;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/TVSeries80;component/EpisodePage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.PivotControl = ((Microsoft.Phone.Controls.Pivot)(this.FindName("PivotControl")));
            this.CastList = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("CastList")));
            this.LinksList = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("LinksList")));
        }
    }
}
