using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Data;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System.Globalization;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.Phone.Tasks;
using MyToolkit.Multimedia;
using TVSeries80.Models;
using CreateTVSeriesDatabase.Models;
using HtmlAgilityPack;
using System.ServiceModel.Syndication;
using System.Xml;
using Utilities;

namespace TVSeries80
{
    public partial class EpisodePage : PhoneApplicationPage
    {
        private ApplicationBarIconButton mPrevAppBarButton;
        private ApplicationBarIconButton mNextAppBarButton;
        private ApplicationBarMenuItem mGotoSeriesAppBarMenuItem;
        private MobileServiceCollection<Series, Series> mSeriesItems;
        private IMobileServiceTable<Series> mSeriesTable = App.MobileService.GetTable<Series>();
        private Series mSeries = null;
        private ObservableCollection<PersonQuery> mPersonResults = new ObservableCollection<PersonQuery>();
        private MobileServiceCollection<Episode, Episode> mEpisodeItems;
        private IMobileServiceTable<Episode> mEpisodeTable = App.MobileService.GetTable<Episode>();
        private Episode mEpisode = null;
        private int mEpisodeID;
        private int mOffset;

        public EpisodePage()
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
                string value;
                if (NavigationContext.QueryString.TryGetValue("EpisodeID", out value))
                {
                    if (int.TryParse(value, out mEpisodeID))
                    {
                        Load(mEpisodeID);
                    }
                }
                if (NavigationContext.QueryString.TryGetValue("Traverse", out value))
                {
                    int traverseOptions = 0;
                    if (int.TryParse(value, out traverseOptions))
                    {
                        if (traverseOptions == 0)
                        {
                            // disable traversal buttons
                            mPrevAppBarButton.IsEnabled = false;
                            mNextAppBarButton.IsEnabled = false;
                        }
                    }
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

             if (e.NavigationMode == NavigationMode.Back)
            {
                // remove all episode items from settings when going back 
                // (Note: NavigationMode.New happens when user presses home button)
                if (App.ViewModel.EpisodeList != null)
                    App.ViewModel.EpisodeList.Clear();
            }
        }

        private async void Load(int episodeID)
        {
            Utilities.Utilities.Instance.SetProgress(true, "loading episode details...");

            // locate this item in the episodes list
            mOffset = 0;
            if (App.ViewModel.EpisodeList != null)
            {
                foreach (var i in App.ViewModel.EpisodeList)
                {
                    if (i == mEpisodeID)
                        break;
                    mOffset++;
                }
            }
    
            try
            {
                mEpisodeItems = await mEpisodeTable
                .Where(i => i.id == episodeID)
                .ToCollectionAsync<Episode>();
            }
            catch 
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading episode from mobile services");
            }
            finally
            {
                if (mEpisodeItems != null && mEpisodeItems.Count == 1)
                {
                    mEpisode = mEpisodeItems[0];
                    if (mEpisode != null)
                    {
                        // rebind
                        DataContext = mEpisode;
                    }
                }
            }
            
            // extract any useful series info
            try
            {
                if (mEpisode != null)
                {
                    mSeriesItems = await mSeriesTable
                    .Where(i => i.id == mEpisode.SeriesID)
                    .ToCollectionAsync<Series>();
                }
            }
            catch 
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading series from mobile services");
            }
            finally
            {
                if (mSeriesItems != null && mSeriesItems.Count == 1)
                {
                    mSeries = mSeriesItems[0];
                    if (mSeries != null && mEpisode != null)
                    {
                        // bindings
                        PivotControl.Title = mEpisode.CombinedName;
                    }
                }
            }

            // cast&crew pivot
            try
            {
                if (mEpisode != null)
                {
                    // Asynchronously call the custom API using the GET method
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("EpisodeID", mEpisode.id.ToString());
                    mPersonResults = await App.MobileService
                        .InvokeApiAsync<ObservableCollection<PersonQuery>>("getpersonbyepisode",
                        System.Net.Http.HttpMethod.Get, parameters);
                 }
            }
            catch
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading persons from mobile services");
            }
            finally
            {
                if (mPersonResults != null && mPersonResults.Count > 0)
                {
                    // re-bind episodes to episodes list
                    CastList.ItemsSource = mPersonResults;
                }
                else
                {
                    // remove the pivot, its useless
                    foreach (PivotItem item in PivotControl.Items.ToList())
                    {
                        if (((string)item.Header) == "cast+crew")
                        {
                            PivotControl.Items.Remove(item);
                            break;
                        }
                    }
                }

            }

            // Links links binding 
            LinksList.ItemsSource = App.ViewModel.EpisodeLinkOptions;

            Utilities.Utilities.Instance.SetProgress(false, null);
        }

        private void OnPersonSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector list = sender as LongListSelector;
            if (list != null)
            {
                PersonQuery ar = list.SelectedItem as PersonQuery;
                if (ar != null)
                {
                    NavigationService.Navigate(new Uri(String.Format("/PersonPage.xaml?PersonID={0}", ar.PersonID), UriKind.Relative));
                    // item remains selected, if not cleared
                    list.SelectedItem = null;
                }
            }
        }

        private void OnLinksOptionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector list = sender as LongListSelector;
            if (list != null)
            {
                string s = list.SelectedItem as String;
                if (s != null)
                {
                    if (s == "IMDB" & mEpisode.IMDB_ID != "")    // may as well link directly to it
                    {
                        object setting = App.ViewModel.LoadSettingsKey(App.INTERNAL_BROWSER_KEY);
                        if (setting != null)
                        {
                            if ((int)setting == 1)
                                NavigationService.Navigate(new Uri(String.Format("/WebBrowserPage.xaml?URI={0}", mEpisode.IMDB_ID), UriKind.Relative));
                            else
                                Utilities.Utilities.Instance.ExplorerLaunch(new Uri(mEpisode.IMDB_ID));
                        }
                    }
                    else
                    {
                        // try the series/episode name pair for greater accuracy
                        string query = string.Format("{0} {1}", mSeries.SeriesName, mEpisode.EpisodeName);
                        query = query.Replace(" ", "+");
                        NavigationService.Navigate(new Uri(String.Format("/LinksPage.xaml?ID={0}&q={1}&Type=Episode", s, query), UriKind.Relative));
                    }
                    list.SelectedItem = null;
                }
            }
        }

        private void OnSeriesBannerTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (mEpisode != null)
                NavigationService.Navigate(new Uri(String.Format("/SeriesPage.xaml?SeriesID={0}", mEpisode.SeriesID), UriKind.Relative));
        }

        #region Application bar

        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new previous button
            mPrevAppBarButton = new ApplicationBarIconButton(new Uri("/Images/back.png", UriKind.Relative));
            mPrevAppBarButton.Text = "prev";
            mPrevAppBarButton.Click += OnPrevClick;
            ApplicationBar.Buttons.Add(mPrevAppBarButton);

            // Create a new next button
            mNextAppBarButton = new ApplicationBarIconButton(new Uri("/Images/next.png", UriKind.Relative));
            mNextAppBarButton.Text = "next";
            mNextAppBarButton.Click += OnNextClick;
            ApplicationBar.Buttons.Add(mNextAppBarButton);

            // Create a goto home menu item
            mGotoSeriesAppBarMenuItem = new ApplicationBarMenuItem("goto series");
            mGotoSeriesAppBarMenuItem.Click += OnGotoSeriesClick;
            ApplicationBar.MenuItems.Add(mGotoSeriesAppBarMenuItem);

        }

        private void OnPrevClick(object sender, EventArgs e)
        {
            if (App.ViewModel.EpisodeList != null && App.ViewModel.EpisodeList.Count > 0)
            {
                mOffset--;
                if (mOffset < 0) mOffset = App.ViewModel.EpisodeList.Count - 1;
                mEpisodeID = App.ViewModel.EpisodeList[mOffset];
                Load(mEpisodeID);
            }
        }

        private void OnNextClick(object sender, EventArgs e)
        {
            if (App.ViewModel.EpisodeList != null && App.ViewModel.EpisodeList.Count > 0)
            {
                mOffset = (mOffset + 1) % App.ViewModel.EpisodeList.Count;
                mEpisodeID = App.ViewModel.EpisodeList[mOffset];
                Load(mEpisodeID);
            }
        }

        private void OnGotoSeriesClick(object sender, EventArgs e)
        {
            if (mEpisode != null)
                NavigationService.Navigate(new Uri(String.Format("/SeriesPage.xaml?SeriesID={0}", mEpisode.SeriesID), UriKind.Relative));
        }

        #endregion

    }
}