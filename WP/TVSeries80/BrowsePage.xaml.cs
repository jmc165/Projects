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
    public static class DataTemplateSelector
    {
        public static DataTemplate GetTemplate(BrowsePage param)
        {
            Type t = param.GetType();
            return App.Current.Resources[t.Name] as DataTemplate;
        }
    } 

    public partial class BrowsePage : PhoneApplicationPage
    {
        private MobileServiceCollection<Genre, Genre> mGenreItems;
        private IMobileServiceTable<Genre> mGenreTable = App.MobileService.GetTable<Genre>();
        private ObservableCollection<string> mAtoZ = new ObservableCollection<string>();

        public BrowsePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                string value;
                // page may be called via URL
                if (NavigationContext.QueryString.TryGetValue("Option", out value))
                {
                    switch (value)
                    {
                        case "Series A-Z":
                            LoadAtoZ(List);
                            break;
                        case "Genre":
                            LoadGenres(List);
                            break;
                        case "Year":
                            LoadYears(List);
                            break;
                    }
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void LoadAtoZ(LongListSelector list)
        {
            try
            {
                List.ItemsSource = App.ViewModel.AtoZOptions;
                List.LayoutMode = LongListSelectorLayoutMode.Grid;
                List.GridCellSize = new Size(113, 113);
                List.ItemTemplate = this.Resources["AtoZItemTemplate"] as DataTemplate;
            }
            catch
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading genre from mobile services");
            }
            finally
            {
            }
        }

        private async void LoadGenres(LongListSelector list)
        {
            try
            {
               Utilities.Utilities.Instance.SetProgress(true, "loading genres...");
                mGenreItems = await mGenreTable
                .OrderBy(i => i.Name)
                .ToCollectionAsync<Genre>();
            }
            catch 
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading genre from mobile services");
            }
            finally
            {
                if (mGenreItems != null && mGenreItems.Count > 0)
                {
                    List.ItemsSource = mGenreItems;
                    List.LayoutMode = LongListSelectorLayoutMode.List;
                    List.ItemTemplate = this.Resources["GenreItemTemplate"] as DataTemplate;
                }
                Utilities.Utilities.Instance.SetProgress(false, null);
            }
        }

        private void LoadYears(LongListSelector list)
        {
            try
            {
                List.ItemsSource = App.ViewModel.YearOptions;
                List.ItemTemplate = this.Resources["YearItemTemplate"] as DataTemplate;
            }
            catch
            {
            }
            finally
            {
            }
        }

        #region events

        // selecting a Series from the list
        private void OnItemSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector list = sender as LongListSelector;
            if (list != null)
            {
                string s = list.SelectedItem as string;
                if (s != null)
                {
                    // could be a year, could be a letter
                    int year;
                    if (int.TryParse(s, out year))
                        NavigationService.Navigate(new Uri(String.Format("/SeriesListPage.xaml?Year={0}", s), UriKind.Relative));
                    else
                    {
                        if (s == "#")
                            s = "*";
                        NavigationService.Navigate(new Uri(String.Format("/SeriesListPage.xaml?AlphaKey={0}", s), UriKind.Relative));
                    }
                }
                Genre g = list.SelectedItem as Genre;
                if (g != null)
                {
                    NavigationService.Navigate(new Uri(String.Format("/SeriesListPage.xaml?GenreID={0}&GenreName={1}", g.id, g.Name), UriKind.Relative));
                }
                list.SelectedItem = null;
            }
        }

        #endregion

    }
}