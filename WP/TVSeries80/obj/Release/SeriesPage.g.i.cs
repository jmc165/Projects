﻿#pragma checksum "C:\TFSProjects\WP\TVSeries80\SeriesPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "8D0556EC0C9C10EF5705436FFAD31C41"
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
    
    
    public partial class SeriesPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal Microsoft.Phone.Controls.Pivot PivotControl;
        
        internal System.Windows.Controls.TextBlock Genre;
        
        internal Microsoft.Phone.Controls.LongListSelector EpisodeList;
        
        internal Microsoft.Phone.Controls.LongListSelector CastList;
        
        internal Microsoft.Phone.Controls.LongListSelector LinksList;
        
        internal System.Windows.Controls.TextBlock SeriesCount;
        
        internal System.Windows.Controls.Button MoreSeasonBannersButton;
        
        internal Microsoft.Phone.Controls.LongListSelector SeasonBannerList;
        
        internal System.Windows.Controls.TextBlock PostersCount;
        
        internal System.Windows.Controls.Button MorePosterBannersButton;
        
        internal Microsoft.Phone.Controls.LongListSelector PosterBannerList;
        
        internal System.Windows.Controls.TextBlock FanartCount;
        
        internal System.Windows.Controls.Button MoreFanartBannersButton;
        
        internal Microsoft.Phone.Controls.LongListSelector FanartBannerList;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/TVSeries80;component/SeriesPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.PivotControl = ((Microsoft.Phone.Controls.Pivot)(this.FindName("PivotControl")));
            this.Genre = ((System.Windows.Controls.TextBlock)(this.FindName("Genre")));
            this.EpisodeList = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("EpisodeList")));
            this.CastList = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("CastList")));
            this.LinksList = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("LinksList")));
            this.SeriesCount = ((System.Windows.Controls.TextBlock)(this.FindName("SeriesCount")));
            this.MoreSeasonBannersButton = ((System.Windows.Controls.Button)(this.FindName("MoreSeasonBannersButton")));
            this.SeasonBannerList = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("SeasonBannerList")));
            this.PostersCount = ((System.Windows.Controls.TextBlock)(this.FindName("PostersCount")));
            this.MorePosterBannersButton = ((System.Windows.Controls.Button)(this.FindName("MorePosterBannersButton")));
            this.PosterBannerList = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("PosterBannerList")));
            this.FanartCount = ((System.Windows.Controls.TextBlock)(this.FindName("FanartCount")));
            this.MoreFanartBannersButton = ((System.Windows.Controls.Button)(this.FindName("MoreFanartBannersButton")));
            this.FanartBannerList = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("FanartBannerList")));
        }
    }
}

