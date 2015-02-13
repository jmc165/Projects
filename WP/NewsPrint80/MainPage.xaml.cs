using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NewsPrint80.Resources;
using NewsPrint80.Models;

namespace NewsPrint80
{
    public partial class MainPage : PhoneApplicationPage
    {
        private ApplicationBarIconButton mSearchAppBarButton;
        private ApplicationBarMenuItem mSettingsAppBarMenuItem;
        private ApplicationBarMenuItem mAboutAppBarMenuItem;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the main page to the main view model
            this.DataContext = App.MainViewModel;

            // load countries
            App.MainViewModel.NewsPrintViewModel.Load();

            // load the favourites
            App.MainViewModel.FavoritesViewModel.Load();

            // load the recent
            App.MainViewModel.RecentsViewModel.Load();

             // code to localize the ApplicationBar
            BuildLocalizedApplicationBar();
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (App.MainViewModel.ContainsSettingsKey(App.LAST_PIVOT_PAGE_KEY))
            {
                PivotControl.SelectedIndex = (int)App.MainViewModel.LoadSettingsKey(App.LAST_PIVOT_PAGE_KEY); 
            }
      }

        // Load data for the ViewModel Items
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                App.MainViewModel.SaveSettingsKey(App.LAST_PIVOT_PAGE_KEY, PivotControl.SelectedIndex);
            }

        }

        #region recents events

        // selecting a recent
        private void OnRecentSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector list = sender as LongListSelector;
            if (list != null)
            {
                Recent r = list.SelectedItem as Recent;
                if (r != null)
                {
                    Grouping g = new Grouping(r);
                    if (g != null)
                    {
                        NavigationService.Navigate(new Uri(String.Format("/DisplayNewspaperPage.xaml?Group={0}", g), UriKind.Relative));
                    }
                }
                // item remains selected, if not cleared
                this.BrowseRecents.SelectedItem = null;
            }
        }

        private void OnRecentDeleteClick(object sender, RoutedEventArgs e)
        {
            // data context of menu item is same as current list selector item (thank god)
            Recent r = (sender as MenuItem).DataContext as Recent;
            if (r != null)
            {
                App.MainViewModel.RecentsViewModel.RemoveItem(r);
            }
        }       
        
        #endregion

        #region favorites events

        // selecting a Favorite
        private void OnFavoriteSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector list = sender as LongListSelector;
            if (list != null)
            {
                Favorite f = list.SelectedItem as Favorite;
                if (f != null)
                {
                    Grouping g = new Grouping(f);
                    if (g != null)
                    {
                        NavigationService.Navigate(new Uri(String.Format("/DisplayNewspaperPage.xaml?Group={0}", g), UriKind.Relative));
                    }
                }
                // item remains selected, if not cleared
                this.BrowseFavorites.SelectedItem = null;
            }
        }

        private void OnFavoriteDeleteClick(object sender, RoutedEventArgs e)
        {
            // data context of menu item is same as current list selector item (thank god)
            Favorite f = (sender as MenuItem).DataContext as Favorite;
            if (f != null)
            {
                App.MainViewModel.FavoritesViewModel.RemoveItem(f);
            }
        }

        #endregion

        #region country events

        // selecting an country
        private void OnCountrySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector list = sender as LongListSelector;
            if (list != null)
            {
                Country c = list.SelectedItem as Country;
                if (c != null)
                {
                    NavigationService.Navigate(new Uri(String.Format("/BrowseRegionsPage.xaml?CountryID={0}", c.CountryID), UriKind.Relative));
                }
                // item remains selected, if not cleared
                this.BrowseCountries.SelectedItem = null;
            }
        }

        #endregion

        # region app bar events

        private void OnSearchClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(String.Format("/SearchPage.xaml"), UriKind.Relative));
        }

        private void OnAboutClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(String.Format("/AboutPage.xaml"), UriKind.Relative));
        }

        private void OnSettingsClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(String.Format("/SettingsPage.xaml"), UriKind.Relative));
        }

        #endregion

        #region main page events

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        #endregion

        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            mSearchAppBarButton = new ApplicationBarIconButton(new Uri("/Images/feature.search.png", UriKind.Relative));
            mSearchAppBarButton.Text = AppResources.AppBarItemSearchText;
            mSearchAppBarButton.Click += OnSearchClick;
            ApplicationBar.Buttons.Add(mSearchAppBarButton);

            // Create a new menu item with the localized string from AppResources.
            mSettingsAppBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarItemSettingsText);
            mSettingsAppBarMenuItem.Click += OnSettingsClick;
            ApplicationBar.MenuItems.Add(mSettingsAppBarMenuItem);

            // Create a new menu item with the localized string from AppResources.
            mAboutAppBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarItemAboutText);
            mAboutAppBarMenuItem.Click += OnAboutClick;
            ApplicationBar.MenuItems.Add(mAboutAppBarMenuItem);
        }


    }
}