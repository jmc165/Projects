using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NewsPrint80.Models;

namespace NewsPrint80
{
    public partial class SearchPage : PhoneApplicationPage
    {
        private const string mAny = "Any";
        private string mSearchCountry = mAny;

        public SearchPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                // extract key data from IS (last selected country)
                if (App.MainViewModel.ContainsSettingsKey(App.LAST_SELECTED_SEARCH_COUNTRY_KEY))
                {
                    mSearchCountry = App.MainViewModel.LoadSettingsKey(App.LAST_SELECTED_SEARCH_COUNTRY_KEY) as string;
                    if (mSearchCountry != null)
                        // display in place holder on page
                        this.CountryName.Text = mSearchCountry != mAny ? String.Format("Search for a title by name in {0}", mSearchCountry) : "Search for a title by name in any country";
                }

                // get the NewsPrintViewModel Context 
                NewsPrintDataContext context = App.MainViewModel.NewsPrintViewModel.Context;

                // Sorting countries is expensive (try to store in page State)
                List<Grouping> sortedCountries = null;

                var query = (from c in context.Countries
                             select new Grouping
                             {
                                 CountryName = c.CountryName,
                                 CountryID = c.CountryID,
                                 CountryImage = (c.ImageItem != null) ? c.ImageItem.ToArray() : null
                             }).ToList<Grouping>();

                sortedCountries = (from c in query
                                   orderby c.CountryName
                                   select c).ToList<Grouping>();

                // create a dummy
                Grouping dummyGroup = new Grouping();
                dummyGroup.CountryID = -1;
                dummyGroup.CountryName = mAny;
                sortedCountries.Insert(0, dummyGroup);  
          
                // execute the query and bind
                this.Countries.DataContext = sortedCountries;

                // progress finished
                Utilities.Utilities.Instance.SetProgress(false, null);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // stop progress
            Utilities.Utilities.Instance.SetProgress(false, null);
        }

        private void OnSearchItemKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Grouping g = new Grouping
                {
                    NewspaperName = SearchItem.Text
                };

                // show progress
                Utilities.Utilities.Instance.SetProgress(true, null);

                // Navigate to the listing page
                NavigationService.Navigate(new Uri(String.Format("/ListNewspapersPage.xaml?By=NewspaperName&CountryName={0}&Group={1}", mSearchCountry, g), UriKind.Relative));
            }
        }

        private void OnCountrySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector list = sender as LongListSelector;
            if (list != null)
            {
                Grouping g = list.SelectedItem as Grouping;
                if (g != null)
                {
                    mSearchCountry = g.CountryName;
                    // display in place holder on page
                    this.CountryName.Text = mSearchCountry != mAny ? String.Format("Search for a title by name in {0}", mSearchCountry) : "Search for a title by name in any country";
                    // store to local data
                    App.MainViewModel.SaveSettingsKey(App.LAST_SELECTED_SEARCH_COUNTRY_KEY, mSearchCountry);
                }
                // item remains selected, if not cleared
                this.Countries.SelectedItem = null;
            }
        }

    }
}