using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Globalization;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NewsPrint80.Models;
using NewsPrint80.Resources;

namespace NewsPrint80
{
    public partial class DisplayNewspaperPage : PhoneApplicationPage
    {
        private const string MOBILE_MODE = "User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0; SAMSUNG; SGH-i917)";
        private const string DESKTOP_MODE = "User-Agent: Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Win64; x64; Trident/6.0)\r\n";
  
        private Grouping mGroup = null;
        private ApplicationBarIconButton mAddToFavsAppBarButton;
        private ApplicationBarIconButton mRefreshAppBarButton;
        private ApplicationBarMenuItem mExplorerAppBarMenuItem;
        private ApplicationBarIconButton mBackAppBarButton;
        private ApplicationBarIconButton mForwardAppBarButton;

        public DisplayNewspaperPage()
        {
            bool ls = false;

            InitializeComponent();

            // code to localize the ApplicationBar
            BuildLocalizedApplicationBar();
            
            if (App.MainViewModel.ContainsSettingsKey(App.LOCATION_SERVICES_KEY))
                ls = (bool)App.MainViewModel.LoadSettingsKey(App.LOCATION_SERVICES_KEY);
            else
                ls = false;

#if DEBUG
            Ad.AdUnitId = "Image480_80";
            Ad.ApplicationId = "test_client";
#else
            Ad.AdUnitId = "10126803";
            Ad.ApplicationId = "c0276994-e723-4ced-aabb-ae0528975be1";
#endif

        }

        #region events

        private void OnAdRefreshed(object sender, EventArgs e)
        {
            Ad.Visibility = System.Windows.Visibility.Visible;
        }

        private void OnAdErrorOcurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            Ad.Visibility = System.Windows.Visibility.Collapsed;
#if DEBUG
            System.Diagnostics.Debug.WriteLine("Ad Error : " + e.ErrorCode);
#endif
        }

   
        #region web browser events

        private void OnWebBrowserNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            // collapse intermediate progress
            Utilities.Utilities.Instance.SetProgress(false, null);
        }

        private void OnWebBrowserNavigated(object sender, NavigationEventArgs e)
        {
            // collapse intermediate progress
            Utilities.Utilities.Instance.SetProgress(false, null);

            // enable the buttons
            mAddToFavsAppBarButton.IsEnabled = true;
            mRefreshAppBarButton.IsEnabled = true;
            mBackAppBarButton.IsEnabled = WebBrowser.CanGoBack;
            mForwardAppBarButton.IsEnabled = WebBrowser.CanGoForward;

            // add this group to the recents list
            App.MainViewModel.RecentsViewModel.AddItem(new Recent(mGroup));

            // only enable favorites if not already a favorite
            if (mGroup != null)
                mAddToFavsAppBarButton.IsEnabled = !App.MainViewModel.FavoritesViewModel.LocateItem(mGroup.URL);
        }

        private void OnWebBrowserNavigating(object sender, NavigatingEventArgs e)
        {
            // show intermediate progress
            Utilities.Utilities.Instance.SetProgress(true, null);
         }

        private void OnBackClick(object sender, EventArgs e)
        {
            WebBrowser.GoBack();
        }

        private void OnForwardClick(object sender, EventArgs e)
        {
            WebBrowser.GoForward();
        }

        #endregion

        private void OnPageBackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string groupString = null;

            if (e.NavigationMode != NavigationMode.Back)
            {
                if (NavigationContext.QueryString.TryGetValue("Group", out groupString))
                {
                    // demangle the group from URL
                    mGroup = new Grouping(groupString);

                    if (mGroup != null)
                    {

                        string userAgent = MOBILE_MODE;
                        if (App.MainViewModel.LoadSettingsKey(App.WEBSITE_STYLE_KEY) != null)
                        {
                            string value = App.MainViewModel.LoadSettingsKey(App.WEBSITE_STYLE_KEY) as string;
                            if (value == "Mobile")
                                userAgent = MOBILE_MODE;
                            else if (value == "Desktop")
                                userAgent = DESKTOP_MODE;
                        }

                        // navigate to the web page
                        WebBrowser.Navigate(new Uri(mGroup.URL, UriKind.Absolute), null, userAgent);
                    }
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        #endregion

        #region app bar events
  
        private void OnAddToFavsClick(object sender, EventArgs e)
        {
            if (mGroup != null)
            {
                Favorite f = new Favorite(mGroup);
                // add this group to the Favorites list,if not already there
                if (App.MainViewModel.FavoritesViewModel.AddItem(f))
                {
                    MessageBox.Show(AppResources.MessageBoxAddedToFavorites);
                    // only enable favorites and if not already a favorite
                    mAddToFavsAppBarButton.IsEnabled = false; 
                }
            }
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {

            if (mGroup != null && mGroup.URL != "")
            {
                // re-navigate to the web page
                WebBrowser.Navigate(new Uri(mGroup.URL, UriKind.Absolute));
            }
        }

        private void OnExplorerClick(object sender, EventArgs e)
        {
            if (mGroup != null && mGroup.URL != "")
            {
                ExplorerLaunch(new Uri(mGroup.URL));
            }
        }

        // Launch the URI
        private async void ExplorerLaunch(Uri uri)
        {
            // Launch the URI
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);

            if (success)
            {
                // URI launched OK
            }
            else
            {
                // URI failed
            }
        }

        #endregion

        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            mBackAppBarButton = new ApplicationBarIconButton(new Uri("/Images/back.png", UriKind.Relative));
            mBackAppBarButton.Text = AppResources.AppBarItemBackText;
            mBackAppBarButton.IsEnabled = false;
            mBackAppBarButton.Click += OnBackClick;
            ApplicationBar.Buttons.Add(mBackAppBarButton);

            // Create a new button and set the text value to the localized string from AppResources.
            mAddToFavsAppBarButton = new ApplicationBarIconButton(new Uri("/Images/favs.addto.png", UriKind.Relative));
            mAddToFavsAppBarButton.Text = AppResources.AppBarItemAddToFavsText;
            mAddToFavsAppBarButton.IsEnabled = false;
            mAddToFavsAppBarButton.Click += OnAddToFavsClick;
            ApplicationBar.Buttons.Add(mAddToFavsAppBarButton);

            // Create a new button and set the text value to the localized string from AppResources.
            mRefreshAppBarButton = new ApplicationBarIconButton(new Uri("/Images/refresh.png", UriKind.Relative));
            mRefreshAppBarButton.Text = AppResources.AppBarItemRefreshText;
            mRefreshAppBarButton.IsEnabled = false;
            mRefreshAppBarButton.Click += OnRefreshClick;
            ApplicationBar.Buttons.Add(mRefreshAppBarButton);

            // Create a new button and set the text value to the localized string from AppResources.
            mForwardAppBarButton = new ApplicationBarIconButton(new Uri("/Images/next.png", UriKind.Relative));
            mForwardAppBarButton.Text = AppResources.AppBarItemForwardText;
            mForwardAppBarButton.IsEnabled = false;
            mForwardAppBarButton.Click += OnForwardClick;
            ApplicationBar.Buttons.Add(mForwardAppBarButton);

            // Create a new menu item with the localized string from AppResources.
            mExplorerAppBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarItemExplorerText);
            mExplorerAppBarMenuItem.Click += OnExplorerClick;
            ApplicationBar.MenuItems.Add(mExplorerAppBarMenuItem);
  
        }

  
 

    }
}