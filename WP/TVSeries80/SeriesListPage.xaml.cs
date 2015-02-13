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
using Utilities;

namespace TVSeries80
{

    public partial class SeriesListPage : PhoneApplicationPage
    {
        private MobileServiceCollection<Series, Series> mSeriesItems;
        private IMobileServiceTable<Series> mSeriesTable = App.MobileService.GetTable<Series>();
        private int mAlphaKeyPage = 0;
        private int mYearPage = 0;
        private int mItemsPerPage = 50;
        private string mYear;
        private string mAlphaKey;
        private int mGenreID;
        private string mCurrentMode;
        private int mOffsetKnob = 7;

        public SeriesListPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                string value;
                string name;
                // page may be called via URL
                if (NavigationContext.QueryString.TryGetValue("AlphaKey", out value))
                {
                    Header.Text = String.Format("Browse: A-Z '{0}'", value);
                    mCurrentMode = "AlphaKey";
                    mAlphaKeyPage = 0;
                    mAlphaKey = value;
                    LoadByAlphaKey(value);
                }
                else if (NavigationContext.QueryString.TryGetValue("GenreID", out value) &&
                         NavigationContext.QueryString.TryGetValue("GenreName", out name))
                {
                    Header.Text = String.Format("Browse: Genre '{0}'", name);
                    mCurrentMode = "GenreID";
                    mGenreID = int.Parse(value);
                    LoadByGenreID(mGenreID);
                }
                else if (NavigationContext.QueryString.TryGetValue("Year", out value))
                {
                    Header.Text = String.Format("Browse: Year '{0}'", value);
                    mCurrentMode = "Year";
                    mYear = value;
                    mYearPage = 0;
                    LoadByYear(mYear);
                }
             }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // clear memory
            if (mSeriesItems != null)
                mSeriesItems.Clear();

        }

        private async void LoadByAlphaKey(string alphaKey)
        {
            MobileServiceCollection<Series, Series> pageItems = null;
            try
            {
                Utilities.Utilities.Instance.SetProgress(true, "loading series list...");
                // grab a page of items at a time, the service limits the grab to 50 at a time
                pageItems = await mSeriesTable
                .Where(i => i.AlphaKey == alphaKey)
                .OrderBy(i => i.SeriesName)
                .Skip(mAlphaKeyPage * mItemsPerPage)
                .Take(mItemsPerPage)
                .ToCollectionAsync<Series>();
            }
            catch
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading series from mobile services");
            }
            finally
            {
                if (pageItems != null)
                {
                    if (mSeriesItems == null)
                        mSeriesItems = pageItems;
                    else
                    {
                        foreach (var i in pageItems)
                            mSeriesItems.Add(i);
                    }
                }
                if (mSeriesItems != null && mSeriesItems.Count > 0)
                {
                    SeriesList.ItemsSource = mSeriesItems;
                }
                else
                    Header.Text = "No matching series found";

                Utilities.Utilities.Instance.SetProgress(false, null);
            }
        }

        private async void LoadByYear(string year)
        {
            MobileServiceCollection<Series, Series> pageItems = null;
            try
            {
                Utilities.Utilities.Instance.SetProgress(true, "loading series list...");
                // grab a page of items at a time, the service limits the grab to 50 at a time
                pageItems = await mSeriesTable
                .Where(i => i.FirstAired.Contains(year))
                .OrderBy(i => i.SeriesName)
                .Skip(mYearPage * mItemsPerPage)
                .Take(mItemsPerPage)
                .ToCollectionAsync<Series>();
            }
            catch
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading series from mobile services");
            }
            finally
            {
                if (pageItems != null)
                {
                    if (mSeriesItems == null)
                        mSeriesItems = pageItems;
                    else
                    {
                        foreach (var i in pageItems)
                            mSeriesItems.Add(i);
                    }
                }
                if (mSeriesItems != null && mSeriesItems.Count > 0)
                {
                    SeriesList.ItemsSource = mSeriesItems;
                }
                else
                    Header.Text = "No matching series found";

                Utilities.Utilities.Instance.SetProgress(false, null);
            }
        }

        private async void LoadByGenreID(int genreID)
        {
            ObservableCollection<Series> seriesItems = null;
            try
            {
                Utilities.Utilities.Instance.SetProgress(true, "loading series list...");
                // Asynchronously call the custom API using the GET method
                // this doesn't seem to be able to be paged
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("GenreID", genreID.ToString());
                seriesItems = await App.MobileService
                    .InvokeApiAsync<ObservableCollection<Series>>("getseriesbygenre",
                    System.Net.Http.HttpMethod.Get, parameters);
            }
            catch
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading series from mobile services");
            }
            finally
            {
                if (seriesItems != null && seriesItems.Count > 0)
                {
                    SeriesList.ItemsSource = seriesItems;
                }
                else
                    Header.Text = "No matching series found";

                Utilities.Utilities.Instance.SetProgress(false, null);
            }
        }

        #region events

        // selecting a Series from the list
        private void OnSeriesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector list = sender as LongListSelector;
            if (list != null)
            {
                Series series = list.SelectedItem as Series;
                if (series != null)
                {
                    NavigationService.Navigate(new Uri(String.Format("/SeriesPage.xaml?SeriesID={0}", series.id), UriKind.Relative));
                }
                // item remains selected, if not cleared
                list.SelectedItem = null;
            }
        }

        // this function is used to implement infinite scrolling of a very long list
        private void OnSeriesItemRealized(object sender, ItemRealizationEventArgs e)
        {
            if (e.ItemKind == LongListSelectorItemKind.Item)
            {
                if (SeriesList.ItemsSource.Count > mOffsetKnob)
                {
                    if ((e.Container.Content as Series).Equals(SeriesList.ItemsSource[SeriesList.ItemsSource.Count - mOffsetKnob]))
                    {
                        switch (mCurrentMode)
                        {
                            case "AlphaKey":
                                mAlphaKeyPage++;
                                LoadByAlphaKey(mAlphaKey);
                                break;
                            case "Year":
                                mYearPage++;
                                LoadByYear(mYear);
                                break;
                        }
                    }
                }
            }
        }

        #endregion

    }
}