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
using System.Data.Linq.SqlClient;
using NewsPrint80.Models;

namespace NewsPrint80
{
    public partial class ListNewspapersPage : PhoneApplicationPage
    {
        private const string mAny = "Any";
  
        public ListNewspapersPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string groupString = null;

            // if coming back do not re-process the querystring
            if (e.NavigationMode != NavigationMode.Back)
            {
                if (NavigationContext.QueryString.TryGetValue("Group", out groupString))
                {
                    string type;

                    // show progress
                    Utilities.Utilities.Instance.SetProgress(true, null);

                    // demangle the group from URL
                    Grouping grp = new Grouping(groupString);

                    // get the NewsPrintViewModel Context 
                    NewsPrintDataContext context = App.MainViewModel.NewsPrintViewModel.Context;

                    if (NavigationContext.QueryString.TryGetValue("By", out type))
                    {
                        if (type == "NewspaperName") // list by name
                        {
                            // set title to something descriptive
                            PageTitle.Text = "search results";

                            string countryName = mAny;

                            // optional country (or mAny)
                            if (NavigationContext.QueryString.TryGetValue("CountryName", out countryName))
                            {
                                if (countryName == mAny)
                                {
                                    var query = (from n in context.Newspapers
                                                 join r in context.Regions
                                                 on n.RegionID equals r.RegionID
                                                 join c in context.Countries
                                                 on r.CountryID equals c.CountryID
                                                 where n.NewspaperName.Contains(grp.NewspaperName)
                                                 select new Grouping
                                                 {
                                                     NewspaperID = n.NewspaperID,
                                                     NewspaperName = n.NewspaperName,
                                                     CountryName = c.CountryName,
                                                     RegionName = r.RegionName,
                                                     URL = n.URL,
                                                     CountryImage = (c.ImageItem != null) ? c.ImageItem.ToArray() : null
                                                 }).ToList<Grouping>();

                                    // sort items any country is specified
                                    var sortedItems = (from c in query
                                                       orderby c.CountryName
                                                       select c).ToList<Grouping>();

                                    // execute the query and bind
                                    this.List.DataContext = sortedItems;

                                    // put out a summary line of items found
                                    Summary.Text = String.Format("{0} items found.", query.Count);
                                }
                                else
                                {
                                    var query = (from n in context.Newspapers
                                                 join r in context.Regions
                                                 on n.RegionID equals r.RegionID
                                                 join c in context.Countries
                                                 on r.CountryID equals c.CountryID
                                                 where n.NewspaperName.Contains(grp.NewspaperName)
                                                 where c.CountryName.Contains(countryName)
                                                 select new Grouping
                                                 {
                                                     NewspaperID = n.NewspaperID,
                                                     NewspaperName = n.NewspaperName,
                                                     CountryName = c.CountryName,
                                                     RegionName = r.RegionName,
                                                     URL = n.URL,
                                                     CountryImage = (c.ImageItem != null) ? c.ImageItem.ToArray() : null
                                                 }).ToList<Grouping>();

                                    // no need to sort target country is specified

                                    // execute the query and bind
                                    this.List.DataContext = query;

                                    // put out a summary line of items found
                                    Summary.Text = String.Format("{0} items found.", query.Count);
                                }
                            }
                            else // country name not specified
                            {
                                var query = (from n in context.Newspapers
                                             join r in context.Regions
                                             on n.RegionID equals r.RegionID
                                             join c in context.Countries
                                             on r.CountryID equals c.CountryID
                                             where n.NewspaperName.Contains(grp.NewspaperName)
                                             select new Grouping
                                             {
                                                 NewspaperID = n.NewspaperID,
                                                 NewspaperName = n.NewspaperName,
                                                 CountryName = c.CountryName,
                                                 RegionName = r.RegionName,
                                                 URL = n.URL,
                                                 CountryImage = (c.ImageItem != null) ? c.ImageItem.ToArray() : null
                                             }).ToList<Grouping>();

                                // sort 
                                var sortedItems = (from c in query
                                                   orderby c.CountryName
                                                   select c).ToList<Grouping>();

                                // execute the query and bind
                                this.List.DataContext = sortedItems;

                                // put out a summary line of items found
                                Summary.Text = String.Format("{0} items found.", query.Count<Grouping>());
                            }

                            // progress finished
                            Utilities.Utilities.Instance.SetProgress(false, null);

                        }
                        else if (type == "RegionID")
                        {

                            // set title to something descriptive
                            PageTitle.Text = grp.RegionName;

                            var query = (from n in context.Newspapers
                                         join r in context.Regions
                                         on n.RegionID equals r.RegionID
                                         join c in context.Countries
                                         on r.CountryID equals c.CountryID
                                         where n.RegionID.Equals(grp.RegionID)
                                         select new Grouping
                                         {
                                             NewspaperID = n.NewspaperID,
                                             NewspaperName = n.NewspaperName,
                                             CountryName = grp.CountryName,
                                             RegionName = r.RegionName,
                                             URL = n.URL,
                                             CountryImage = (c.ImageItem != null) ? c.ImageItem.ToArray() : null
                                         }).ToList<Grouping>();

                            // no need to sort

                            // execute the query and bind
                            this.List.DataContext = query;

                            // put out a summary line of items found
                            Summary.Text = String.Format("{0} items found.", query.Count<Grouping>());

                            // progress finished
                            Utilities.Utilities.Instance.SetProgress(false, null);
                        }
                    }
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        // selecting a newspaper
        private void OnItemSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector list = sender as LongListSelector;
            if (list != null)
            {
                Grouping g = list.SelectedItem as Grouping;
                if (g != null)
                {
                    // navigate
                    NavigationService.Navigate(new Uri(String.Format("/DisplayNewspaperPage.xaml?Group={0}", g), UriKind.Relative));
                }
                // item remains selected, if not cleared
                list.SelectedItem = null;
            }
        }

    }
}