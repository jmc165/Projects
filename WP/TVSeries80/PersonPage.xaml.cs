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
using Utilities;

namespace TVSeries80
{
    public partial class PersonPage : PhoneApplicationPage
    {
        private MobileServiceCollection<Person, Person> mPersonItems;
        private IMobileServiceTable<Person> mPersonTable = App.MobileService.GetTable<Person>();
        private ObservableCollection<PersonQuery> mPersonResults = new ObservableCollection<PersonQuery>();
        private int mPersonID = -1;
        private Person mPerson;
        private bool mProcessSelectedItem = true;

        public PersonPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                string value;
                if (NavigationContext.QueryString.TryGetValue("PersonID", out value))
                {
                    if (int.TryParse(value, out mPersonID))
                    {
                        Load(mPersonID);
                    }
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private async void Load(int PersonID)
        {
            Utilities.Utilities.Instance.SetProgress(true, "loading person details...");

            // pivot page main
            try
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("PersonID", PersonID.ToString());
                var results = await App.MobileService
                    .InvokeApiAsync<List<PersonQuery>>("getpersoninstancebyperson",
                    System.Net.Http.HttpMethod.Get, parameters);
                foreach (var r in results)
                    mPersonResults.Add(r);
            }
            catch 
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading persons from mobile services");
            }
            finally
            {
                if (mPersonResults != null)
                {
                    PersonList.ItemsSource = mPersonResults;             
                }
            }

            try
            {
                mPersonItems = await mPersonTable
               .Where(i => i.id == PersonID)
               .ToCollectionAsync<Person>();
            }
            catch
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading Persons from mobile services");
            }
            finally
            {
                if (mPersonItems != null && mPersonItems.Count == 1)
                {
                    mPerson = mPersonItems[0];
                    if (mPerson != null)
                    {
                        // rebind
                        PivotControl.Title = mPerson.Name;
                    }
                }
            }

            // links pivot
            LinksList.ItemsSource = App.ViewModel.PersonLinkOptions;

            Utilities.Utilities.Instance.SetProgress(false, null);
        }

        #region events

        private void OnLinksOptionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector list = sender as LongListSelector;
            if (list != null)
            {
                string s = list.SelectedItem as String;
                if (s != null)
                {
                    string query = string.Format("{0}", mPerson.Name);
                    query = query.Replace(" ", "+");
                    NavigationService.Navigate(new Uri(String.Format("/LinksPage.xaml?ID={0}&q={1}&Type=Person", s, query), UriKind.Relative));
                    list.SelectedItem = null; 
                }
            }
        }

        private void OnDetailsOptionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mProcessSelectedItem)
            {
                LongListSelector list = sender as LongListSelector;
                if (list != null)
                {
                    PersonQuery cast = list.SelectedItem as PersonQuery;
                    if (cast != null)
                    {
                        switch (cast.CastType)
                        {
                            case PersonInstance.MAIN_ACTOR:
                                NavigationService.Navigate(new Uri(String.Format("/SeriesPage.xaml?SeriesID={0}", cast.SeriesID), UriKind.Relative));
                                break;
                            case PersonInstance.GUEST_STAR:
                            case PersonInstance.WRITER:
                            case PersonInstance.DIRECTOR:
                                NavigationService.Navigate(new Uri(String.Format("/EpisodePage.xaml?EpisodeID={0}&Traverse=0", cast.EpisodeID), UriKind.Relative));
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

        private void OnPersonImageTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // we are already displaying this page don#t display it again
            mProcessSelectedItem = false;
        }

        #endregion


    
    }
}