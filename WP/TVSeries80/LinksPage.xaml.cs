using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MyToolkit.Multimedia;
using TVSeries80.Models;
using CreateTVSeriesDatabase.Models;
using System.ServiceModel.Syndication;
using System.Xml;
using System.IO;
using Newtonsoft.Json;
using HtmlAgilityPack;
using Microsoft.Phone.Tasks;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.Xna.Framework.Media;
using System.Threading.Tasks;
using Windows.Storage;
using System.Threading;
using LinqToTwitter;
using Utilities;

namespace TVSeries80
{

    public class RequestState
    {
        // This class stores the State of the request.
        private const int BUFFER_SIZE = 1024;
        public StringBuilder requestData;
        public byte[] BufferRead;
        public HttpWebRequest request;
        public HttpWebResponse response;
        public Stream streamResponse;

        public RequestState()
        {
            BufferRead = new byte[BUFFER_SIZE];
            requestData = new StringBuilder("");
            request = null;
            streamResponse = null;
        }
    }

    public partial class LinksPage : PhoneApplicationPage
    {
        private string mID = null;  // YouTube etc
        private string mQuery = null;
        private string mType = null;
        private ObservableCollection<Link> mLinkItems = new ObservableCollection<Link>();
        private ObservableCollection<Link> mNewsItems = new ObservableCollection<Link>();
        private int mWatchLinksPage = 0;
        private int mWatchLinksItemsPerPage = 25;
        private int mOffsetKnob = 7;
        private bool mYouTubePlaying = false;
        private bool mYouTubeDownloading = false;
        private bool mAbortDownloading = false;
        private CustomWebClient mWebClient = null;
        private HttpWebRequest mWebRequest = null;
        private ManualResetEvent mAllDone = new ManualResetEvent(false);
        private const int BUFFER_SIZE = 1024;
        private int mTwitterItemsPerPage = 15;
        private ResultType mTwitterSearchType = ResultType.Mixed;
        private SingleUserAuthorizer mSingleUserAuthoriser = null;

        public LinksPage()
        {
            InitializeComponent();

            // get the seach limit from settings
            if (App.ViewModel.LoadSettingsKey(App.TWITTER_SEARCH_LIMIT_KEY) != null)
            {
                mTwitterItemsPerPage = (int)App.ViewModel.LoadSettingsKey(App.TWITTER_SEARCH_LIMIT_KEY);
            }

            // get the search type from settings
            if (App.ViewModel.LoadSettingsKey(App.TWITTER_SEARCH_TYPE_KEY) != null)
            {
                // extract resultType
                Enum.TryParse<ResultType>((string)App.ViewModel.LoadSettingsKey(App.TWITTER_SEARCH_TYPE_KEY), out mTwitterSearchType);
            }

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (mYouTubePlaying)
            {
                YouTube.CancelPlay();
            }
   
            if (e.NavigationMode != NavigationMode.Back)
            {
                // page may be called via URL (series ID == TVDB_ID)
                if (NavigationContext.QueryString.TryGetValue("ID", out mID) &&
                    NavigationContext.QueryString.TryGetValue("q", out mQuery) &&
                    NavigationContext.QueryString.TryGetValue("Type", out mType))
                {
                    Header.Text = String.Format("0 links found");
                    switch (mID)
                    {
                        case "YouTube":
                            LoadYouTubeJson(mQuery);
                            break;

                        case "IMDB":
                            LoadIMDB(mQuery, mType);
                            break;

                        case "Twitter":
                            LoadTwitter(mQuery, mType);
                            break;

                        case "Theme":
                            LoadTheme(mQuery);
                            break;

                        case "News":
                            mNewsItems.Clear();
                            // load newsfeeds from current settings
                            ObservableCollection<News> feeds = App.ViewModel.LoadSettingsKey(App.DEFAULT_NEWS_KEY) as ObservableCollection<News>;
                            if (feeds != null)
                            {
                                string[] words = mQuery.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (News feed in feeds)
                                {
                                    if (feed.Enabled)
                                    {
                                        LoadNews(feed, words);
                                    }
                                }
                            }
                            break;
                    }
                    PageName.Text = mID.ToLower();
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (mYouTubePlaying)
            {
                YouTube.CancelPlay();
            }

            if (mWebClient != null)
                mWebClient.CancelOperation();

            if (mWebRequest != null && mYouTubeDownloading)
            {
                mAbortDownloading = true;
            }

            //Utilities.Utilities.Instance.SetProgress(false, null);
   
        }

        public async void LoadIMDBSearchItems(string URI, string type)
        {
            mLinkItems.Clear();
            string response = null;
            bool cancelled = false;

            try
            {
                Utilities.Utilities.Instance.SetProgress(true, "loading links...");
                mWebClient = new CustomWebClient(URI);
                response = await mWebClient.DownloadStringAsync();
            }
            catch (TaskCanceledException)
            {
                cancelled = true;
            }
            catch
            {
                cancelled = true;
                Utilities.Utilities.Instance.MessageBoxShow("Error searching for links");
            }
            finally
            {
                if (response != null && response != "" && !cancelled)
                {
                    if (type == "Person")
                    {
                        PersonRootobject obj = JsonConvert.DeserializeObject<PersonRootobject>(response);
                        if (obj != null && obj.name_popular != null)
                        {
                            foreach (var i in obj.name_popular)
                            {
                                Link li = new Link();
                                li.MatchType = "Popular Match:";
                                li.Name = i.name;
                                li.Description = i.description;
                                li.URI = String.Format(App.URI_IMDB_NAME, i.id);
                                mLinkItems.Add(li);
                            }
                        }
                        if (obj != null && obj.name_exact != null)
                        {
                            foreach (var i in obj.name_exact)
                            {
                                Link li = new Link();
                                li.MatchType = "Exact Match:";
                                li.Name = i.name;
                                li.Description = i.description;
                                li.URI = String.Format(App.URI_IMDB_NAME, i.id);
                                mLinkItems.Add(li);
                            }
                        }
                        if (obj != null && obj.name_approx != null)
                        {
                            foreach (var i in obj.name_approx)
                            {
                                Link li = new Link();
                                li.MatchType = "Approx Match:";
                                li.Name = i.name;
                                li.Description = i.description;
                                li.URI = String.Format(App.URI_IMDB_NAME, i.id);
                                mLinkItems.Add(li);
                            }
                        }
                    }
                    else
                    {
                        // Series or Episode
                        TitleRootobject obj = JsonConvert.DeserializeObject<TitleRootobject>(response);
                        if (obj != null && obj.title_popular != null)
                        {
                            foreach (var i in obj.title_popular)
                            {
                                Link li = new Link();
                                li.MatchType = "Popular Match:";
                                li.Name = i.title;
                                li.URI = String.Format(App.URI_IMDB_TITLE, i.id);
                                mLinkItems.Add(li);
                            }
                        }
                        if (obj != null && obj.title_exact != null)
                        {
                            foreach (var i in obj.title_exact)
                            {
                                Link li = new Link();
                                li.MatchType = "Exact Match:";
                                li.Name = i.title;
                                li.URI = String.Format(App.URI_IMDB_TITLE, i.id);
                                mLinkItems.Add(li);
                            }
                        }
                        if (obj != null && obj.title_approx != null)
                        {
                            foreach (var i in obj.title_approx)
                            {
                                Link li = new Link();
                                li.MatchType = "Approx Match:";
                                li.Name = i.title;
                                li.URI = String.Format(App.URI_IMDB_TITLE, i.id);
                                mLinkItems.Add(li);
                            }
                        }
                    }
                }
                Header.Text = String.Format("{0} links found", mLinkItems.Count);
                LinksList.ItemsSource = mLinkItems;
                LinksList.ItemTemplate = this.Resources["IMDBTemplate"] as DataTemplate;
                Utilities.Utilities.Instance.SetProgress(false, null);
            }
        }

        private void LoadIMDB(string query, string type)
        {
            switch (type)
            {
                case "Person":
                    LoadIMDBSearchItems(string.Format(App.URI_IMDB_JSON_NAME, query), type);
                    break;
                case "Episode":
                    LoadIMDBSearchItems(string.Format(App.URI_IMDB_JSON_TITLE, query), type);
                    break;
                case "Series":
                    LoadIMDBSearchItems(string.Format(App.URI_IMDB_JSON_TITLE, query), type);
                    break;
            }
        }

        private void LoadTwitterSearchItems(string query, string type)
        {

            try
            {
                Utilities.Utilities.Instance.SetProgress(true, "loading links...");

                // store current page to View Model
                App.ViewModel.CurrentPage = this;

                mLinkItems.Clear();

                // get authorisation, either from state or twitter
                mSingleUserAuthoriser = App.ViewModel.GetTwitterAuthorisation();

                query = string.Format("#{0} OR {1}", query.Replace(" ", ""), query);

                if (mSingleUserAuthoriser != null)
                {
                    var twitterCtx = new TwitterContext(mSingleUserAuthoriser);
                    twitterCtx.UserAgent = App.TWITTER_USER_AGENT_KEY;
   
                    (from search in twitterCtx.Search
                     where search.Count == mTwitterItemsPerPage &&
                     search.ResultType == mTwitterSearchType &&
                     search.Type == SearchType.Search &&
                     search.Query == query
                     select search).AsyncCallback(searches =>
                     {
                         if (searches != null)
                         {
                             Utilities.Utilities.Instance.DispatchInvoke(() =>
                             {
                                 var srch = searches.SingleOrDefault();
                                 foreach (var status in srch.Statuses)
                                 {
                                     Link li = new Link();
                                     li.Image = status.User.ProfileImageUrl;
                                     li.Name = string.Format("{0} @{1}", status.User.Name, status.User.Identifier.ScreenName);
                                     li.Description = status.Text;
                                     li.Date = status.CreatedAt;
                                     li.MediaImage = "";
                                     foreach (var u in status.Entities.MediaEntities)
                                     {
                                         if (u.Type == "photo")
                                         {
                                             li.MediaImage = u.MediaUrl;
                                             break;
                                         }
                                     }
                                     mLinkItems.Add(li);
                                 }
                             });
                         }

                         Utilities.Utilities.Instance.DispatchInvoke(() =>
                         {
                             Header.Text = String.Format("{0} links found ({1})", mLinkItems.Count, mTwitterSearchType.ToString());
                             LinksList.ItemsSource = mLinkItems;
                             LinksList.ItemTemplate = this.Resources["TwitterTemplate"] as DataTemplate;
                             Utilities.Utilities.Instance.SetProgress(false, null);
                         });

                     }).SingleOrDefault();
                }
                else
                    Utilities.Utilities.Instance.MessageBoxShow("Problem with Twitter authorisation - please try later");

            }
            catch
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error searching for links");
            }
            finally
            {
            }
        }

        private void LoadTwitter(string query, string type)
        {
            switch (type)
            {
                case "Person":
                    LoadTwitterSearchItems(query, type);
                    break;
                case "Series":
                    LoadTwitterSearchItems(query, type);
                    break;
            }
        }

        private async void LoadYouTubeJson(string query)
        {
            string response = null;
            bool cancelled = false;
            try
            {
                Utilities.Utilities.Instance.SetProgress(true, "loading links...");
                string URI = string.Format(App.URI_YOUTUBE_GDATA_JSON, query, mWatchLinksPage * mWatchLinksItemsPerPage + 1);
                mWebClient = new CustomWebClient(URI, null);
                response = await mWebClient.DownloadStringAsync();
            }
            catch (TaskCanceledException)
            {
                cancelled = true;
            }
            catch
            {
                cancelled = true;
                Utilities.Utilities.Instance.MessageBoxShow("Error searching for links");
            }
            finally
            {
                if (response != null && response != "" && !cancelled)
                {
                    if (mWatchLinksPage == 0)
                        mLinkItems.Clear();
                    YoutubeRootobject obj = JsonConvert.DeserializeObject<YoutubeRootobject>(response);
                    if (obj != null && obj.data != null && obj.data.items != null)
                    {
                        foreach (var item in obj.data.items)
                        {
                            Link li = new Link();
                            li.Name = item.title;
                            li.Description = item.description;
                            li.URI = item.id;
                            li.Date = item.uploaded;
                            // put out duration as information 
                            int secs = item.duration;
                            int s, m, h;
                            Utilities.Utilities.Instance.ConvertSec(secs, out s, out m, out h);
                            TimeSpan ts = new TimeSpan(h, m, s);
                            li.Information = String.Format("{0}", ts.ToString());
                            mLinkItems.Add(li);
                        }
                    }
                }                     
                Header.Text = String.Format("{0} links found", mLinkItems.Count);
                LinksList.ItemsSource = mLinkItems;
                LinksList.ItemTemplate = this.Resources["WatchTemplate"] as DataTemplate;
                Utilities.Utilities.Instance.SetProgress(false, null);
            }
        }

        private async void LoadNews(News f, string[] query)
        {
            string response = null;
            bool cancelled = false;
 
            if (f.URI == null)
                return;
            try
            {
                Utilities.Utilities.Instance.SetProgress(true, "loading links...");
                mWebClient = new CustomWebClient(f.URI);
                response = await mWebClient.DownloadStringAsync();
            }
            catch (TaskCanceledException)
            {
                cancelled = true;
            }
            catch
            {
                cancelled = true;
                Utilities.Utilities.Instance.MessageBoxShow("Error searching for links");
            }
            finally
            {
                if (response != null && response != "" && !cancelled)
                {
                    StringReader stringReader = new StringReader(response);
                    XmlReader xmlreader = XmlReader.Create(stringReader);
                    try
                    {
                        SyndicationFeed feed = SyndicationFeed.Load(xmlreader);
                        foreach (var item in feed.Items)
                        {
                            Link li = null;
                            if (item.Title != null)
                            {
                                int count = 0;
                                foreach (string word in query)
                                {
                                    if (item.Title.Text.IndexOf(word, StringComparison.CurrentCultureIgnoreCase) >= 0)
                                        count++;
                                }
                                 // look for more word matches in a short query
                                bool m = false;
                                if (query.Count() > 3 && count > 0 && count >= query.Count() - 1)
                                    m = true;
                                else if (query.Count() <= 3 && count > 0 && count == query.Count())
                                    m = true;
                                if (m)
                                {
                                    // match
                                    li = new Link();
                                    li.Source = f.NewsName;
                                    li.Name = item.Title.Text;
                                }
                            }
                            if (li != null && item.PublishDate != null)
                                li.Date = item.PublishDate.DateTime;
                            MatchCollection match = Regex.Matches(
                                item.Summary.Text, "src=\"(\\S+)\"",
                                 RegexOptions.IgnoreCase);
                            // Here we check the Match instance.
                            if (li != null && match.Count > 0 && f.Images == 1)
                            {
                                // extract image (first in match)
                                li.Image = match[0].Groups[1].Value;
                            }
                            if (li != null)
                            {
                                li.Description = item.Summary.Text;
                                if (item.Links.Count > 0)
                                    li.URI = item.Links[0].Uri.AbsoluteUri;
                                else
                                    li.URI = null;
                                mNewsItems.Add(li);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                Header.Text = String.Format("{0} links found", mNewsItems.Count);
                LinksList.ItemsSource = mNewsItems;
                LinksList.ItemTemplate = this.Resources["NewsTemplate"] as DataTemplate;
                Utilities.Utilities.Instance.SetProgress(false, null);
            }
        }

        private async void LoadThemeSong(Link li, string URI)
        {
            string response = null;
            bool cancelled = false;

            try
            {
                Utilities.Utilities.Instance.SetProgress(true, "loading links...");
                mWebClient = new CustomWebClient(URI);
                response = await mWebClient.DownloadStringAsync();
            }
            catch (TaskCanceledException)
            {
                cancelled = true;
            }
            catch
            {
                cancelled = true;
            }
            finally
            {
                if (response != null && response != "" && !cancelled)
                {
                    using (StringReader reader = new StringReader(response))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Contains(".mp3"))
                            {
                                Match match = Regex.Match(line, "\"(.*?)\"", RegexOptions.IgnoreCase);
                                if (match.Success)
                                {
                                    if (match.Groups[1].Value != null)
                                    {
                                        string mp3 = match.Groups[1].Value;
                                        if (!mp3.Contains("http") && !mp3.Contains("HTTP"))
                                            li.URI = string.Format(App.URI_TVTUNES_ROOT, mp3);
                                        else
                                            li.URI = mp3;
                                        mLinkItems.Add(li);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                Header.Text = String.Format("{0} links found ({1})", mLinkItems.Count, "http://www.televisiontunes.com");
                LinksList.ItemsSource = mLinkItems;
                LinksList.ItemTemplate = this.Resources["ListenTemplate"] as DataTemplate;
                Utilities.Utilities.Instance.SetProgress(false, null);
            }
        }

        private async void LoadTheme(string query)
        {
            Stream response = null;
            bool cancelled = false;

            try
            {
                Utilities.Utilities.Instance.SetProgress(true, "loading links...");
                string URI = string.Format(App.URI_TVTUNES, query);
                mWebClient = new CustomWebClient(URI);
                response = await mWebClient.DownloadStreamAsync();
            }
            catch (TaskCanceledException)
            {
                cancelled = true;
            }
            catch
            {
                cancelled = true;
                Utilities.Utilities.Instance.MessageBoxShow("Error searching for links");
            }
            finally
            {
                if (response != null && !cancelled)
                {
                    mLinkItems.Clear();
                    HtmlDocument doc = new HtmlDocument();
                    doc.Load(response);
                    bool OK = false;
                    int count = 0;
                    foreach (HtmlNode node in doc.DocumentNode.DescendantNodes())
                    {
                        switch (node.Name)
                        {
                            case "form":
                                if (node.Attributes["name"] != null)
                                {
                                    string s = node.Attributes["name"].Value;
                                    if (s != null && s == "formSear")
                                        OK = true;
                                }
                                break;

                            case "a":
                                if (OK)
                                {
                                    if (node.Attributes["href"] != null)
                                    {
                                        string u = node.Attributes["href"].Value;
                                        if (u.Contains("http"))
                                        {
                                            Link li = new Link()
                                            {
                                                URI = u,
                                                Name = node.InnerText,
                                            };
                                            count++;
                                            if (count < 4)
                                                // secondary traversal, search for MPG (top three only)
                                                LoadThemeSong(li, li.URI);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    response.Close();
                }
                Utilities.Utilities.Instance.SetProgress(false, null);
            }
        }

        // called on a watch tap play icon
        private void OnWatchTapPlay(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image i = sender as Image;
            if (i != null)
            {
                // get parent item using 
                Link l = i.DataContext as Link;
                if (l != null)
                {
                    MyToolkit.Multimedia.YouTubeQuality quality = YouTubeQuality.Quality480P;
                    string option = App.ViewModel.LoadSettingsKey(App.YOUTUBE_STREAM_KEY) as string;
                    switch (option)
                    {
                        case "480p":
                        default:
                            quality = YouTubeQuality.Quality480P;
                            break;
                        case "720p":
                            quality = YouTubeQuality.Quality720P;
                            break;
                        case "1080p":
                            quality = YouTubeQuality.Quality1080P;
                            break;
                    }
                    mYouTubePlaying = true;
                    YouTube.Play(l.URI, true, quality, x =>
                    {
                        if (x != null)
                            MessageBox.Show(x.Message);
                    });
                }
            }
        }

        // called on a watch tap download
        private void OnWatchTapDownload(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (App.ViewModel.IsTrialMode)
                Utilities.Utilities.Instance.MessageBoxShow("Downloading videos is disabled in Trial Mode");
            else
            {
                Image i = sender as Image;
                if (i != null)
                {
                    // get parent item using 
                    Link l = i.DataContext as Link;
                    if (l != null)
                    {
                        MyToolkit.Multimedia.YouTubeQuality quality = YouTubeQuality.Quality480P;
                        string option = App.ViewModel.LoadSettingsKey(App.YOUTUBE_DL_KEY) as string;
                        switch (option)
                        {
                            case "480p":
                            default:
                                quality = YouTubeQuality.Quality480P;
                                break;
                            case "720p":
                                quality = YouTubeQuality.Quality720P;
                                break;
                            case "1080p":
                                quality = YouTubeQuality.Quality1080P;
                                break;
                        }
                        YouTube.GetVideoUri(l.URI, quality, (entry, excep) =>
                        {
                            if (excep != null)
                            {
                            }
                            else
                            {
                                if (entry != null)
                                {
                                    // here starts the download
                                    DownloadYouTubeWithProgress(l.Name, entry.Uri.AbsoluteUri);
                                }
                            }
                        });
                    }
                }
            }
        }

        // called on a listen tap play icon
        private void OnListenTapPlay(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image i = sender as Image;
            if (i != null)
            {
                // get parent item using 
                Link l = i.DataContext as Link;
                if (l != null)
                {
                    if (l.URI != null && l.URI != "")
                    {
                        NavigationService.Navigate(new Uri(String.Format("/PlayAudioPage.xaml?URI={0}&Name={1}", l.URI, l.Name), UriKind.Relative));
                    }
                }
            }
        }

        private async void DownloadYouTubeWithProgress(string name, string URI)
        {
            if (!mYouTubeDownloading) // don't do more than one at a time
            {
                bool cancelled = false;
                string fileName = name + ".mp4";
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                fileName = Utilities.Utilities.Instance.MakeValidFileName(fileName);
                try
                {
                    StorageFile videoFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                    var videoOutputStream = await videoFile.OpenStreamForWriteAsync();
                    mWebRequest = WebRequest.CreateHttp(URI);
                    mWebRequest.BeginGetResponse(ir =>
                    {
                        try
                        {
                            var result = mWebRequest.EndGetResponse(ir);
                            using (var response = result.GetResponseStream())
                            {
                                mYouTubeDownloading = true;
                                mAbortDownloading = false;
                                Utilities.Utilities.Instance.SetProgress(true, "downloading video...", result.ContentLength, (int)0);
                                var dataBuffer = new byte[BUFFER_SIZE];
                                int count = 0;
                                while (response.Read(dataBuffer, 0, dataBuffer.Length) > 0 && !mAbortDownloading)
                                {
                                    count += dataBuffer.Length;
                                    if (!mAbortDownloading)
                                        Utilities.Utilities.Instance.SetProgress(true, "downloading video...", result.ContentLength, count);
                                    videoOutputStream.Write(dataBuffer, 0, dataBuffer.Length);
                                }
                            }
                            videoOutputStream.Close();
                            mWebRequest = null;
                        }
                        catch (TaskCanceledException)
                        {
                            cancelled = true;
                        }
                        catch
                        {
                            cancelled = true;
                            Utilities.Utilities.Instance.MessageBoxShow("Error downloading video");
                        }
                        finally
                        {
                            Utilities.Utilities.Instance.SetProgress(false, null);
                            if (!cancelled && !mAbortDownloading)
                            {
                                // put out message
                                Utilities.Utilities.Instance.MessageBoxShow("Saved video to local storage (Downloads)");
                            }
                            mYouTubeDownloading = false;
                            mAbortDownloading = false;
                        }
                    }, null);
                }
                catch
                {
                    Utilities.Utilities.Instance.MessageBoxShow("Error downloading video");
                }
                finally
                {
                }
            }
        }

        private void OnLinkSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Note: Youtube play and dowmnload is done on icon tap not selection changed
            // Note: Theme music play and is done on icon tap not selection changed
            LongListSelector list = sender as LongListSelector;
            if (list != null)
            {
                Link l = list.SelectedItem as Link;
                if (l != null)
                {
                    switch (mID)
                    {
                        case "IMDB":
                            if (l.URI != null && l.URI != "")
                            {
                                object setting = App.ViewModel.LoadSettingsKey(App.INTERNAL_BROWSER_KEY);
                                if (setting != null)
                                {
                                    if ((int)setting == 1)
                                        NavigationService.Navigate(new Uri(String.Format("/WebBrowserPage.xaml?URI={0}", l.URI), UriKind.Relative));
                                    else
                                        Utilities.Utilities.Instance.ExplorerLaunch(new Uri(l.URI));
                                }
                            }
                            break;
                        case "News":
                            if (l.URI != null && l.URI != "")
                            {
                                object setting = App.ViewModel.LoadSettingsKey(App.INTERNAL_BROWSER_KEY);
                                if (setting != null)
                                {
                                    if ((int)setting == 1)
                                        NavigationService.Navigate(new Uri(String.Format("/WebBrowserPage.xaml?URI={0}", l.URI), UriKind.Relative));
                                    else
                                        Utilities.Utilities.Instance.ExplorerLaunch(new Uri(l.URI));
                                }
                            }
                            break;
                    }
                }
                list.SelectedItem = null;
            }
        }

        // this function is used to implement infinite scrolling of a very long list
        private void OnLinkItemRealized(object sender, ItemRealizationEventArgs e)
        {
            if (e.ItemKind == LongListSelectorItemKind.Item)
            {
                if (LinksList.ItemsSource.Count > mOffsetKnob)
                {
                    if (e.Container.Content is Link)
                    {
                        if ((e.Container.Content as Link).Equals(LinksList.ItemsSource[LinksList.ItemsSource.Count - mOffsetKnob]))
                        {
                            switch (mID)
                            {
                                case "YouTube":
                                    mWatchLinksPage++;
                                    LoadYouTubeJson(mQuery);
                                    break;
                            }
                        }
                    }
                }
            }
        }

    }
}