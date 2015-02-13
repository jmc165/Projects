using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Text;
using System.Globalization;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.WindowsAzure.MobileServices;
using MyToolkit.Multimedia;
using TVSeries80.Models;
using CreateTVSeriesDatabase.Models;
using System.ServiceModel.Syndication;
using System.Xml;
using System.IO;
using System.Threading.Tasks;
using Utilities;

namespace TVSeries80
{
    public partial class SeriesPage : PhoneApplicationPage
    {

        private enum EpisodeSort
        {
            Default,
            ByRating,
            ByName,
            ByAirDate,
        }

        private ApplicationBarIconButton mAddToFavsAppBarButton;
        private ApplicationBarMenuItem mPinToStartAppBarMenuItem;
        private ApplicationBarMenuItem mGotoHomeAppBarMenuItem;
        private MobileServiceCollection<Series, Series> mSeriesItems;
        private IMobileServiceTable<Series> mSeriesTable = App.MobileService.GetTable<Series>();
        private Series mSeries = null;
        private MobileServiceCollection<Episode, Episode> mEpisodeItems;
        private IMobileServiceTable<Episode> mEpisodeTable = App.MobileService.GetTable<Episode>();
        private ObservableCollection<PersonQuery> mPersonResults = new ObservableCollection<PersonQuery>();
        private ObservableCollection<Banner> mSeasonBannerItems = new ObservableCollection<Banner>();
        private ObservableCollection<Banner> mPosterBannerItems = new ObservableCollection<Banner>();
        private ObservableCollection<Banner> mFanartBannerItems = new ObservableCollection<Banner>();
        private int mEpisodesPage = 0;
        private int mItemsPerPage = 50;
        private int mOffsetKnob = 7;
        private int mSeriesID = -1;
        private EpisodeSort mEpisodeSort = EpisodeSort.Default;

        public SeriesPage()
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
                // page may be called via URL (series ID == TVDB_ID)
                if (NavigationContext.QueryString.TryGetValue("SeriesID", out value))
                {
                    if (int.TryParse(value, out mSeriesID))
                    {
                        Load();
                    }
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private async void Load()
        {
            Utilities.Utilities.Instance.SetProgress(true, "loading series details...");

            // if already favorites, disable add button
            bool enable = true;
            ObservableCollection<Series> favorites = App.ViewModel.LoadSettingsKey(App.FAVORITES_KEY) as ObservableCollection<Series>;
            if (favorites != null)
            {
                var exists =
                       from f in favorites
                       where f.id == mSeriesID
                       select f;
                enable = exists.Count<Series>() == 0;
            }
            mAddToFavsAppBarButton.IsEnabled = enable;

            // if this already has a secondary tile, disable the "pin to start" menu item
            string URI = String.Format("/SeriesPage.xaml?SeriesID={0}", mSeriesID);
            ShellTile oTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString() == URI);
            if (oTile != null)
                mPinToStartAppBarMenuItem.IsEnabled = false;
 
            // pivot page main
            try
            {
                 mSeriesItems = await mSeriesTable
                .Where(i => i.id == mSeriesID)
                .ToCollectionAsync<Series>();
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
                    if (mSeries != null)
                    {
                        // rebind
                        DataContext = mSeries;
                    }
                }
            }

            // extract genre(s) of this series, from the mobile services API
            try
            {
                if (mSeries != null)
                {
                    Genre.Text = "";
                    // Asynchronously call the custom API using the GET method
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("SeriesID", mSeries.id.ToString());
                    var results = await App.MobileService
                        .InvokeApiAsync<List<Genre>>("getgenrebyseries",
                        System.Net.Http.HttpMethod.Get, parameters);
                    List<string> l = new List<string>();
                    foreach (var r in results)
                        l.Add(r.Name);
                    if (l.Count > 0)
                        Genre.Text = String.Join(", ", l.ToArray());
                    else
                        Genre.Text = "";
                }
            }
            catch 
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading genres from mobile services");
            }
            finally
            {
            }

            // links pivot
            LinksList.ItemsSource = App.ViewModel.SeriesLinkOptions;

            // episode pivot
            LoadEpisodes(true);

            // cast pivot
            try
            {
                if (mSeries != null)
                {
                    mPersonResults.Clear();
                    // Asynchronously call the custom API using the GET method
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("SeriesID", mSeries.id.ToString());
                    var results = await App.MobileService
                        .InvokeApiAsync<List<PersonQuery>>("getpersonbyseries",
                        System.Net.Http.HttpMethod.Get, parameters);
                    // sort will be done by IComparer
                    results.Sort();
                    foreach (var r in results)
                        mPersonResults.Add(r);
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
                    // remove the cast pivot, its useless
                    foreach (PivotItem item in PivotControl.Items.ToList())
                    {
                        if (((string)item.Header) == "cast")
                        {
                            PivotControl.Items.Remove(item);
                            break;
                        }
                    }
                }
            }

            // artwork pivot
            int count1 = await LoadBanners(1, mSeasonBannerItems, SeasonBannerList, "se", 3);
            int count2 = await LoadBanners(2, mPosterBannerItems, PosterBannerList, "poster", 3);
            int count3 = await LoadBanners(3, mFanartBannerItems, FanartBannerList, "fan", 3);

            if (count1 == 0 && count2 == 0 && count3 == 0)
            {
                // remove the artwork pivot, its useless
                foreach (PivotItem item in PivotControl.Items.ToList())
                {
                    if (((string)item.Header) == "artwork")
                    {
                        PivotControl.Items.Remove(item);
                        break;
                    }
                }
            }

            if (mSeries != null)
                // add to recent
                App.ViewModel.AddHistory(mSeries);

            Utilities.Utilities.Instance.SetProgress(false, null); 
        }

        private async Task<int> LoadBanners(int id, ObservableCollection<Banner> bannerList, LongListSelector list, string matchString, int fetch)
        {
            try
            {
                if (mSeries != null)
                {
                    // Asynchronously call the custom API using the GET method
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    string parameter = string.Format("Banner.SeriesID={0} AND Banner.BannerType LIKE '%{1}%'", mSeries.TVDB_ID.ToString(), matchString);
                    parameters.Add("Parameter", parameter);
                    parameters.Add("Fetch", fetch.ToString());
                    bannerList = await App.MobileService
                        .InvokeApiAsync<ObservableCollection<Banner>>("getbannerbyseries",
                        System.Net.Http.HttpMethod.Get, parameters);
                }
            }
            catch
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading banners from mobile services");
            }
            finally
            {
                if (bannerList != null && bannerList.Count > 0)
                {
                    // re-bind banners to banners list
                    list.ItemsSource = bannerList;
                    switch (id)
                    {
                        case 1:
                            SeriesCount.Text = String.Format("Series({0})", bannerList.Count);
                            break;
                        case 2:
                            PostersCount.Text = String.Format("Posters({0})", bannerList.Count);
                            break;
                        case 3:
                            FanartCount.Text = String.Format("Fanart({0})", bannerList.Count);
                            break;
                    }
                }
            }
            return bannerList.Count();
        }

        // needs to be paged because of potential length
        private async void LoadEpisodes(bool clear)
        {
            MobileServiceCollection<Episode, Episode> pageItems = null;
            try
            {
                Utilities.Utilities.Instance.SetProgress(true, "loading episode list...");
                if (clear && mEpisodeItems != null)
                    mEpisodeItems.Clear();
                if (mSeries != null)
                {
                    switch (mEpisodeSort)
                    {
                        case EpisodeSort.Default:
                        default:
                            pageItems = await mEpisodeTable
                            .Where(i => i.SeriesID == mSeries.id)
                            .Skip(mEpisodesPage * mItemsPerPage)
                            .Take(mItemsPerPage)
                            .ToCollectionAsync<Episode>();
                            break;
                        case EpisodeSort.ByName:
                            pageItems = await mEpisodeTable
                            .Where(i => i.SeriesID == mSeries.id)
                            .Skip(mEpisodesPage * mItemsPerPage)
                            .Take(mItemsPerPage)
                            .OrderBy(i => i.EpisodeName)
                            .ToCollectionAsync<Episode>();
                            break;
                        case EpisodeSort.ByRating:
                            pageItems = await mEpisodeTable
                            .Where(i => i.SeriesID == mSeries.id && i.Rating != "Unrated")
                            .Skip(mEpisodesPage * mItemsPerPage)
                            .Take(mItemsPerPage)
                            .OrderByDescending(i => i.Rating)
                            .ToCollectionAsync<Episode>();
                            break;
                        case EpisodeSort.ByAirDate:
                            pageItems = await mEpisodeTable
                            .Where(i => i.SeriesID == mSeries.id && i.FirstAired != "Unknown")
                            .Skip(mEpisodesPage * mItemsPerPage)
                            .Take(mItemsPerPage)
                            .OrderByDescending(i => i.FirstAired)
                            .ToCollectionAsync<Episode>();
                            break;
                    }
                }
            }
            catch 
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading episode from mobile services");
            }
            finally
            {
                if (pageItems != null)
                {
                    if (mEpisodeItems == null)
                        mEpisodeItems = pageItems;
                    else
                    {
                        foreach (var i in pageItems)
                            mEpisodeItems.Add(i);
                    }
                }
                if (mEpisodeItems != null && mEpisodeItems.Count > 0)
                {
                    // re-bind episodes to episodes list
                    EpisodeList.ItemsSource = mEpisodeItems;
                }
            }
            Utilities.Utilities.Instance.SetProgress(false, null);
        }

        #region events

        // selecting a Episode from the list
        private void OnEpisodeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector list = sender as LongListSelector;
            if (list != null)
            {
                Episode episode = list.SelectedItem as Episode;
                if (episode != null)
                {
                    // save the entire list to app settings (as ints)
                    List<int> ids = new List<int>();
                    foreach (Episode ep in list.ItemsSource)
                        ids.Add(ep.id);
                     App.ViewModel.EpisodeList = ids;
                    NavigationService.Navigate(new Uri(String.Format("/EpisodePage.xaml?EpisodeID={0}&Traverse=1", episode.id), UriKind.Relative));
                }
                // item remains selected, if not cleared
                list.SelectedItem = null;
            }
        }

        // this function is used to implement infinite scrolling of a very long list
        private void OnEpisodeItemRealized(object sender, ItemRealizationEventArgs e)
        {
            if (e.ItemKind == LongListSelectorItemKind.Item)
            {
                if (EpisodeList.ItemsSource.Count > mOffsetKnob)
                {
                    if ((e.Container.Content as Episode).Equals(EpisodeList.ItemsSource[EpisodeList.ItemsSource.Count - mOffsetKnob]))
                    {
                        mEpisodesPage++;
                        LoadEpisodes(false);
                    }
                }
            }
        }

        private void OnBannerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector list = sender as LongListSelector;
            if (list != null)
            {
                // save the entire list to app settings
                Banner bn = list.SelectedItem as Banner;
                if (bn != null)
                {
                    // save the entire list to app settings (as ints)
                    List<int> ids = new List<int>();
                    foreach (Banner ba in list.ItemsSource)
                        ids.Add(ba.id);
                    App.ViewModel.BannerList = ids;
                    NavigationService.Navigate
                        (new Uri(String.Format("/ImagePage.xaml?BannerID={0}&SeriesID={1}", bn.id, mSeries.TVDB_ID), UriKind.Relative));
                    // item remains selected, if not cleared
                    list.SelectedItem = null;
                }
            }
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
                    if (s == "IMDB" & mSeries.IMDB_ID != "")    // may as well link directly to it
                    {
                        object setting = App.ViewModel.LoadSettingsKey(App.INTERNAL_BROWSER_KEY);
                        if (setting != null)
                        {
                            if ((int)setting == 1)
                                NavigationService.Navigate(
                                    new Uri(String.Format("/WebBrowserPage.xaml?URI={0}", mSeries.IMDB_ID), UriKind.Relative));
                            else
                                Utilities.Utilities.Instance.ExplorerLaunch(new Uri(mSeries.IMDB_ID));
                        }
                    }
                    else
                    {
                        string query = string.Format("{0}", mSeries.SeriesName);
                        query = Utilities.Utilities.Instance.MakeValidIdentifier(query);
                        query = query.Replace(" ", "+");
                        NavigationService.Navigate(new Uri(String.Format("/LinksPage.xaml?ID={0}&q={1}&Type=Series", s, query), UriKind.Relative));
                    }
                    list.SelectedItem = null;
                }
            }
        }

        private async void OnMoreSeasonBanners(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                if ((string)button.Content == "More")
                {
                    button.Content = "Less";
                    var result = await LoadBanners(1, mSeasonBannerItems, SeasonBannerList, "se", 10000);
                }
                else
                {
                    button.Content = "More";
                    var result = await LoadBanners(1, mSeasonBannerItems, SeasonBannerList, "se", 3);
                }
            }
        }

        private async void OnMorePosterBanners(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                if ((string)button.Content == "More")
                {
                    button.Content = "Less";
                    var result = await LoadBanners(2, mPosterBannerItems, PosterBannerList, "poster", 10000);
                }
                else
                {
                    button.Content = "More";
                    var result = await LoadBanners(2, mPosterBannerItems, PosterBannerList, "poster", 3);
                }
            }
        }

        private async void OnMoreFanartBanners(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                if ((string)button.Content == "More")
                {
                    button.Content = "Less";
                    var result = await LoadBanners(3, mFanartBannerItems, FanartBannerList, "fanart", 10000);
                }
                else
                {
                    button.Content = "More";
                    var result = await LoadBanners(3, mFanartBannerItems, FanartBannerList, "fanart", 3);
                }
            }
        }

        // resort, no option but to reload from page 0
        private void OnEpisodeInOrderDefaultButton(object sender, RoutedEventArgs e)
        {
            mEpisodeSort = EpisodeSort.Default;
            mEpisodesPage = 0;
            LoadEpisodes(true);
        }

        // resort, no option but to reload from page 0
        private void OnEpisodeInOrderRatingButton(object sender, RoutedEventArgs e)
        {
            mEpisodeSort = EpisodeSort.ByRating;
            mEpisodesPage = 0;
            LoadEpisodes(true);
        }


        // resort, no option but to reload from page 0
        private void OnEpisodeInOrderNameButton(object sender, RoutedEventArgs e)
        {
            mEpisodeSort = EpisodeSort.ByName;
            mEpisodesPage = 0;
            LoadEpisodes(true);
        }

        private void OnEpisodeInOrderAirDateButton(object sender, RoutedEventArgs e)
        {
            mEpisodeSort = EpisodeSort.ByAirDate;
            mEpisodesPage = 0;
            LoadEpisodes(true);
        }


        #endregion

        #region application bar

        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

           // Create a new "add to favs" button
            mAddToFavsAppBarButton = new ApplicationBarIconButton(new Uri("/Images/favs.addto.png", UriKind.Relative));
            mAddToFavsAppBarButton.Text = "add favs";
            mAddToFavsAppBarButton.IsEnabled = false;
            mAddToFavsAppBarButton.Click += OnAddToFavsClick;
            ApplicationBar.Buttons.Add(mAddToFavsAppBarButton);

            // Create a new pin to start menu item
            mPinToStartAppBarMenuItem = new ApplicationBarMenuItem("pin to start");
            mPinToStartAppBarMenuItem.Click += OnPinToStartClick;
            ApplicationBar.MenuItems.Add(mPinToStartAppBarMenuItem);

            // Create a goto home menu item
            mGotoHomeAppBarMenuItem = new ApplicationBarMenuItem("goto home");
            mGotoHomeAppBarMenuItem.Click += OnGotoHomeClick;
            ApplicationBar.MenuItems.Add(mGotoHomeAppBarMenuItem);

        }

        private void OnAddToFavsClick(object sender, EventArgs e)
        {
            if (mSeries != null)
            {
                if (App.ViewModel.AddFavorite(mSeries))
                {
                    Utilities.Utilities.Instance.MessageBoxShow("Added to favorites");
                    // only enable favorites and if not already a favorite
                    mAddToFavsAppBarButton.IsEnabled = false;
                }
            }
        }

        private void OnGotoHomeClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private void OnPinToStartClick(object sender, EventArgs e)
        {
            if (mSeries != null)
            {
                try
                {
                    // see if tile is already there
                    string URI = String.Format("/SeriesPage.xaml?SeriesID={0}", mSeries.id);
                    ShellTile oTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString() == URI);
                    if (oTile == null)
                    {
                        /*
                        // take two snapshots and store to local directory
                        var iconUri = Utilities.Utilities.Instance.ScreenshotToShellContent(
                            LayoutRoot,
                            string.Format("Series-{0}-Image", mSeries.TVDB_ID),
                            (int)this.ActualWidth, (int)this.ActualHeight,
                            202, 202);
                        var smallIconUri = Utilities.Utilities.Instance.ScreenshotToShellContent(
                             LayoutRoot,
                             string.Format("Series-{0}-SmallImage", mSeries.TVDB_ID),
                             (int)this.ActualWidth, (int)this.ActualHeight,
                             110, 110);
                         * */
                        FlipTileData tile = new FlipTileData()
                        {
                            Title = "TV Series Guru",
                            BackgroundImage = new Uri("/Assets/Tiles/FlipCycleTileMedium.png", UriKind.Relative),
                            SmallBackgroundImage = new Uri("/Assets/Tiles/FlipCycleTileSmall.png", UriKind.Relative),
                            BackBackgroundImage = new Uri("/Assets/Tiles/FlipCycleTileMedium.png", UriKind.Relative),
                            WideBackgroundImage = new Uri("/Assets/Tiles/FlipCycleTileLarge.png", UriKind.Relative),
                            BackTitle = "TV Series Guru",
                            BackContent = mSeries.SeriesName,
                            WideBackContent = mSeries.SeriesName + "\r" + ((mSeries.Overview.Length > 50) ? mSeries.Overview.Substring(0, 50) : mSeries.Overview) + "...",
                        };
                        Uri tileUri = new Uri(URI, UriKind.Relative);
                        ShellTile.Create(tileUri, tile, true);
                    }
                }
                catch
                {
                    MessageBox.Show("Error generating secondary tile");
                }
            }
        }

 
        #endregion  

    
    }
}