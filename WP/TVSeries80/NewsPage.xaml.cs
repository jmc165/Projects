using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TVSeries80.Models;
using CreateTVSeriesDatabase.Models;
using System.IO;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Threading.Tasks;
using Utilities;

namespace TVSeries80
{
    public partial class NewsPage : PhoneApplicationPage
    {
        private ObservableCollection<Link> mNewsItems = new ObservableCollection<Link>();
        private CustomWebClient mWebClient = null;

        public NewsPage()
        {
            InitializeComponent();

            DataContext = App.ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                string name;
                string URI;
                string images;
                // page may be called via URL
                if (NavigationContext.QueryString.TryGetValue("Name", out name) &&
                    NavigationContext.QueryString.TryGetValue("URI", out URI) &&
                    NavigationContext.QueryString.TryGetValue("Images", out images))
                {
                    Header.Text = "No items found";
                    if (name == "Top Stories")  // combine top 3 stories from each
                    {
                        ObservableCollection<News> feeds = App.ViewModel.LoadSettingsKey(App.DEFAULT_NEWS_KEY) as ObservableCollection<News>;
                        if (feeds != null)
                        {
                            mNewsItems.Clear();
                            foreach (News feed in feeds)
                            {
                                if (feed.Enabled)
                                    LoadNewsItems(feed.NewsName, feed.URI, feed.Images, 3);
                            }
                        }
                    }
                    else // just load individually
                        LoadNewsItems(name, URI, int.Parse(images), 0);
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (mWebClient != null)
                mWebClient.CancelOperation();

        }

        public async void LoadNewsItems(string name, string URI, int images, int limit)
        {
            string response = null;
            bool cancelled = false;

            if (URI == null)
                return;
            try
            {
                Utilities.Utilities.Instance.SetProgress(true, "loading feed details...");
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
                Utilities.Utilities.Instance.MessageBoxShow("Error loading feed items from mobile services");
            }
            finally
            {
                if (response != null && response != "" && !cancelled)
                {
                    if (limit == 0)
                        mNewsItems.Clear();
                    StringReader stringReader = new StringReader(response);
                    XmlReader xmlreader = XmlReader.Create(stringReader);
                    try
                    {
                        SyndicationFeed news = SyndicationFeed.Load(xmlreader);
                        int count = 0;
                        foreach (var item in news.Items)
                        {
                            Link li = new Link();
                            li.Source = name;
                            if (item.Title != null)
                                li.Name = item.Title.Text;
                            if (item.PublishDate != null)
                                li.Date = item.PublishDate.DateTime;
                            if (item.Summary != null)
                            {
                                MatchCollection match = Regex.Matches(
                                    item.Summary.Text, "src=\"(\\S+)\"",
                                     RegexOptions.IgnoreCase);
                                // Here we check the Match instance.
                                if (match.Count > 0 && images == 1)
                                {
                                    string url = match[0].Groups[1].Value;
                                    // extract image (first in match)
                                    if (url.Contains(".jpg"))
                                        li.Image = url;
                                }
                                li.Description = item.Summary.Text;
                            }
                            if (item.Links.Count > 0)
                                li.URI = item.Links[0].Uri.AbsoluteUri;
                            else
                                li.URI = null;
                            mNewsItems.Add(li);
                            count++;
                            // limit the new items requested
                            if (limit != 0 && count == limit)
                                break;
                        }
                    }
                    catch
                    {
                    }
                }
                // update UI
                if (limit == 0)
                    Header.Text = String.Format("{0} ({1} items)", name, mNewsItems.Count);
                else
                    Header.Text = String.Format("{0} ({1} items)", "Top Stories", mNewsItems.Count);
                NewsList.ItemsSource = mNewsItems;
                Utilities.Utilities.Instance.SetProgress(false, null);
            }
        }

        private void OnNewsItemSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
          LongListSelector list = sender as LongListSelector;
          if (list != null)
          {
              Link l = list.SelectedItem as Link;
              if (l != null && l.URI != null)
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
              list.SelectedItem = null;
          }
        }

    }
}