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
    public partial class DynamicPlayerPage : PhoneApplicationPage
    {

        private const string MOBILE_MODE = "User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0; SAMSUNG; SGH-i917)";
        private const string DESKTOP_MODE = "User-Agent: Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Win64; x64; Trident/6.0)\r\n";

        public DynamicPlayerPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                //string value;
                //// page may be called via URL (series ID == TVDB_ID)
                //if (NavigationContext.QueryString.TryGetValue("SeriesID", out value))
                //{
                //    if (int.TryParse(value, out mSeriesID))
                //    {
                //        Load();
                //    }
                //}
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<html><head><meta name='viewport' content='width=480, user-scalable=yes' /></head><body>");
            sb.Append("<iframe src='http://www.dailymotion.com/video/x17llgg_captcha-x-criminal-vinyl_music' width='480' height='270'>");
            sb.Append("</iframe>");
            sb.Append("</body></html>");
            //WebBrowser.NavigateToString("<html><head></head><body><iframe src=\"http://www.dailymotion.com/embed/video/x17ey2b\" width=\"480\" height=\"270\" frameborder=\"0\"></iframe></body></html>");
            WebBrowser.Navigate(new Uri("http://www.dailymotion.com/video/x17llgg_captcha-x-criminal-vinyl_music"), null, MOBILE_MODE);
        }

    }
}