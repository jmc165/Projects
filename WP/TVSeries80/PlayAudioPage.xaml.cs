using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace TVSeries80
{
    public partial class PlayAudioPage : PhoneApplicationPage
    {
        private string mURI = null;
        private string mName = null;

        public PlayAudioPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                if (NavigationContext.QueryString.TryGetValue("URI", out mURI) &&
                    NavigationContext.QueryString.TryGetValue("Name", out mName))
                {
                    Header.Text = mName;
                    Media.Source = new Uri(mURI);
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void OnAudioMediaOpened(object sender, RoutedEventArgs e)
        {
            Stop.IsEnabled = true;
            Pause.IsEnabled = true;
            Play.IsEnabled = false;
        }

        private void OnMediaEnded(object sender, RoutedEventArgs e)
        {
            Stop.IsEnabled = false;
            Pause.IsEnabled = false;
            Play.IsEnabled = true;
        }

        private void OnMediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            OnMediaEnded(sender, e);
        }

        private void OnStopMedia(object sender, RoutedEventArgs e)
        {
            Stop.IsEnabled = false;
            Pause.IsEnabled = false;
            Play.IsEnabled = true;
            Media.Stop();
        }

        private void OnPauseMedia(object sender, RoutedEventArgs e)
        {
            Stop.IsEnabled = false;
            Pause.IsEnabled = false;
            Play.IsEnabled = true;
            Media.Pause();
        }

        private void OnPlayMedia(object sender, RoutedEventArgs e)
        {
            Stop.IsEnabled = true;
            Pause.IsEnabled = true;
            Play.IsEnabled = false;
            Media.Play(); 
        }

 
   
    }
}