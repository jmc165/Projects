using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Windows.Storage;
using Windows.Networking.Connectivity;
using Microsoft.Xna.Framework.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Info;
using System.IO.IsolatedStorage;
using Utilities;
using HtmlAgilityPack;

namespace Utilities
{
    public class Link
    {
        public string Name { get; set; }
        public string MatchType { get; set; }
        public string URI { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string Information { get; set; }
        public string Image { get; set; }
        public string MediaImage { get; set; }
        public string Source { get; set; }
        public string FormatDate
        {
            get
            {
                if (Date != null)
                    return Date.ToString("yyyy-MM-dd hh:mm");
                else
                    return "";
            }
        }
        public string FormatTimeSpan
        {
            get
            {
                DateTime dt = DateTime.Now;
                if (Date != null)
                {
                    TimeSpan ts = dt - Date;
                    if (ts.Seconds < 0) // back to the future !!
                        return Date.ToString("yyyy-MM-dd hh:mm");
                    else if ((int)ts.TotalDays > 0)
                        return string.Format("{0}d ago", (int)ts.TotalDays);
                    else if ((int)ts.TotalHours > 0)
                        return string.Format("{0}h ago", (int)ts.TotalHours);
                    else if ((int)ts.TotalMinutes > 0)
                        return string.Format("{0}m ago", (int)ts.TotalMinutes);
                    else if ((int)ts.TotalSeconds > 0)
                        return string.Format("{0}s ago", (int)ts.TotalSeconds);
                    else
                        return "";
                }
                else
                    return "";
            }
        }

        public string WatchIcon
        {
            get
            {
                if ((Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible)
                    return "Images/Dark/transport.play.png";
                else
                    return "Images/Light/transport.play.png";
            }
        }

        public string SaveIcon
        {
            get
            {
                if ((Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible)
                    return "Images/Dark/save.png";
                else
                    return "Images/Light/save.png";
            }
        }

    }

    public class CustomWebClient : WebClient
    {
        private string mURI;
        private EventHandler mEventHandler;
        private object mData;

        public CustomWebClient(string URI, EventHandler eventHandler, object data)
        {
            mURI = URI;
            mEventHandler = eventHandler;
            mData = data;
        }

        public CustomWebClient(string URI)
        {
            mURI = URI;
            mEventHandler = null;
        }

        public CustomWebClient(string URI, object data)
        {
            mURI = URI;
            mEventHandler = null;
            mData = data;
        }

        public string GetURI
        {
            get
            {
                return mURI;
            }
        }

        public EventHandler GetEventHandler
        {
            get
            {
                return mEventHandler;
            }
        }

        public object GetData
        {
            get
            {
                return mData;
            }
        }

        public Task<string> DownloadStringAsync()
        {
            return this.DownloadStringTaskAsync(new Uri(mURI));
        }

        public Task<Stream> DownloadStreamAsync()
        {
            return this.OpenReadTaskAsync(new Uri(mURI));
        }

        public void CancelOperation()
        {
            this.CancelAsync();
        }

    }

    public class CustomWebRequest : WebRequest
    {
        // Our generic success callback accepts a string - to read whatever got sent back from server
        public delegate void RESTSuccessCallback(Stream results);

        // the generic fail callback accepts a string - possible dynamic /hardcoded error/exception message from client side
        public delegate void RESTErrorCallback(string reason);

        private WebRequest mWebRequest;
        public RESTSuccessCallback SuccessCallback { get; set; }
        public RESTErrorCallback ErrorCallback { get; set; }
        public Dictionary<string, string> PostData { set; get; }
        public Uri URI { set; get; }

        public WebRequest Request
        {
            get
            {
                return mWebRequest;
            }
        }

        public CustomWebRequest(Uri uri, Dictionary<string, string> postData, RESTSuccessCallback success, RESTErrorCallback error)
        {
            mWebRequest = WebRequest.Create(uri);
            URI = uri;
            PostData = postData;
            SuccessCallback = success;
            ErrorCallback = error;
        }

    }

    public class Utilities : PhoneApplicationPage
    {        
        private Timer mTimer = null;
        private ProgressIndicator mProgress = null;
        private static Utilities mInstance = null;

        public static Utilities Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = new Utilities();
                return mInstance;
            }
        }

        public string MakeValidIdentifier(string name)
        {
            name = name.Replace(":", "");
            name = name.Replace("\"", "");
            name = name.Replace("|", "");
            name = name.Replace("'", "");
            name = name.Replace("&", "");
            name = name.Replace("$", "");
            name = name.Replace("#", "");
            name = name.Replace("!", "");
            name = name.Replace("*", "");
            name = name.Replace("?", "");
            name = name.Replace("=", "");
            return name;
        }

        public void CheckConnectivity()
        {
            bool isNetwork = NetworkInterface.GetIsNetworkAvailable();
            if (!isNetwork)
            {
                 MessageBox.Show("TV Series Guru requires a network connection to fully functional, none has been detected...");
            }
        }

        public string MakeValidFileName(string fileName)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        public void ConvertSec(int xsec, out int sec, out int min, out int hour)
        {
            sec = xsec % 60;
            min = xsec / 60;
            hour = xsec / (60 * 60);
        }

        public async Task<ulong> GetFileSizeAsync(StorageFile file)
        {
            var properties = await file.GetBasicPropertiesAsync();
            return properties.Size;
        }

        public string FormatBytes(ulong bytes)
        {
            double mb = ((double)bytes/1048576.0);
            return mb.ToString("F03");
        }

        // render text to a bitmap, store the file as a JPEG ion local folder and return its path
        public async Task<string> RenderText(string text, int width, int height, int fontsize, string imagename)
        {
            WriteableBitmap b = new WriteableBitmap(width, height);

            var canvas = new Grid();
            canvas.Width = b.PixelWidth;
            canvas.Height = b.PixelHeight;

            var background = new Canvas();
            background.Height = b.PixelHeight;
            background.Width = b.PixelWidth;

            // Created background color as Accent color
            SolidColorBrush backColor = new SolidColorBrush((Color)Application.Current.Resources["PhoneAccentColor"]);
            background.Background = backColor;

            var textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.FontWeight = FontWeights.Bold;
            textBlock.TextAlignment = TextAlignment.Left;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Stretch;
            textBlock.Margin = new Thickness(35);
            textBlock.Width = b.PixelWidth - textBlock.Margin.Left * 2;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Foreground = new SolidColorBrush(Colors.White); //color of the text on the Tile
            textBlock.FontSize = fontsize;

            canvas.Children.Add(textBlock);

            b.Render(background, null);
            b.Render(canvas, null);
            b.Invalidate(); //Draw bitmap

            string fileName = Guid.NewGuid() + ".jpg";
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile pictureFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            using (var pictureOutputStream = await pictureFile.OpenStreamForWriteAsync())
            {
                b.SaveJpeg(pictureOutputStream, b.PixelWidth, b.PixelHeight, 0, 100);
            }
            return pictureFile.Path;
        }

        // save a video to local folder and return its path
        public async Task<string> SaveVideo(Stream videoToSave, string fileName)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile videoFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            using (var videoOutputStream = await videoFile.OpenStreamForWriteAsync())
            {
                await videoToSave.CopyToAsync(videoOutputStream);
            }
            return videoFile.Path;
        }

        // screen shots to local data and stores as file
        public async Task<Uri> Screenshot(Grid LayoutRoot, string uniqueName, int actualWidth, int actualHeight, int width, int height)
        {
            WriteableBitmap bitmap = new WriteableBitmap(actualWidth, actualHeight);
            bitmap.Render(LayoutRoot, new MatrixTransform());
            bitmap.Invalidate();
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await localFolder.CreateFileAsync(uniqueName, CreationCollisionOption.ReplaceExisting);
            using (var fileOutputStream = await file.OpenStreamForWriteAsync())
            {
                bitmap.SaveJpeg(fileOutputStream, width, height, 0, 100);
            }
            bool exists = File.Exists(file.Path);
            return new Uri(string.Format("ms-appdata:///local/{0}", uniqueName), UriKind.Absolute);
        }

        /*
        public Uri ScreenshotToShellContent(Grid LayoutRoot, string uniqueName, int actualWidth, int actualHeight, int width, int height)
        {
            ImageTools.IO.Decoders.AddDecoder<ImageTools.IO.Bmp.BmpDecoder>();
            ImageTools.IO.Encoders.AddEncoder<ImageTools.IO.Png.PngEncoder>();
  
            string file = "Shared/ShellContent/" + uniqueName + ".png";
            WriteableBitmap bitmap = new WriteableBitmap(actualWidth, actualHeight);
            bitmap = bitmap.Resize(width, height, WriteableBitmapExtensions.Interpolation.Bilinear);
            bitmap.Render(LayoutRoot, new MatrixTransform());
            bitmap.Invalidate();
  
            var img = bitmap.ToImage();
            var encoder = new ImageTools.IO.Png.PngEncoder();
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            using (var isoStoreFile = isoStore.OpenFile(file, FileMode.OpenOrCreate, FileAccess.Write))
            {
                encoder.Encode(img, isoStoreFile);
                isoStoreFile.Close();
            }
            return new Uri("isostore:/" + file, UriKind.Absolute);
        }
        */

        /// Copies an image from the internet (http protocol) locally to the AppData LocalFolder.  This is used by some methods 
        /// (like the SecondaryTile constructor) that do not support referencing images over http but can reference them using 
        /// the ms-appdata protocol.  
        public async Task<Uri> GetLocalImageAsync(string internetURI, string uniqueName)
        {
            if (string.IsNullOrEmpty(internetURI))
            {
                return null;
            }
            using (var response = await HttpWebRequest.CreateHttp(internetURI).GetResponseAsync())
            {
                using (var stream = response.GetResponseStream())
                {
                    var desiredName = string.Format("{0}.jpg", uniqueName);
                    var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(desiredName, CreationCollisionOption.ReplaceExisting);

                    using (var filestream = await file.OpenStreamForWriteAsync())
                    {
                        await stream.CopyToAsync(filestream);
                        return new Uri(string.Format("ms-appdata:///local/{0}.jpg", uniqueName), UriKind.Absolute);
                    }
                }
            }
        }

        // save picture to media library
        public void SavePicture(Stream response, string fileName)
        {
            BitmapImage bi = new BitmapImage();
            bi.SetSource(response);
            WriteableBitmap wb = new WriteableBitmap(bi);
            using (var mediaLibrary = new MediaLibrary())
            {
                using (var targetStream = new MemoryStream())
                {
                    wb.SaveJpeg(targetStream, bi.PixelWidth, bi.PixelHeight, 0, 100);
                    targetStream.Seek(0, SeekOrigin.Begin);
                    var picture = mediaLibrary.SavePicture(fileName, targetStream);
                }
            }
        }

        public int LD(string s, string t)
        {
            int n = s.Length; //length of s
            int m = t.Length; //length of t
            int[,] d = new int[n + 1, m + 1]; // matrix
            int cost; // cost
            // Step 1
            if (n == 0) return m;
            if (m == 0) return n;
            // Step 2

            for (int i = 0; i <= n; d[i, 0] = i++) ;

            for (int j = 0; j <= m; d[0, j] = j++) ;

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4

                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    cost = (t.Substring(j - 1, 1) == s.Substring(i - 1, 1) ? 0 : 1);
                    // Step 6
                    d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                              d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        public void HighlightText(TextBlock tb, string query)
        {
            if (tb != null)
            {
                string[] mwords = query.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string[] words = tb.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                tb.Inlines.Clear();
                foreach (string w in words)
                {
                    bool match = false;
                    foreach (string mw in mwords)
                    {
                        if (w.Contains(mw))
                        {
                            match = true;
                            break;
                        }
                    }
                    if (!match)
                    {
                        tb.Inlines.Add(w + " ");
                    }
                    else
                    {
                        tb.Inlines.Add(new Run() { Text = w, FontWeight = FontWeights.ExtraBold});
                        tb.Inlines.Add(new Run() { Text = " ", FontWeight = FontWeights.Normal});
                    }
                }
            }
        }

        public string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.Encoding.Unicode.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        // does a post
        public void Post(
            Uri uri,
            string contentType,
            Dictionary<String, String> post_params,
            Dictionary<String, String> extra_headers,
            CustomWebRequest.RESTSuccessCallback success_callback,
            CustomWebRequest.RESTErrorCallback error_callback)
        {
            WebRequest request = WebRequest.Create(uri);
            request.Method = "POST";
            //we could move the content-type into a function argument too.
            if (contentType == null)
                request.ContentType = "application/x-www-form-urlencoded";
            else
                request.ContentType = contentType;

            //this might be helpful for APIs that require setting custom headers...
            if (extra_headers != null)
                foreach (String header in extra_headers.Keys)
                    try
                    {
                        request.Headers[header] = extra_headers[header];
                    }
                    catch (Exception) { }


            //we first obtain an input stream to which to write the body of the HTTP POST
            request.BeginGetRequestStream((IAsyncResult result) =>
            {
                HttpWebRequest preq = result.AsyncState as HttpWebRequest;
                if (preq != null)
                {
                    Stream postStream = preq.EndGetRequestStream(result);

                    //allow for dynamic spec of post body
                    StringBuilder postParamBuilder = new StringBuilder();
                    if (post_params != null)
                        foreach (String key in post_params.Keys)
                            postParamBuilder.Append(String.Format("{0}={1}", key, post_params[key]));

                    Byte[] byteArray = Encoding.UTF8.GetBytes(postParamBuilder.ToString());

                    //guess one could just accept a byte[] [via function argument] for arbitrary data types - images, audio,...
                    postStream.Write(byteArray, 0, byteArray.Length);
                    postStream.Close();

                    //we can then finalize the request...
                    preq.BeginGetResponse((IAsyncResult final_result) =>
                    {
                        HttpWebRequest req = final_result.AsyncState as HttpWebRequest;
                        if (req != null)
                        {
                            try
                            {
                                //we call the success callback as long as we get a response stream
                                WebResponse response = req.EndGetResponse(final_result);
                                success_callback(response.GetResponseStream());
                            }
                            catch (WebException e)
                            {
                                //otherwise call the error/failure callback
                                error_callback(e.Message);
                                return;
                            }
                        }
                    }, preq);
                }
            }, request);
        }

        // does a post
        public void Post2(
            Uri uri,
            string contentType,
            Dictionary<String, String> postData,
            Dictionary<String, String> extraHeaders,
            CustomWebRequest.RESTSuccessCallback successCallback,
            CustomWebRequest.RESTErrorCallback errorCallback)
        {
            CustomWebRequest request = new CustomWebRequest(uri, postData, successCallback, errorCallback);
            request.Request.Method = "POST";
            //we could move the content-type into a function argument too.
            if (contentType == null)
                request.Request.ContentType = "application/x-www-form-urlencoded";
            else
                request.Request.ContentType = contentType;

            //this might be helpful for APIs that require setting custom headers...
            if (extraHeaders != null)
            {
                foreach (String header in extraHeaders.Keys)
                {
                    try
                    {
                        request.Request.Headers[header] = extraHeaders[header];
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            request.Request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), request);
        }

        private void GetRequestStreamCallback(IAsyncResult callbackResult)
        {
            CustomWebRequest request = (CustomWebRequest)callbackResult.AsyncState;

            // End the stream request operation
            Stream postStream = request.Request.EndGetRequestStream(callbackResult);

            //allow for dynamic spec of post body
            StringBuilder postParamBuilder = new StringBuilder();
            if (request.PostData != null)
                foreach (String key in request.PostData.Keys)
                    postParamBuilder.Append(String.Format("{0}={1}", key, request.PostData[key]));

            Byte[] byteArray = Encoding.UTF8.GetBytes(postParamBuilder.ToString());

            //guess one could just accept a byte[] [via function argument] for arbitrary data types - images, audio,...
            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Close();


            // Start the web request
            request.Request.BeginGetResponse(new AsyncCallback(GetResponseStreamCallback), request);
        }

        private void GetResponseStreamCallback(IAsyncResult callbackResult)
        {
            CustomWebRequest request = (CustomWebRequest)callbackResult.AsyncState;
             try
            {
                HttpWebResponse response = (HttpWebResponse)request.Request.EndGetResponse(callbackResult);
                if (response != null)
                {
                    if (request.SuccessCallback != null)
                        request.SuccessCallback(response.GetResponseStream());
                }
            }
            catch (WebException e)
            {
                if (request.ErrorCallback != null)
                    request.ErrorCallback(e.Message);
            }
        }

        public void BeginRecording()
        {          
            // start a timer to report memory conditions every 3 seconds
            //
            mTimer = new Timer(state =>
            {
                string report = "";
                report += Environment.NewLine +
                  "Current: " + (DeviceStatus.ApplicationCurrentMemoryUsage / 1000000).ToString() + "MB\n" +
                   "Peak: " + (DeviceStatus.ApplicationPeakMemoryUsage / 1000000).ToString() + "MB\n" +
                   "Memory Usage Limit: " + (DeviceStatus.ApplicationMemoryUsageLimit / 1000000).ToString() + "MB\n" +
                   "Device Total Memory: " + (DeviceStatus.DeviceTotalMemory / 1000000).ToString() + "MB\n" +
                   "Working Set Limit: " + Convert.ToInt32((Convert.ToDouble(DeviceExtendedProperties.GetValue("ApplicationWorkingSetLimit")) / 1000000)).ToString() + "MB\n";

                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Debug.WriteLine(report);
                });
            },
                null,
                TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(3));
        }

        public void MessageBoxShow(string message)
        {
            DispatchInvoke(() =>
            {
                MessageBox.Show(message);
            });
        }

        public void SetProgress(bool visible, string text)
        {
            DispatchInvoke(() =>
            {
                if (mProgress == null)
                {
                    mProgress = new ProgressIndicator();
                }
                mProgress.IsIndeterminate = true;
                SystemTray.ProgressIndicator = mProgress;
                if (text != null)
                    mProgress.Text = text;
                 mProgress.IsVisible = visible;
            });
        }

        public void SetProgress(bool visible, string text, long max, int current)
        {
            DispatchInvoke(() =>
            {
                if (mProgress == null)
                {
                    mProgress = new ProgressIndicator();
                }
                mProgress.IsIndeterminate = false;
                mProgress.Value = (double) current / (double) max;
                SystemTray.ProgressIndicator = mProgress;
                if (text != null)
                    mProgress.Text = text;
                mProgress.IsVisible = visible;
            });
        }

        public void DispatchInvoke(Action a)
        {
#if SILVERLIGHT
            if (Dispatcher == null)
                a();
            else
                Dispatcher.BeginInvoke(a);
#else
    if ((Dispatcher != null) && (!Dispatcher.HasThreadAccess))
    {
        Dispatcher.InvokeAsync(
                    Windows.UI.Core.CoreDispatcherPriority.Normal, 
                    (obj, invokedArgs) => { a(); }, 
                    this, 
                    null
         );
    }
    else
        a();
#endif
        }

        // Launch the URI in IE
        public async void ExplorerLaunch(Uri uri)
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

   

    }

}
