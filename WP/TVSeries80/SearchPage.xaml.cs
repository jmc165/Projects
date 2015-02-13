using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.WindowsAzure.MobileServices;
using TVSeries80.Models;
using CreateTVSeriesDatabase.Models;
using System.Windows.Documents;
using Utilities;

namespace TVSeries80
{
    public partial class SearchPage : PhoneApplicationPage
    {
        private string mOption;
        private string mQuery;
        private int mItemsPerPage = 50;
        private bool mProcessSelectedItem = true;

        public SearchPage()
        {
            InitializeComponent();

            // get the seach limit from settings
            if (App.ViewModel.LoadSettingsKey(App.SEARCH_LIMIT_KEY) != null)
            {
                mItemsPerPage = (int)App.ViewModel.LoadSettingsKey(App.SEARCH_LIMIT_KEY);
            }
        }

        #region events

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.Back)
            {

                if (NavigationContext.QueryString.TryGetValue("Option", out mOption))
                {
                    Header.Text = String.Format("Search: '{0}'", mOption);
                    if (NavigationContext.QueryString.TryGetValue("Query", out mQuery))
                    {
                        // store query to view model
                        App.ViewModel.CurrentQuery = mQuery;
                        switch (mOption)
                        {
                            case "Series Name":
                                LoadSeriesName(List);
                                break;
                            case "Series Overview":
                                if (App.ViewModel.IsTrialMode)
                                    Utilities.Utilities.Instance.MessageBoxShow("This search option is disabled in Trial Mode");
                                else
                                    LoadSeriesOverview(List);
                                break;
                            case "Episode Name":
                                if (App.ViewModel.IsTrialMode)
                                    Utilities.Utilities.Instance.MessageBoxShow("This search option is disabled in Trial Mode");
                                else
                                    LoadEpisodeName(List);
                                break;
                            case "Episode Overview":
                                if (App.ViewModel.IsTrialMode)
                                    Utilities.Utilities.Instance.MessageBoxShow("This search option is disabled in Trial Mode");
                                else
                                    LoadEpisodeOverview(List);
                                break;
                            case "Any Person":
                                if (App.ViewModel.IsTrialMode)
                                    Utilities.Utilities.Instance.MessageBoxShow("This search option is disabled in Trial Mode");
                                else
                                    LoadPerson(List, mOption);
                                break;
                            case "Actor":
                                if (App.ViewModel.IsTrialMode)
                                    Utilities.Utilities.Instance.MessageBoxShow("This search option is disabled in Trial Mode");
                                else
                                    LoadPerson(List, mOption);
                                break;
                            case "Writer":
                                if (App.ViewModel.IsTrialMode)
                                    Utilities.Utilities.Instance.MessageBoxShow("This search option is disabled in Trial Mode");
                                else
                                    LoadPerson(List, mOption);
                                break;
                            case "Director":
                                if (App.ViewModel.IsTrialMode)
                                    Utilities.Utilities.Instance.MessageBoxShow("This search option is disabled in Trial Mode");
                                else
                                    LoadPerson(List, mOption);
                                break;
                        }
                    }
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void OnSearchItemKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (SearchItem.Text != null && SearchItem.Text != "")
                {
                    // not for postback to work different URI must be used
                    NavigationService.Navigate(new Uri(String.Format("/SearchPage.xaml?Random={0}&Option={1}&Query={2}", Guid.NewGuid(), mOption, SearchItem.Text), UriKind.Relative));
                }
            }
        }

        // selecting a Series from the list
        private void OnItemSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mProcessSelectedItem)
            {
                LongListSelector list = sender as LongListSelector;
                if (list != null)
                {
                    Series series = list.SelectedItem as Series;
                    if (series != null)
                    {
                        NavigationService.Navigate(new Uri(String.Format("/SeriesPage.xaml?SeriesID={0}", series.id), UriKind.Relative));
                    }
                    EpisodeQuery episode = list.SelectedItem as EpisodeQuery;
                    if (episode != null)
                    {
                        NavigationService.Navigate(new Uri(String.Format("/EpisodePage.xaml?EpisodeID={0}&Traverse=0", episode.EpisodeID), UriKind.Relative));
                    }
                    PersonQuery person = list.SelectedItem as PersonQuery;
                    if (person != null)
                    {
                        switch (person.CastType)
                        {
                            case PersonInstance.MAIN_ACTOR:
                                NavigationService.Navigate(new Uri(String.Format("/SeriesPage.xaml?SeriesID={0}", person.SeriesID), UriKind.Relative));
                                break;
                            case PersonInstance.GUEST_STAR:
                            case PersonInstance.WRITER:
                            case PersonInstance.DIRECTOR:
                                NavigationService.Navigate(new Uri(String.Format("/EpisodePage.xaml?EpisodeID={0}&Traverse=0", person.EpisodeID), UriKind.Relative));
                                break;
                        }
                    }
                    list.SelectedItem = null;
                }
            }
            else
            {
                mProcessSelectedItem = true;
                LongListSelector list = sender as LongListSelector;
                if (list != null)
                    list.SelectedItem = null;
            }
        }

        private void OnPersonImageTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image i = sender as Image;
            if (i != null)
            {
                // get parent item using 
                PersonQuery ar = i.DataContext as PersonQuery;
                if (ar != null)
                {
                    mProcessSelectedItem = false;
                    NavigationService.Navigate(new Uri(String.Format("/PersonPage.xaml?PersonID={0}", ar.PersonID), UriKind.Relative));
                }
            }
        }

        private void OnSeriesImageTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image i = sender as Image;
            if (i != null)
            {
                // get parent item using 
                PersonQuery ar = i.DataContext as PersonQuery;
                if (ar != null)
                {
                    mProcessSelectedItem = false;
                    NavigationService.Navigate(new Uri(String.Format("/SeriesPage.xaml?SeriesID={0}", ar.SeriesID), UriKind.Relative));
                }
            }
        }

        private void OnSeriesOverviewLoaded(object sender, RoutedEventArgs e)
        {
            if (mQuery != null)
            {
                TextBlock tb = sender as TextBlock;
                if (tb != null)
                {
                    Utilities.Utilities.Instance.HighlightText(tb, mQuery);
                }
            }
        }

        #endregion

        private async void LoadSeriesName(LongListSelector list)
        {
            try
            {
                // reload searchItem item it gets wiped on postback
                SearchItem.Text = mQuery;
                ObservableCollection<Series> items = null;
                try
                {
                    Utilities.Utilities.Instance.SetProgress(true, "loading matching series...");
                    // Asynchronously call the custom API using the GET method
                    // this doesn't seem to be able to be paged
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    // split seach items into a LIKE statement
                    string[] words = mQuery.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < words.Length; i++)
                    {
                        words[i] = String.Format("Series.SeriesName LIKE '%{0}%'", words[i]);
                    }
                    // combine with ORs between each word
                    string searchItem = String.Join(" AND ", words);
                    searchItem += " ORDER BY Series.SeriesName";
                    parameters.Add("Parameter", searchItem);
                    parameters.Add("Fetch", mItemsPerPage.ToString());
                    items = await App.MobileService
                        .InvokeApiAsync<ObservableCollection<Series>>("getseriesbynamematch",
                        System.Net.Http.HttpMethod.Get, parameters);
                }
                catch 
                {
                    Utilities.Utilities.Instance.MessageBoxShow("Error loading series from mobile services");
                }
                finally
                {
                    if (items != null && items.Count > 0)
                    {
                        List.ItemsSource = items;
                        List.LayoutMode = LongListSelectorLayoutMode.List;
                        List.ItemTemplate = this.Resources["SeriesNameItemTemplate"] as DataTemplate;
                        Header.Text = String.Format("{0} item(s) found", items.Count);
                    }
                    else
                    {
                        List.ItemsSource = null;
                        Header.Text = String.Format("0 items found");
                    }
                }
            }
            catch 
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error searching series from mobile services");
            }
            finally
            {
                Utilities.Utilities.Instance.SetProgress(false, null);
            }
        }

        private async void LoadSeriesOverview(LongListSelector list)
        {
            try
            {
                // reload searchItem item it gets wiped on postback
                SearchItem.Text = mQuery;
                // store to state
                ObservableCollection<Series> items = null;
                try
                {
                    Utilities.Utilities.Instance.SetProgress(true, "loading matching series...");
                    // Asynchronously call the custom API using the GET method
                    // this doesn't seem to be able to be paged
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    // split seach items into a LIKE statement
                    string[] words = mQuery.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < words.Length; i++)
                    {
                        words[i] = String.Format("Series.Overview LIKE '%{0}%'", words[i]);
                    }
                    // combine with ORs between each word
                    string searchItem = String.Join(" AND ", words);
                    searchItem += " ORDER BY Series.SeriesName";
                    parameters.Add("Parameter", searchItem);
                    parameters.Add("Fetch", mItemsPerPage.ToString());
                    items = await App.MobileService
                        .InvokeApiAsync<ObservableCollection<Series>>("getseriesbyoverviewmatch",
                        System.Net.Http.HttpMethod.Get, parameters);
                }
                catch 
                {
                    Utilities.Utilities.Instance.MessageBoxShow("Error loading series from mobile services");
                }
                finally
                {
                    if (items != null && items.Count > 0)
                    {
                        List.ItemsSource = items;
                        List.LayoutMode = LongListSelectorLayoutMode.List;
                        List.ItemTemplate = this.Resources["SeriesOverviewItemTemplate"] as DataTemplate;
                        Header.Text = String.Format("{0} item(s) found", items.Count);
                    }
                    else
                    {
                        List.ItemsSource = null;
                        Header.Text = String.Format("0 items found");
                    }
                }
            }
            catch 
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading series from mobile services");
            }
            finally
            {
                Utilities.Utilities.Instance.SetProgress(false, null);
            }
        }

        private async void LoadEpisodeName(LongListSelector list)
        {
            try
            {
                // reload searchItem item it gets wiped on postback
                SearchItem.Text = mQuery;
                ObservableCollection<EpisodeQuery> items = null;
                try
                {
                    Utilities.Utilities.Instance.SetProgress(true, "loading matching episodes...");
                    // Asynchronously call the custom API using the GET method
                    // this doesn't seem to be able to be paged
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    // split seach items into a LIKE statement
                    string[] words = mQuery.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < words.Length; i++)
                    {
                        words[i] = String.Format("Episode.EpisodeName LIKE '%{0}%'", words[i]);
                    }
                    // combine with ORs between each word
                    string searchItem = String.Join(" AND ", words);
                    searchItem += " ORDER BY Episode.EpisodeName";
                    parameters.Add("Parameter", searchItem);
                    parameters.Add("Fetch", mItemsPerPage.ToString());
                    items = await App.MobileService
                        .InvokeApiAsync<ObservableCollection<EpisodeQuery>>("getepisodebynamematch",
                        System.Net.Http.HttpMethod.Get, parameters);
                }
                catch
                {
                    Utilities.Utilities.Instance.MessageBoxShow("Error loading episode from mobile services");
                }
                finally
                {
                    if (items != null && items.Count > 0)
                    {
                        List.ItemsSource = items;
                        List.LayoutMode = LongListSelectorLayoutMode.List;
                        List.ItemTemplate = this.Resources["EpisodeNameItemTemplate"] as DataTemplate;
                        Header.Text = String.Format("{0} item(s) found", items.Count);
                    }
                    else
                    {
                        List.ItemsSource = null;
                        Header.Text = String.Format("0 items found");
                    }
                }
            }
            catch 
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading series name from mobile services");
            }
            finally
            {
                Utilities.Utilities.Instance.SetProgress(false, null);
            }
        }

        private async void LoadEpisodeOverview(LongListSelector list)
        {
            try
            {
                SearchItem.Text = mQuery;
                // reload searchItem item it gets wiped on postback
                ObservableCollection<EpisodeQuery> items = null;
                try
                {
                    Utilities.Utilities.Instance.SetProgress(true, "loading matching episodes...");
                    // Asynchronously call the custom API using the GET method
                    // this doesn't seem to be able to be paged
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    // split seach items into a LIKE statement
                    string[] words = mQuery.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < words.Length; i++)
                    {
                        words[i] = String.Format("Episode.Overview LIKE '%{0}%'", words[i]);
                    }
                    // combine with ORs between each word
                    string searchItem = String.Join(" AND ", words);
                    searchItem += " ORDER BY Episode.EpisodeName";
                    parameters.Add("Parameter", searchItem);
                    parameters.Add("Fetch", mItemsPerPage.ToString());
                    items = await App.MobileService
                        .InvokeApiAsync<ObservableCollection<EpisodeQuery>>("getepisodebyoverviewmatch",
                        System.Net.Http.HttpMethod.Get, parameters);
                }
                catch
                {
                    Utilities.Utilities.Instance.MessageBoxShow("Error loading episodes from mobile services");
                }
                finally
                {
                    if (items != null && items.Count > 0)
                    {
                        List.ItemsSource = items;
                        List.LayoutMode = LongListSelectorLayoutMode.List;
                        List.ItemTemplate = this.Resources["EpisodeOverviewItemTemplate"] as DataTemplate;
                        Header.Text = String.Format("{0} item(s) found", items.Count);
                    }
                    else
                    {
                        List.ItemsSource = null;
                        Header.Text = String.Format("0 items found");
                    }
                }
            }
            catch
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading episode overview from mobile services");
            }
            finally
            {
                Utilities.Utilities.Instance.SetProgress(false, null);
            }
        }

        private async void LoadPerson(LongListSelector list, string option)
        {
            try
            {
                // reload searchItem item it gets wiped on postback
                SearchItem.Text = mQuery;
                ObservableCollection<PersonQuery> items = null;
                try
                {
                    Utilities.Utilities.Instance.SetProgress(true, "loading matching person...");
                    // Asynchronously call the custom API using the GET method
                    // this doesn't seem to be able to be paged
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    // split seach items into a LIKE statement
                    string[] words = mQuery.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < words.Length; i++)
                    {
                        words[i] = String.Format("Person.Name LIKE '%{0}%'", words[i]);
                    }
                    // combine with ORs between each word
                    string searchItem = String.Join(" AND ", words);
                    // AND in extra option search
                    switch (option)
                    {
                        case "Actor":
                            searchItem += String.Format(" AND PersonInstance.CastType <= {0}", PersonInstance.GUEST_STAR); // includes main actor
                            break;
                        case "Writer":
                            searchItem += String.Format(" AND PersonInstance.CastType = {0}", PersonInstance.WRITER);
                            break;
                        case "Director":
                            searchItem += String.Format(" AND PersonInstance.CastType = {0}", PersonInstance.DIRECTOR);
                            break;
                    }
                    searchItem += " ORDER BY Person.Name";
                    parameters.Add("Parameter", searchItem);
                    parameters.Add("Fetch", mItemsPerPage.ToString());
                    items = await App.MobileService
                        .InvokeApiAsync<ObservableCollection<PersonQuery>>("getpersonmatch",
                        System.Net.Http.HttpMethod.Get, parameters);
                }
                catch
                {
                    Utilities.Utilities.Instance.MessageBoxShow("Error loading person from mobile services");
                }
                finally
                {
                    if (items != null && items.Count > 0)
                    {
                        List.ItemsSource = items;
                        List.LayoutMode = LongListSelectorLayoutMode.List;
                        List.ItemTemplate = this.Resources["PersonItemTemplate"] as DataTemplate;
                        Header.Text = String.Format("{0} item(s) found", items.Count);
                    }
                    else
                    {
                        List.ItemsSource = null;
                        Header.Text = String.Format("0 items found");
                    }
                }
            }
            catch 
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading person from mobile services");
            }
            finally
            {
                Utilities.Utilities.Instance.SetProgress(false, null);
            }
        }

  
    }
}