using System;
using System.Net;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.WindowsAzure.MobileServices;
using CreateTVSeriesDatabase.Models;
using TVSeries80.Models;
using Microsoft.Phone.Tasks;
using Utilities;

namespace TVSeries80
{
    public partial class MainPage : PhoneApplicationPage
    {
        private ApplicationBarIconButton mSearchAppBarButton;
        private ApplicationBarMenuItem mSettingsAppBarMenuItem;
        private ApplicationBarMenuItem mAboutAppBarMenuItem;

        // Constructor
        public MainPage()
        {
            DataContext = App.ViewModel;

            InitializeComponent();

            // code to localize the ApplicationBar
            BuildLocalizedApplicationBar();
        }

        #region events

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                Utilities.Utilities.Instance.CheckConnectivity();
            }

            // all these items can actually change outside this page,
            // so re-load everytime
            // load favorites
            App.ViewModel.LoadFavorites();
            // load favorites
            App.ViewModel.LoadHistory();
            // load newsfeeds from current settings
            App.ViewModel.LoadDisplayNews();
            // load downloads from isolated storage
            App.ViewModel.LoadDownloads();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void OnFeedSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector list = sender as LongListSelector;
            if (list != null)
            {
                News f = list.SelectedItem as News;
                if (f != null)
                    NavigationService.Navigate(new Uri(String.Format("/NewsPage.xaml?Name={0}&URI={1}&Images={2}", f.NewsName, f.URI, f.Images.ToString()), UriKind.Relative));
                list.SelectedItem = null;
            }
        }

        private void OnBrowseSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector list = sender as LongListSelector;
            if (list != null)
            {
                string s = list.SelectedItem as string;
                if (s != null)
                    NavigationService.Navigate(new Uri(String.Format("/BrowsePage.xaml?Option={0}", s), UriKind.Relative));
                list.SelectedItem = null;
            }
        }

        private void OnSearchSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector list = sender as LongListSelector;
            if (list != null)
            {
                string s = list.SelectedItem as string;
                if (s != null)
                    NavigationService.Navigate(new Uri(String.Format("/SearchPage.xaml?Option={0}", s), UriKind.Relative));
                list.SelectedItem = null;
            }
        }

        private void OnFavoriteSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector list = sender as LongListSelector;
            if (list != null)
            {
                Series s = list.SelectedItem as Series;
                if (s != null)
                    NavigationService.Navigate(new Uri(String.Format("/SeriesPage.xaml?SeriesID={0}", s.id), UriKind.Relative));
                list.SelectedItem = null;
            }
        }

        private void OnFavoriteDeleteClick(object sender, RoutedEventArgs e)
        {
            Series series = (sender as MenuItem).DataContext as Series;
            if (series != null)
            {
                App.ViewModel.RemoveFavorite(series);
            }
        }

        private void OnHistorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector list = sender as LongListSelector;
            if (list != null)
            {
                Series s = list.SelectedItem as Series;
                if (s != null)
                    NavigationService.Navigate(new Uri(String.Format("/SeriesPage.xaml?SeriesID={0}", s.id), UriKind.Relative));
                list.SelectedItem = null;
            }
        }

        private void OnHistoryDeleteClick(object sender, RoutedEventArgs e)
        {
            Series series = (sender as MenuItem).DataContext as Series;
            if (series != null)
            {
                App.ViewModel.RemoveHistory(series);
            }
        }

        private void OnTapPlay(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image image = sender as Image;
            if (image != null)
            {
                // get parent item using 
                Link link = image.DataContext as Link;
                if (link != null)
                {
                    try
                    {
                        MediaPlayerLauncher player = new MediaPlayerLauncher();
                        player.Media = new Uri(link.URI, UriKind.Relative);
                        player.Controls = MediaPlaybackControls.All;
                        player.Location = MediaLocationType.Data;
                        player.Show();
                    }
                    catch
                    {
                        Utilities.Utilities.Instance.MessageBoxShow("This file cannot be played on Windows Phone");
                    }
                }
            }
        }

        // nothing particular yet
        private void OnDownloadSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector list = sender as LongListSelector;
            if (list != null)
            {
                list.SelectedItem = null;
            }
        }

        private void OnDownloadDeleteClick(object sender, RoutedEventArgs e)
        {
            Link link = (sender as MenuItem).DataContext as Link;
            if (link != null)
            {
                App.ViewModel.RemoveDownload(link);
            }
        }

        #endregion

        # region app bar events

        private void OnSettingsClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(String.Format("/SettingsPage.xaml"), UriKind.Relative));
        }

        private void OnAboutClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(String.Format("/AboutPage.xaml"), UriKind.Relative));
        }

        private void OnSearchClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(String.Format("/SearchPage.xaml?Option=Series Name"), UriKind.Relative));
        }

        #endregion

        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            mSearchAppBarButton = new ApplicationBarIconButton(new Uri("/Images/feature.search.png", UriKind.Relative));
            mSearchAppBarButton.Text = "search";
            mSearchAppBarButton.Click += OnSearchClick;
            ApplicationBar.Buttons.Add(mSearchAppBarButton);

            // Create a new menu item for "settings"
            mSettingsAppBarMenuItem = new ApplicationBarMenuItem("settings");
            mSettingsAppBarMenuItem.Click += OnSettingsClick;
            ApplicationBar.MenuItems.Add(mSettingsAppBarMenuItem);

            // Create a new menu item for "about"
            mAboutAppBarMenuItem = new ApplicationBarMenuItem("about");
            mAboutAppBarMenuItem.Click += OnAboutClick;
            ApplicationBar.MenuItems.Add(mAboutAppBarMenuItem);
        }

    }
}