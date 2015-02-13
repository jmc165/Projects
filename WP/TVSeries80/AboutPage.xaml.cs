using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Reflection;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using TVSeries80.Models;
using CreateTVSeriesDatabase.Models;
using Microsoft.WindowsAzure.MobileServices;
using Utilities;

namespace TVSeries80
{
    public partial class AboutPage : PhoneApplicationPage
    {
        private const string mAuthor = "jmc165";
        private ObservableCollection<Statistics> mStatistics = new ObservableCollection<Statistics>();
        private ObservableCollection<Administration> mAdministrations = new ObservableCollection<Administration>();
        private Statistics mStatistic;
        private Administration mAdministration;
        private ApplicationBarIconButton mRefreshAppBarButton;
        private string mCount3a;
        private string mCount3b;
        private string mCount5a;
        private string mCount5b;

        public AboutPage()
        {
            this.DataContext = App.ViewModel;

            InitializeComponent();

            // code to localize the ApplicationBar
            BuildLocalizedApplicationBar();
 
            // fill in about
            var nameHelper = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
            var version = nameHelper.Version;
            var appName = nameHelper.Name;
            // TODO: extract author

            // fill in the about text
            this.AboutInfo1.Text = mAuthor;
#if DEBUG
            this.AboutInfo2.Text = version.ToString() + " (D)";
#else
            this.AboutInfo2.Text = version.ToString() + " (R)";
#endif
            this.AboutInfo3.Text = (App.ViewModel.IsTrialMode) ? "Trial" : "Full Version";

            // Chicken out from Email feedback
            EmailButton.Visibility = System.Windows.Visibility.Collapsed;

            // only show purchase button if in trial mode
            PurchaseButton.Visibility = (App.ViewModel.IsTrialMode) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                Load();
            }
        }

        private async void Load()
        {
            // get 7 rowcounts for each major table
            try
            {
                Utilities.Utilities.Instance.SetProgress(true, "loading statistics ...");
                // Asynchronously call the custom API using the GET method
                // this doesn't seem to be able to be paged
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                // get series row count using API
                parameters.Add("Parameter", "Series");
                mStatistics = await App.MobileService
                    .InvokeApiAsync<ObservableCollection<Statistics>>("getrowcount",
                    System.Net.Http.HttpMethod.Get, parameters);
            }
            catch 
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading statistics from mobile services");
            }
            finally
            {
                if (mStatistics != null && mStatistics.Count == 1)
                {
                    mStatistic = mStatistics[0];
                    if (mStatistic != null)
                    {
                        Count1.Text = mStatistic.Count.ToString();
                    }
                }
                Utilities.Utilities.Instance.SetProgress(false, null);
            }

            try
            {
                Utilities.Utilities.Instance.SetProgress(true, "loading statistics ...");
                // Asynchronously call the custom API using the GET method
                // this doesn't seem to be able to be paged
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                // get series row count using API
                parameters.Add("Parameter", "Episode");
                mStatistics = await App.MobileService
                    .InvokeApiAsync<ObservableCollection<Statistics>>("getrowcount",
                    System.Net.Http.HttpMethod.Get, parameters);
            }
            catch
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading statistics from mobile services");
            }
            finally
            {
                if (mStatistics != null && mStatistics.Count == 1)
                {
                    mStatistic = mStatistics[0];
                    if (mStatistic != null)
                    {
                        Count2.Text = mStatistic.Count.ToString();
                    }
                }
                Utilities.Utilities.Instance.SetProgress(false, null);
            }

            try
            {
                Utilities.Utilities.Instance.SetProgress(true, "loading statistics ...");
                // Asynchronously call the custom API using the GET method
                // this doesn't seem to be able to be paged
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                // get series row count using API
                parameters.Add("Parameter", "Person");
                mStatistics = await App.MobileService
                    .InvokeApiAsync<ObservableCollection<Statistics>>("getrowcount",
                    System.Net.Http.HttpMethod.Get, parameters);
            }
            catch 
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading statistics from mobile services");
            }
            finally
            {
                if (mStatistics != null && mStatistics.Count == 1)
                {
                    mStatistic = mStatistics[0];
                    if (mStatistic != null)
                    {
                        mCount3a = mStatistic.Count.ToString();
                    }
                }
                Utilities.Utilities.Instance.SetProgress(false, null);
            }

            try
            {
                Utilities.Utilities.Instance.SetProgress(true, "loading statistics ...");
                // Asynchronously call the custom API using the GET method
                // this doesn't seem to be able to be paged
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                // get series row count using API
                parameters.Add("Parameter", "PersonInstance");
                mStatistics = await App.MobileService
                    .InvokeApiAsync<ObservableCollection<Statistics>>("getrowcount",
                    System.Net.Http.HttpMethod.Get, parameters);
            }
            catch
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading statistics from mobile services");
            }
            finally
            {
                if (mStatistics != null && mStatistics.Count == 1)
                {
                    mStatistic = mStatistics[0];
                    if (mStatistic != null)
                    {
                        mCount3b = mStatistic.Count.ToString();
                        Count3.Text = string.Format("{0} / {1}", mCount3a, mCount3b); // combine to save space
                    }
                }
                Utilities.Utilities.Instance.SetProgress(false, null);
            }

            try
            {
                Utilities.Utilities.Instance.SetProgress(true, "loading statistics ...");
                // Asynchronously call the custom API using the GET method
                // this doesn't seem to be able to be paged
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                // get series row count using API
                parameters.Add("Parameter", "Banner");
                mStatistics = await App.MobileService
                    .InvokeApiAsync<ObservableCollection<Statistics>>("getrowcount",
                    System.Net.Http.HttpMethod.Get, parameters);
            }
            catch 
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading statistics from mobile services");
            }
            finally
            {
                if (mStatistics != null && mStatistics.Count == 1)
                {
                    mStatistic = mStatistics[0];
                    if (mStatistic != null)
                    {
                        Count4.Text = mStatistic.Count.ToString();
                    }
                }
                Utilities.Utilities.Instance.SetProgress(false, null);
            }

            try
            {
                Utilities.Utilities.Instance.SetProgress(true, "loading statistics ...");
                // Asynchronously call the custom API using the GET method
                // this doesn't seem to be able to be paged
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                // get series row count using API
                parameters.Add("Parameter", "Genre");
                mStatistics = await App.MobileService
                    .InvokeApiAsync<ObservableCollection<Statistics>>("getrowcount",
                    System.Net.Http.HttpMethod.Get, parameters);
            }
            catch
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading statistics from mobile services");
            }
            finally
            {
                if (mStatistics != null && mStatistics.Count == 1)
                {
                    mStatistic = mStatistics[0];
                    if (mStatistic != null)
                    {
                        mCount5a = mStatistic.Count.ToString();
                    }
                }
                Utilities.Utilities.Instance.SetProgress(false, null);
            }

            try
            {
                Utilities.Utilities.Instance.SetProgress(true, "loading statistics ...");
                // Asynchronously call the custom API using the GET method
                // this doesn't seem to be able to be paged
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                // get series row count using API
                parameters.Add("Parameter", "GenreInstance");
                mStatistics = await App.MobileService
                    .InvokeApiAsync<ObservableCollection<Statistics>>("getrowcount",
                    System.Net.Http.HttpMethod.Get, parameters);
            }
            catch
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading statistics from mobile services");
            }
            finally
            {
                if (mStatistics != null && mStatistics.Count == 1)
                {
                    mStatistic = mStatistics[0];
                    if (mStatistic != null)
                    {
                        mCount5b = mStatistic.Count.ToString();
                        Count5.Text = string.Format("{0} / {1}", mCount5a, mCount5b); // combine to save space
                    }
                }
                Utilities.Utilities.Instance.SetProgress(false, null);
            }

            // get administration record (only one row)
            try
            {
                Utilities.Utilities.Instance.SetProgress(true, "loading statistics ...");
                // Asynchronously call the custom API using the GET method
                mAdministrations = await App.MobileService
                    .InvokeApiAsync<ObservableCollection<Administration>>("getadministration",
                    System.Net.Http.HttpMethod.Get, null);
            }
            catch 
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading statistics from mobile services");
            }
            finally
            {
                if (mAdministrations != null && mAdministrations.Count == 1)
                {
                    mAdministration = mAdministrations[0];
                    if (mAdministration != null)
                    {
                        if (mAdministration.Mode == 1)
                            Admin1.Text = "Updating...";
                        else
                            Admin1.Text = "Updated";
                        Admin2.Text = mAdministration.LastUpdated.ToString(App.DATE_FORMAT);
                        Admin3.Text = mAdministration.Counter.ToString();
                    }
                }
                Utilities.Utilities.Instance.SetProgress(false, null);
            }
        }

        private void OnFeedbackHyperlinkButtonClick(object sender, RoutedEventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.Subject = "WP8 App 'TV Series Guru' Feedback";
            //emailComposeTask.Body = "message body";
            emailComposeTask.To = "jmchadbourne@hotmail.com";
            emailComposeTask.Show();
        }

        private void OnSubmitReviewHyperlinkButtonClick(object sender, RoutedEventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }

        private void OnPurchaseHyperlinkButtonClick(object sender, RoutedEventArgs e)
        {
            MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();
            marketplaceDetailTask.Show();
        }

        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new refresh button
            mRefreshAppBarButton = new ApplicationBarIconButton(new Uri("/Images/refresh.png", UriKind.Relative));
            mRefreshAppBarButton.Text = "refresh";
            mRefreshAppBarButton.IsEnabled = true;
            mRefreshAppBarButton.Click += OnRefreshClick;
            ApplicationBar.Buttons.Add(mRefreshAppBarButton);
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            Load();
        }

 
    }
}