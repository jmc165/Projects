using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.WindowsAzure.MobileServices;
using TVSeries80.Models;
using CreateTVSeriesDatabase.Models;
using System.IO;
using System.Threading.Tasks;
using Utilities;

namespace TVSeries80
{
    public partial class ImagePage : PhoneApplicationPage
    {
        private ApplicationBarIconButton mPrevAppBarButton;
        private ApplicationBarIconButton mNextAppBarButton;
        private ApplicationBarIconButton mSaveAppBarButton;
        private ObservableCollection<Banner> mBannerItems = new ObservableCollection<Banner>();
        private IMobileServiceTable<Banner> mBannerTable = App.MobileService.GetTable<Banner>();
        private Banner mBanner = null;
        private int mBannerID;
        private int mSeriesID;
        private int mOffset = 0;
        private CustomWebClient mWebClient = null;

        public ImagePage()
        {
            InitializeComponent();

            // code to localize the ApplicationBar
            BuildLocalizedApplicationBar();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                string value1;
                string value2;
                if (NavigationContext.QueryString.TryGetValue("SeriesID", out value1) &&
                    NavigationContext.QueryString.TryGetValue("BannerID", out value2))
                {
                    if (int.TryParse(value1, out mSeriesID) &&
                        int.TryParse(value2, out mBannerID))
                    {
                        int results = await Load(mBannerID, mSeriesID);
                    }
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (mWebClient != null)
                mWebClient.CancelOperation();
 
            if (e.NavigationMode == NavigationMode.Back)
            {
                // remove all banner items from settings when going back 
                // (Note: NavigationMode.New happens when user presses home button)
                if (App.ViewModel.BannerList != null)
                    App.ViewModel.BannerList.Clear();
            }
        }

        private async Task<int> Load(int bannerID, int seriesID)
        {
            Utilities.Utilities.Instance.SetProgress(true, "");

            // locate this item in the banner list
            mOffset = 0;
            if (App.ViewModel.BannerList != null)
            {
                foreach (var i in App.ViewModel.BannerList)
                {
                    if (i == mBannerID)
                        break;
                    mOffset++;
                }
            }

            // pivot page main
            try
            {
                mBannerItems = await mBannerTable
                .Where(i => (i.id == bannerID && i.SeriesID == seriesID))
                .ToCollectionAsync<Banner>();
            }
            catch
            {
                Utilities.Utilities.Instance.MessageBoxShow("Error loading image from mobile services");
            }
            finally
            {
                if (mBannerItems != null && mBannerItems.Count == 1)
                {
                    mBanner = mBannerItems[0];
                    if (mBanner != null)
                    {
                        // rebind
                        DataContext = mBanner;
                    }
                }
            }
            Utilities.Utilities.Instance.SetProgress(false, null);
            return mBannerItems.Count;
        }

        private async void LoadPrev()
        {
            if (App.ViewModel.BannerList != null && App.ViewModel.BannerList.Count > 0)
            {
                // get offset of current one in the list
                mOffset--;
                if (mOffset < 0) mOffset = App.ViewModel.BannerList.Count - 1;
                mBannerID = App.ViewModel.BannerList[mOffset];
                int results = await Load(mBannerID, mSeriesID);
            }
        }

        private async void LoadNext()
        {
            if (App.ViewModel.BannerList != null && App.ViewModel.BannerList.Count > 0)
            {
                // advance and get next id
                mOffset = (mOffset + 1) % App.ViewModel.BannerList.Count;
                mBannerID = App.ViewModel.BannerList[mOffset];
                int results = await Load(mBannerID, mSeriesID);
            }
        }

        private void OnImageTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            if (fe != null)
            {
                Point tapPosition = e.GetPosition(fe);
                bool left = false;
                if (tapPosition.X < fe.ActualWidth / 2)
                    left = true;
                else
                    left = false;
                if (left)
                    LoadPrev();
                else
                    LoadNext();
            }
        }

        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new previous button
            mPrevAppBarButton = new ApplicationBarIconButton(new Uri("/Images/back.png", UriKind.Relative));
            mPrevAppBarButton.Text = "prev";
            mPrevAppBarButton.Click += OnPrevClick;
            ApplicationBar.Buttons.Add(mPrevAppBarButton);

            // Create a new refresh button
            mSaveAppBarButton = new ApplicationBarIconButton(new Uri("/Images/Dark/save.png", UriKind.Relative));
            mSaveAppBarButton.Text = "save";
            mSaveAppBarButton.Click += OnSaveClick;
            ApplicationBar.Buttons.Add(mSaveAppBarButton);

            // Create a new next button
            mNextAppBarButton = new ApplicationBarIconButton(new Uri("/Images/next.png", UriKind.Relative));
            mNextAppBarButton.Text = "next";
            mNextAppBarButton.Click += OnNextClick;
            ApplicationBar.Buttons.Add(mNextAppBarButton);
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            if (mBanner != null)
            {
                SaveImage(mBanner.BannerPath);
            }
        }

        private void OnPrevClick(object sender, EventArgs e)
        {
            if (mBanner != null)
            {
                LoadPrev();           
            }
        }

        private void OnNextClick(object sender, EventArgs e)
        {
            if (mBanner != null)
            {
                LoadNext();
            }
        }

        private async void SaveImage(string URI)
        {
            Stream response = null;
            bool cancelled = false;
            try
            {
                Utilities.Utilities.Instance.SetProgress(true, "downloading image...");
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
                Utilities.Utilities.Instance.MessageBoxShow("Error downloading image");
            }
            finally
            {
                Utilities.Utilities.Instance.SetProgress(false, null);
                if (response != null && !cancelled)
                {
                    var fileName = string.Format("Gs{0}.jpg", Guid.NewGuid());
                    Utilities.Utilities.Instance.SavePicture(response, fileName);
                    response.Close();
                    Utilities.Utilities.Instance.SetProgress(false, null);
                    Utilities.Utilities.Instance.MessageBoxShow("Saved image to album 'Saved Pictures'");
                 }
            }
        }

    }

   
}