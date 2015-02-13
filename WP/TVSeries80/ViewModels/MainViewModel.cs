using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.MobileServices;
using System.Windows;
using CreateTVSeriesDatabase.Models;
using TVSeries80.Models;
using TVSeries80.Resources;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Net;
using System.Linq;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.IO;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Threading.Tasks;
using Windows.Storage;
using LinqToTwitter;
using Utilities;

namespace TVSeries80.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Series> mFavorites = new ObservableCollection<Series>();
        private ObservableCollection<Series> mHistory = new ObservableCollection<Series>();
        private ObservableCollection<News> mDisplayNews = new ObservableCollection<News>();
        private ObservableCollection<News> mDefaultNews = new ObservableCollection<News>();
        private ObservableCollection<Link> mNewsItems = new ObservableCollection<Link>();
        private ObservableCollection<Link> mDownloadItems = new ObservableCollection<Link>();
        private ObservableCollection<String> mYears = new ObservableCollection<String>();
        private ObservableCollection<string> mOptionItems = new ObservableCollection<string>()
        {
            "Series A-Z",
            "Genre",
            "Year",
        };
        private ObservableCollection<string> mSearchOptions = new ObservableCollection<string>()
        {
            "Series Name",
            "Series Overview",
            "Episode Name",
            "Episode Overview",
            "Any Person",
            "Actor",
            "Writer",
            "Director",
        };
        private ObservableCollection<string> mAtoZOptions = new ObservableCollection<string>()
        {
            "#", "a", "b", "c", "d", "e", "f", "g", "h", "i", 
            "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", 
            "t", "u", "v", "w", "x", "y", "z",
        };
        private ObservableCollection<string> mSeriesLinkOptions = new ObservableCollection<string>()
        {
            "YouTube",
            "IMDB",
            "Twitter",
            "Theme",
            "News",
        };
        private ObservableCollection<string> mEpisodeLinkOptions = new ObservableCollection<string>()
        {
            "YouTube",
            "IMDB",
        };
        private ObservableCollection<string> mPersonLinkOptions = new ObservableCollection<string>()
        {
            "YouTube",
            "IMDB",
            "Twitter",
            "News",
        };
        private ObservableCollection<News> mDefaultNewsFeeds = new ObservableCollection<News>()
        { 
            new News { NewsName="Top Stories", URI=null, Images=0, Enabled=true },   // special
            new News { NewsName="Hollywood Reporter", URI="http://www.hollywoodreporter.com/taxonomy/term/60/0/feed", Images=1, Enabled=true },
            new News { NewsName="Hitfix", URI="http://www.hitfix.com/channels/tv.rss", Images=1, Enabled=true },
            new News { NewsName="Warming Glow", URI="http://www.uproxx.com/tv/feed", Images=1, Enabled=true },
            new News { NewsName="Digital Spy", URI="http://www.digitalspy.co.uk/rss/categories/gb/tv.xml", Images=0, Enabled=true },
            new News { NewsName="Buzzfeed", URI="http://www.buzzfeed.com/tag/tv.xml", Images=1, Enabled=true },   
            new News { NewsName="Flavor Wire", URI="http://flavorwire.com/feed/", Images=0, Enabled=true },   
            new News { NewsName="Zap2it", URI="http://feeds.feedburner.com/zap2it/frominsidethebox", Images=1, Enabled=true },        
            new News { NewsName="Screen Crush", URI="http://screencrush.com/feed/", Images=0, Enabled=true },        
            new News { NewsName="TV Biz Wire", URI="http://feeds2.feedburner.com/tvweek/tvbizwire", Images=0, Enabled=true },        
            new News { NewsName="TV Guide", URI="http://rss.tvguide.com/breakingnews", Images=1, Enabled=true },  
            new News { NewsName="Daily Mirror(UK)", URI="http://www.mirror.co.uk/tv/tv-news/rss.xml", Images=1, Enabled=true },        
            new News { NewsName="Huffington Post(UK)", URI="http://www.huffingtonpost.com/feeds/verticals/uk-entertainment/index.xml", Images=1, Enabled=true }, 
            new News { NewsName="Guardian(UK)", URI="http://www.theguardian.com/tv-and-radio/rss", Images=1, Enabled=true }, 
        };

        public MainViewModel()
        {
        }

        // storage properties
        public Page CurrentPage { get; set; }

        // storage properties
        public string CurrentQuery { get; set; }

        // storage properties
        public List<int> BannerList { get; set; }

        // storage properties
        public List<int> EpisodeList { get; set; }

        // storage properties
        public SingleUserAuthorizer CurrentTwitterAuthorisation { get; set; }
    
        // available for public binding
        public ObservableCollection<Series> FavoriteItems
        {
            get
            {
                return mFavorites;
            }
            set
            {
                mFavorites = value;
                NotifyPropertyChanged("Favorites");
            }
        }

        // available for public binding
        public ObservableCollection<Series> HistoryItems
        {
            get
            {
                return mHistory;
            }
            set
            {
                mHistory = value;
                NotifyPropertyChanged("History");
            }
        }

        // available for public binding
        public ObservableCollection<News> DisplayNews
        {
            get
            {
                return mDisplayNews;
            }
            set
            {
                mDisplayNews = value;
                NotifyPropertyChanged("DisplayNews");
            }
        }        
        
        // available for public binding
        public ObservableCollection<News> DefaultNews
        {
            get
            {
                return mDefaultNews;
            }
            set
            {
                mDisplayNews = value;
                NotifyPropertyChanged("DefaultNews");
            }
        }

        // available for public binding
        public ObservableCollection<Link> DownloadItems
        {
            get
            {
                return mDownloadItems;
            }
            set
            {
                mDownloadItems = value;
                NotifyPropertyChanged("Downloads");
            }
        }

        public SingleUserAuthorizer GetTwitterAuthorisation()
        {
            if (CurrentTwitterAuthorisation != null)
            {
                return CurrentTwitterAuthorisation;
            }
            else
            {
                CurrentTwitterAuthorisation = new SingleUserAuthorizer
                {
                    Credentials = new SingleUserInMemoryCredentials
                    {
                        ConsumerKey = App.TWITTER_CONSUMER_KEY,
                        ConsumerSecret = App.TWITTER_CONSUMER_SECRET_KEY,
                        TwitterAccessToken = App.TWITTER_ACCESS_TOKEN_KEY,
                        TwitterAccessTokenSecret = App.TWITTER_ACCESS_TOKEN_SECRET_KEY,
                    }
                };

                return CurrentTwitterAuthorisation;
            }
        }

        // called when app launching (good place to initialise default settings)
        public void InitialiseDefaultSettings()
        {

            if (App.ViewModel.LoadSettingsKey(App.FAVORITES_KEY) == null)
                App.ViewModel.SaveSettingsKey(App.FAVORITES_KEY, mFavorites);

            if (App.ViewModel.LoadSettingsKey(App.HISTORY_KEY) == null)
                App.ViewModel.SaveSettingsKey(App.HISTORY_KEY, mHistory);    

            if (App.ViewModel.LoadSettingsKey(App.INTERNAL_BROWSER_KEY) == null)
                App.ViewModel.SaveSettingsKey(App.INTERNAL_BROWSER_KEY, 1);

            if (App.ViewModel.LoadSettingsKey(App.SEARCH_LIMIT_KEY) == null)
                App.ViewModel.SaveSettingsKey(App.SEARCH_LIMIT_KEY, 50);

            // default news feeds
            if (App.ViewModel.LoadSettingsKey(App.DEFAULT_NEWS_KEY) == null)
            {
                App.ViewModel.SaveSettingsKey(App.DEFAULT_NEWS_KEY, mDefaultNewsFeeds);
                GenerateDisplayNews();
            }
            else
            {
                // if the number of default feeds has changed make sure the settings keep in sync
                mDefaultNews = App.ViewModel.LoadSettingsKey(App.DEFAULT_NEWS_KEY) as ObservableCollection<News>;
                if (mDefaultNews == null || mDefaultNews.Count != mDefaultNewsFeeds.Count)
                {
                    mDefaultNews = new ObservableCollection<News>();
                    foreach (News f in mDefaultNewsFeeds)
                        mDefaultNews.Add(f);
                    // re-save the default feeds
                    App.ViewModel.SaveSettingsKey(App.DEFAULT_NEWS_KEY, mDefaultNews);
                    GenerateDisplayNews();
                }
            }

            if (App.ViewModel.LoadSettingsKey(App.DISPLAY_NEWS_KEY) == null)
                GenerateDisplayNews();

            if (App.ViewModel.LoadSettingsKey(App.YOUTUBE_STREAM_KEY) == null)
                App.ViewModel.SaveSettingsKey(App.YOUTUBE_STREAM_KEY, "480p");

            if (App.ViewModel.LoadSettingsKey(App.YOUTUBE_DL_KEY) == null)
                App.ViewModel.SaveSettingsKey(App.YOUTUBE_DL_KEY, "480p");

            if (App.ViewModel.LoadSettingsKey(App.WEBSITE_STYLE_KEY) == null)
                App.ViewModel.SaveSettingsKey(App.WEBSITE_STYLE_KEY, "Mobile");

            if (App.ViewModel.LoadSettingsKey(App.TWITTER_SEARCH_LIMIT_KEY) == null)
                App.ViewModel.SaveSettingsKey(App.TWITTER_SEARCH_LIMIT_KEY, 15);

            if (App.ViewModel.LoadSettingsKey(App.TWITTER_SEARCH_TYPE_KEY) == null)
                App.ViewModel.SaveSettingsKey(App.TWITTER_SEARCH_TYPE_KEY, "Recent");

         }

        public void GenerateDisplayNews()
        {
            mDisplayNews.Clear();
            // reload the default feeds (assume they have been edited)
            mDefaultNews = App.ViewModel.LoadSettingsKey(App.DEFAULT_NEWS_KEY) as ObservableCollection<News>;
            foreach (News f in mDefaultNews)
            {
                if (f.Enabled)
                {
                    News nf = new News();
                    nf.Images = f.Images;
                    nf.URI = f.URI;
                    nf.NewsName = f.NewsName;
                    nf.Enabled = true;
                    mDisplayNews.Add(nf);
                }
            }
            NotifyPropertyChanged("DisplayNews");
            App.ViewModel.SaveSettingsKey(App.DISPLAY_NEWS_KEY, mDisplayNews);
        }

        public void SaveSettingsKey(string key, object value)
        {
            // add value to isolated storage
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(key))
                IsolatedStorageSettings.ApplicationSettings.Add(key, value);
            else
                IsolatedStorageSettings.ApplicationSettings[key] = value;
             // must save
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public bool ContainsSettingsKey(string key)
        {
            // add value to isolated storage
            return IsolatedStorageSettings.ApplicationSettings.Contains(key);
        }

        public object LoadSettingsKey(string key)
        {
            // add value to isolated storage
            if (IsolatedStorageSettings.ApplicationSettings.Contains(key))
                return IsolatedStorageSettings.ApplicationSettings[key];
            else
                return null;
        }

        public void RemoveKey(string key, object value)
        {
            // add value to isolated storage
            if (IsolatedStorageSettings.ApplicationSettings.Contains(key))
            {
                IsolatedStorageSettings.ApplicationSettings.Remove(key);
                // must save
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

        #region feed processing

        public void LoadDisplayNews()
        {
            mDisplayNews = App.ViewModel.LoadSettingsKey(App.DISPLAY_NEWS_KEY) as ObservableCollection<News>;
        }

        #endregion

        #region favorite processing

        // save Favorite to isolated storage/observable items 
        public bool AddFavorite(Series series)
        {
            bool added = false;
             // save to isolated storage
            FavoriteItems = App.ViewModel.LoadSettingsKey(App.FAVORITES_KEY) as ObservableCollection<Series>;
            if (FavoriteItems == null)
            {
                FavoriteItems = new ObservableCollection<Series>();
                FavoriteItems.Add(series);
                NotifyPropertyChanged("Favorites");
                added = true;
            }
            else
            {
                var exists =
                   from f in FavoriteItems
                   where f.id == series.id
                   select f; 
                if (exists.Count<Series>() == 0)
                {
                    FavoriteItems.Add(series);
                    NotifyPropertyChanged("Favorites");
                    added = true;
                }
            }
            App.ViewModel.SaveSettingsKey(App.FAVORITES_KEY, FavoriteItems);
            return added;
        }

        public void LoadFavorites()
        {
            FavoriteItems = App.ViewModel.LoadSettingsKey(App.FAVORITES_KEY) as ObservableCollection<Series>;
        }

        public bool RemoveFavorite(Series series)
        {
            bool removed = false;
            // save to isolated storage
            FavoriteItems = App.ViewModel.LoadSettingsKey(App.FAVORITES_KEY) as ObservableCollection<Series>;
            if (FavoriteItems != null)
            {
                foreach (Series s in FavoriteItems)
                {
                    if (s.id == series.id)
                    {
                        FavoriteItems.Remove(s);
                        NotifyPropertyChanged("Favorites");
                        App.ViewModel.SaveSettingsKey(App.FAVORITES_KEY, FavoriteItems);
                        removed = true;
                        break;
                    }
                }
            }
            return removed;
        }

        #endregion

        #region history processing

        // save History to isolated storage/observable items 
        public bool AddHistory(Series series)
        {
            bool added = false;
            // save to isolated storage
            HistoryItems = App.ViewModel.LoadSettingsKey(App.HISTORY_KEY) as ObservableCollection<Series>;
            if (HistoryItems == null)
            {
                HistoryItems = new ObservableCollection<Series>();
                HistoryItems.Add(series);
                NotifyPropertyChanged("History");
                added = true;
            }
            else
            {
                var exists =
                   from f in HistoryItems
                   where f.id == series.id
                   select f;
                if (exists.Count<Series>() == 0)
                {
                    // insert to head
                    HistoryItems.Insert(0, series);
                    // but remove the last
                    if (HistoryItems.Count > App.MAX_HISTORY)
                    {
                        HistoryItems.RemoveAt(HistoryItems.Count - 1);
                    }
                    NotifyPropertyChanged("History");
                    added = true;
                }
                else
                {
                    Series s = exists.First<Series>();
                    if (s != null)
                    {
                        // remove
                        HistoryItems.Remove(s);
                        // insert to head
                        HistoryItems.Insert(0, s);
                        NotifyPropertyChanged("History");
                        added = true;
                    }
                }
            }
            App.ViewModel.SaveSettingsKey(App.HISTORY_KEY, HistoryItems);
            return added;
        }

        public void LoadHistory()
        {
            HistoryItems = App.ViewModel.LoadSettingsKey(App.HISTORY_KEY) as ObservableCollection<Series>;
        }

        public bool RemoveHistory(Series series)
        {
            bool removed = false;
            // save to isolated storage
            HistoryItems = App.ViewModel.LoadSettingsKey(App.HISTORY_KEY) as ObservableCollection<Series>;
            if (HistoryItems != null)
            {
                foreach (Series s in HistoryItems)
                {
                    if (s.id == series.id)
                    {
                        HistoryItems.Remove(s);
                        NotifyPropertyChanged("History");
                        App.ViewModel.SaveSettingsKey(App.HISTORY_KEY, HistoryItems);
                        removed = true;
                        break;
                    }
                }
            }
            return removed;
        }

  
        #endregion

        #region downloads processing

        public async void LoadDownloads()
        {
            mDownloadItems.Clear();
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            var filesInFolder = await localFolder.GetFilesAsync();
            foreach (StorageFile currentfile in filesInFolder)
            {
                if (currentfile.Name.Contains(".mp4"))
                {
                    var size = await Utilities.Utilities.Instance.GetFileSizeAsync(currentfile);
                    Link link = new Link
                    {
                        Name = currentfile.Name.Replace(".mp4", ""),
                        URI = currentfile.Path,
                        Information = String.Format("{0}MB", Utilities.Utilities.Instance.FormatBytes(size)),
                    };
                    DownloadItems.Add(link);
                }
            }
        }

        public async void RemoveDownload(Link link)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            var filesInFolder = await localFolder.GetFilesAsync();
            foreach (StorageFile currentfile in filesInFolder)
            {
                if (currentfile.Name == link.Name + ".mp4")
                {
                    await currentfile.DeleteAsync();
                    break;
                }
            }
            if (DownloadItems != null)
            {
                DownloadItems.Remove(link);
                NotifyPropertyChanged("Downloads");
            }
        }

        #endregion

        #region web launchers

        // Launch the URI in Explorer
        public async void ExplorerLaunch(Uri uri)
        {
            try
            {
                // Launch the URI
                var success = await Windows.System.Launcher.LaunchUriAsync(uri);

                if (success)
                {
                    // URI launched OK
                }
                else
                {
                    // URI failed
                }
            }
            catch 
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error launching to " + uri.ToString());
            }
        }

        #endregion

        #region public properties to bind against

        public ObservableCollection<string> BrowseOptions { get { return mOptionItems; } }
        public ObservableCollection<string> SearchOptions { get { return mSearchOptions; } }
        public ObservableCollection<string> AtoZOptions { get { return mAtoZOptions; } }
        public ObservableCollection<string> YearOptions 
        {
            get
            {
                mYears.Clear();
                int thisYear = DateTime.Now.Year;
                int firstYear = 1930;
                for (int year = thisYear; year >= firstYear; year--)
                {
                    mYears.Add(year.ToString());
                }
                return mYears;
            }
        }
        public ObservableCollection<string> SeriesLinkOptions { get { return mSeriesLinkOptions; } }
        public ObservableCollection<string> EpisodeLinkOptions { get { return mEpisodeLinkOptions; } }
        public ObservableCollection<string> PersonLinkOptions { get { return mPersonLinkOptions; } }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool IsTrialMode
        {
            get;
            set;
        }

        private void CheckTrialStatus()
        {
/*
#if DEBUG
            IsTrialMode = false;    // change to true to test true Trial mode
#else
            try
            {
                IsTrialMode = Windows.ApplicationModel.Store.CurrentApp.LicenseInformation.IsTrial;
            }
            catch
            {
                IsTrialMode = true;
            }
#endif
 * */
            // Trial mode disabled.
            IsTrialMode = false;
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        public void Launch(LaunchingEventArgs e)
        {
            GetTwitterAuthorisation();
            InitialiseDefaultSettings();
            CheckTrialStatus();
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        public void Activated(ActivatedEventArgs e)
        {
            if (e.IsApplicationInstancePreserved)
            {
                GetTwitterAuthorisation();
                CheckTrialStatus();
            }
            else
            {
                // TOMBSTONED
                GetTwitterAuthorisation();
                CheckTrialStatus();
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        public void Deactivated(DeactivatedEventArgs e)
        {
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        public void Closing(ClosingEventArgs e)
        {
        }
    }
}