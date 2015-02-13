using System;
using System.Net;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.WindowsAzure.MobileServices;
using CreateTVSeriesDatabase.Models;
using TVSeries80.Models;

namespace TVSeries80
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        private bool mLoading = false;
        ObservableCollection<News> mNews = new ObservableCollection<News>();

        public SettingsPage()
        {
            InitializeComponent();
            mLoading = true;
            LoadOptions();
            mLoading = false;
        }

        private void LoadOptions()
        {
            if (App.ViewModel.LoadSettingsKey(App.INTERNAL_BROWSER_KEY) != null)
            {
                int value = (int)App.ViewModel.LoadSettingsKey(App.INTERNAL_BROWSER_KEY);
                if (value == 1)
                {
                    InternalRB.IsChecked = true;
                    IERB.IsChecked = false;
                }
                else
                {
                    InternalRB.IsChecked = false;
                    IERB.IsChecked = true;
                }
            }

            if (App.ViewModel.LoadSettingsKey(App.SEARCH_LIMIT_KEY) != null)
            {
                int value = (int)App.ViewModel.LoadSettingsKey(App.SEARCH_LIMIT_KEY);
                switch (value)
                {
                    case 50:
                        Search50.IsChecked = true;
                        Search100.IsChecked = false;
                        Search200.IsChecked = false;
                        Search500.IsChecked = false;
                        break;
                    case 100:
                        Search50.IsChecked = false;
                        Search100.IsChecked = true;
                        Search200.IsChecked = false;
                        Search500.IsChecked = false;
                        break;
                    case 200:
                        Search50.IsChecked = false;
                        Search100.IsChecked = false;
                        Search200.IsChecked = true;
                        Search500.IsChecked = false;
                        break;
                    case 500:
                        Search50.IsChecked = false;
                        Search100.IsChecked = false;
                        Search200.IsChecked = false;
                        Search500.IsChecked = true;
                        break;
                }
            }

            if (App.ViewModel.LoadSettingsKey(App.TWITTER_SEARCH_LIMIT_KEY) != null)
            {
                int value = (int)App.ViewModel.LoadSettingsKey(App.TWITTER_SEARCH_LIMIT_KEY);
                switch (value)
                {
                    case 15:
                        TwitterSearch15.IsChecked = true;
                        TwitterSearch30.IsChecked = false;
                        TwitterSearch50.IsChecked = false;
                        break;
                    case 30:
                        TwitterSearch15.IsChecked = false;
                        TwitterSearch30.IsChecked = true;
                        TwitterSearch50.IsChecked = false;
                        break;
                    case 50:
                        TwitterSearch15.IsChecked = false;
                        TwitterSearch30.IsChecked = false;
                        TwitterSearch50.IsChecked = true;
                        break;
                }
            }

            if (App.ViewModel.LoadSettingsKey(App.TWITTER_SEARCH_TYPE_KEY) != null)
            {
                string value = (string)App.ViewModel.LoadSettingsKey(App.TWITTER_SEARCH_TYPE_KEY);
                switch (value)
                {
                    case "Mixed":
                        TwitterSearchTypeMixed.IsChecked = true;
                        TwitterSearchTypeRecent.IsChecked = false;
                        TwitterSearchTypePopular.IsChecked = false;
                        break;
                    case "Recent":
                        TwitterSearchTypeMixed.IsChecked = false;
                        TwitterSearchTypeRecent.IsChecked = true;
                        TwitterSearchTypePopular.IsChecked = false;
                        break;
                    case "Popular":
                        TwitterSearchTypeMixed.IsChecked = false;
                        TwitterSearchTypeRecent.IsChecked = false;
                        TwitterSearchTypePopular.IsChecked = true;
                        break;
                }
            }

            if (App.ViewModel.LoadSettingsKey(App.DEFAULT_NEWS_KEY) != null)
            {
                mNews = App.ViewModel.LoadSettingsKey(App.DEFAULT_NEWS_KEY) as ObservableCollection<News>;
                NewsList.ItemsSource = mNews;
            }

            if (App.ViewModel.LoadSettingsKey(App.YOUTUBE_STREAM_KEY) != null)
            {
                string value = App.ViewModel.LoadSettingsKey(App.YOUTUBE_STREAM_KEY) as string;
                if (value != null)
                {
                    switch (value)
                    {
                        case "480p":
                            s480p.IsChecked = true;
                            s720p.IsChecked = false;
                            s1080p.IsChecked = false;
                            break;
                        case "720p":
                            s480p.IsChecked = false;
                            s720p.IsChecked = true;
                            s1080p.IsChecked = false;
                            break;
                        case "1080p":
                            s480p.IsChecked = false;
                            s720p.IsChecked = false;
                            s1080p.IsChecked = true;
                            break;
                    }
                }
            }

            if (App.ViewModel.LoadSettingsKey(App.YOUTUBE_DL_KEY) != null)
            {
                string value = App.ViewModel.LoadSettingsKey(App.YOUTUBE_DL_KEY) as string;
                if (value != null)
                {
                    switch (value)
                    {
                        case "480p":
                            d480p.IsChecked = true;
                            d720p.IsChecked = false;
                            d1080p.IsChecked = false;
                            break;
                        case "720p":
                            d480p.IsChecked = false;
                            d720p.IsChecked = true;
                            d1080p.IsChecked = false;
                            break;
                        case "1080p":
                            d480p.IsChecked = false;
                            d720p.IsChecked = false;
                            d1080p.IsChecked = true;
                            break;
                    }
                }
            }

            if (App.ViewModel.LoadSettingsKey(App.WEBSITE_STYLE_KEY) != null)
            {
                string value = App.ViewModel.LoadSettingsKey(App.WEBSITE_STYLE_KEY) as string;
                if (value != null)
                {
                    switch (value)
                    {
                        case "Mobile":
                            iMobile.IsChecked = true;
                            iDesktop.IsChecked = false;
                            break;
                        case "Desktop":
                            iMobile.IsChecked = false;
                            iDesktop.IsChecked = true;
                            break;
                    }
                }
            }
        }

        private void OnBrowserOptionsChecked(object sender, RoutedEventArgs e)
        {
            if (mLoading)
                return;
            RadioButton rb = sender as RadioButton;
            if (rb == InternalRB)
                App.ViewModel.SaveSettingsKey(App.INTERNAL_BROWSER_KEY, (int)1);
            else
                App.ViewModel.SaveSettingsKey(App.INTERNAL_BROWSER_KEY, (int)0);
        }

        private void OnSearchLimitOptionsChecked(object sender, RoutedEventArgs e)
        {
            if (mLoading)
                return;
            RadioButton rb = sender as RadioButton;
            string value = rb.Content as string;
            if (value != null)
                App.ViewModel.SaveSettingsKey(App.SEARCH_LIMIT_KEY, int.Parse(value));
        }

        private void OnNewsEnableClick(object sender, RoutedEventArgs e)
        {
            if (mLoading)
                return;
            CheckBox cb = sender as CheckBox;
            if (cb != null)
            {
                // get parent item using 
                News f = cb.DataContext as News;
                if (f != null)
                {
                    if (f.NewsName != "Top Stories")
                    {
                        f.Enabled = (bool)cb.IsChecked;
                        if (mNews != null)
                        {
                            App.ViewModel.SaveSettingsKey(App.DEFAULT_NEWS_KEY, mNews);
                            // regenerate the display feeds
                            App.ViewModel.GenerateDisplayNews();
                        }
                    }
                    else
                    {
                        mLoading = true;
                        cb.IsChecked = true;
                        mLoading = false;  
                    }
                }
            }
        }

        private void OnYouTubeStreamOptionsChecked(object sender, RoutedEventArgs e)
        {
            if (mLoading)
                return;
            RadioButton rb = sender as RadioButton;
            string value = rb.Content as string;
            if (value != null)
                App.ViewModel.SaveSettingsKey(App.YOUTUBE_STREAM_KEY, value);
        }

        private void OnYouTubeDLOptionsChecked(object sender, RoutedEventArgs e)
        {
            if (mLoading)
                return;
            RadioButton rb = sender as RadioButton;
            string value = rb.Content as string;
            if (value != null)
                App.ViewModel.SaveSettingsKey(App.YOUTUBE_DL_KEY, value);
        }

        private void OnWebsiteStyleChecked(object sender, RoutedEventArgs e)
        {
            if (mLoading)
                return;
            RadioButton rb = sender as RadioButton;
            string value = rb.Content as string;
            if (value != null)
                App.ViewModel.SaveSettingsKey(App.WEBSITE_STYLE_KEY, value);
        }

        private void OnTwitterSearchLimitOptionsChecked(object sender, RoutedEventArgs e)
        {
            if (mLoading)
                return;
            RadioButton rb = sender as RadioButton;
            string value = rb.Content as string;
            if (value != null)
                App.ViewModel.SaveSettingsKey(App.TWITTER_SEARCH_LIMIT_KEY, int.Parse(value));
        }

        private void OnTwitterSearchTypeOptionsChecked(object sender, RoutedEventArgs e)
        {
            if (mLoading)
                return;
            RadioButton rb = sender as RadioButton;
            string value = rb.Content as string;
            if (value != null)
                App.ViewModel.SaveSettingsKey(App.TWITTER_SEARCH_TYPE_KEY, value);

        }
    }
}