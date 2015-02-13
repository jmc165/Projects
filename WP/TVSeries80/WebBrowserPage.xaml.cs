using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Utilities;

namespace TVSeries80
{
    public partial class WebBrowserPage : PhoneApplicationPage
    {
        private const string MOBILE_MODE = "User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0; SAMSUNG; SGH-i917)";
        private const string DESKTOP_MODE = "User-Agent: Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Win64; x64; Trident/6.0)\r\n";

        private string mURI;
        private ApplicationBarIconButton mRefreshAppBarButton;
        private ApplicationBarIconButton mBackAppBarButton;
        private ApplicationBarIconButton mForwardAppBarButton;
        private ApplicationBarMenuItem mExplorerAppBarMenuItem;
        private Uri mLastUri;
 
        public WebBrowserPage()
        {
            InitializeComponent();

            // code to localize the ApplicationBar
            BuildLocalizedApplicationBar();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                if (NavigationContext.QueryString.TryGetValue("URI", out mURI))
                {
                    // show intermediate progress
                    Utilities.Utilities.Instance.SetProgress(true, "loading page...");

                    // navigate to the web page
                    string userAgent = DESKTOP_MODE;
                    if (App.ViewModel.LoadSettingsKey(App.WEBSITE_STYLE_KEY) != null)
                    {
                        string value = App.ViewModel.LoadSettingsKey(App.WEBSITE_STYLE_KEY) as string;
                        if (value == "Mobile")
                            userAgent = MOBILE_MODE;
                        else if (value == "Desktop")
                            userAgent = DESKTOP_MODE;
                    }
                    WebBrowser.Navigate(new Uri(mURI, UriKind.Absolute), null, userAgent);
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            mBackAppBarButton = new ApplicationBarIconButton(new Uri("/Images/back.png", UriKind.Relative));
            mBackAppBarButton.Text = "back";
            mBackAppBarButton.IsEnabled = false;
            mBackAppBarButton.Click += OnBackClick;
            ApplicationBar.Buttons.Add(mBackAppBarButton);

            // Create a new refresh button
            mRefreshAppBarButton = new ApplicationBarIconButton(new Uri("/Images/refresh.png", UriKind.Relative));
            mRefreshAppBarButton.Text = "refresh";
            mRefreshAppBarButton.IsEnabled = false;
            mRefreshAppBarButton.Click += OnRefreshClick;
            ApplicationBar.Buttons.Add(mRefreshAppBarButton);

            // Create a new button and set the text value to the localized string from AppResources.
            mForwardAppBarButton = new ApplicationBarIconButton(new Uri("/Images/next.png", UriKind.Relative));
            mForwardAppBarButton.Text = "next";
            mForwardAppBarButton.IsEnabled = false;
            mForwardAppBarButton.Click += OnForwardClick;
            ApplicationBar.Buttons.Add(mForwardAppBarButton);

           // Create a new "launc in ie" menu item 
            mExplorerAppBarMenuItem = new ApplicationBarMenuItem("launch in ie");
            mExplorerAppBarMenuItem.Click += OnExplorerClick;
            ApplicationBar.MenuItems.Add(mExplorerAppBarMenuItem);

        }

        #region web browser events

        private void OnWebBrowserNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            // collapse intermediate progress
            Utilities.Utilities.Instance.SetProgress(false, null);      
        }

        private void OnWebBrowserNavigated(object sender, NavigationEventArgs e)
        {
            mLastUri = e.Uri;

            // collapse intermediate progress
            Utilities.Utilities.Instance.SetProgress(false, null);

            // enable the buttons
            mRefreshAppBarButton.IsEnabled = true;
            mBackAppBarButton.IsEnabled = WebBrowser.CanGoBack;
            mForwardAppBarButton.IsEnabled = WebBrowser.CanGoForward;

        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            if (mLastUri != null)
            {
                // re-navigate to the web page
                WebBrowser.Navigate(mLastUri);
            }
        }

        private void OnBackClick(object sender, EventArgs e)
        {
            WebBrowser.GoBack();
        }

        private void OnForwardClick(object sender, EventArgs e)
        {
            WebBrowser.GoForward();
        }

        private void OnExplorerClick(object sender, EventArgs e)
        {
            if (mURI != null)
            {
                Utilities.Utilities.Instance.ExplorerLaunch(new Uri(mURI));
            }
        }

        #endregion

    }
}