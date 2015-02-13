using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NewsPrint80.Models;

namespace NewsPrint80
{
    public partial class BrowseRegionsPage : PhoneApplicationPage
    {
        public BrowseRegionsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string value;
            int countryID;

            if (e.NavigationMode != NavigationMode.Back)
            {
                // page may be called via URL
                if (NavigationContext.QueryString.TryGetValue("CountryID", out value))
                {
                    if (int.TryParse(value, out countryID))
                    {
                        OnNavigatedTo(countryID);
                    }
                }
            }
        }

        private void OnNavigatedTo(int countryID)
        {
            // get the NewsPrintViewModel Context 
            NewsPrintDataContext context = App.MainViewModel.NewsPrintViewModel.Context;

            // get country by name
            var query = (from c in context.Countries
                         where c.CountryID.Equals(countryID)
                         select new Grouping
                         {
                             CountryName = c.CountryName,
                         }).ToList<Grouping>().First<Grouping>();

            if (query != null)
            {
                PageTitle.Text = query.CountryName;
            }

            // form a query
            var query2 = (from r in context.Regions
                         join c in context.Countries
                         on r.CountryID equals c.CountryID
                         where c.CountryID.Equals(countryID)
                         select new Grouping
                         {
                             CountryID = c.CountryID,
                             CountryName = c.CountryName,
                             RegionID = r.RegionID,
                             // region name is of the form CountryName+RegionName
                             RegionName = r.RegionName,
                         }).ToList<Grouping>();

            // execute the query
            this.List.DataContext = query2;
        }

        // selecting an item
        private void OnItemSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector list = sender as LongListSelector;
            if (list != null)
            {
                Grouping g = list.SelectedItem as Grouping;
                if (g != null)
                {
                    NavigationService.Navigate(new Uri(String.Format("/ListNewspapersPage.xaml?By=RegionID&Group={0}", g), UriKind.Relative));
                }
                // item remains selected, if not cleared
                list.SelectedItem = null;
            }
        }

    }
}