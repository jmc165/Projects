using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using System.Resources;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TVSeries80.Resources;
using TVSeries80.ViewModels;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using TVSeries80.Models;
using CreateTVSeriesDatabase.Models;

namespace TVSeries80
{

    public partial class App : Application
    {
        public const string URI_WATCH_SERIES = "http://watchseries.lt";
        public const string URI_IMDB = "http://www.imdb.com";
        public const string URI_TVDB = "http://thetvdb.com";
        public const string URI_YOUTUBE_GDATA_NEWS = "https://gdata.youtube.com/feeds/api/videos?alt=rss&q={0}&start-index={1}";
        public const string URI_YOUTUBE_GDATA_JSON = "https://gdata.youtube.com/feeds/api/videos?v=2&alt=jsonc&q={0}&start-index={1}";
        public const string URI_IMDB_JSON_NAME = "http://www.imdb.com/xml/find?json=1&nr=1&nm=on&q={0}";
        public const string URI_IMDB_JSON_TITLE = "http://www.imdb.com/xml/find?json=1&nr=1&tt=on&q={0}";
        public const string URI_IMDB_NAME = "http://www.imdb.com/name/{0}";
        public const string URI_IMDB_TITLE = "http://www.imdb.com/title/{0}";
        public const string URI_TVTUNES = "http://m.televisiontunes.com/search.php?searWords={0}&Send=Search";
        public const string URI_TVTUNES_ROOT = "http://www.televisiontunes.com{0}";

        public const string TWITTER_CONSUMER_KEY = "LCDF80QvBsc3n2iWlrYMA";
        public const string TWITTER_CONSUMER_SECRET_KEY = "hoUoCQyUR8DreXaTpURd40zSmZ3Ei4n4TV8nnJA4";
        public const string TWITTER_ACCESS_TOKEN_KEY = "29516291-eaytXM3rPMDBsrq5MN6itrkxmBfvwFfwqJFeVkjnP";
        public const string TWITTER_ACCESS_TOKEN_SECRET_KEY = "GKj9jSZxt3FeWirRA4SjRm4QpJ7HJjn9tnwRYMADw";
        public const string TWITTER_USER_AGENT_KEY = "TV Series Guru";

        public const string URI_TWITTER = "api.twitter.com";
        public const string URI_TWITTER_REQUEST_TOKEN = "https://api.twitter.com/oauth/request_token";
        public const string URI_TWITTER_GET_BEARER_TOKEN = "https://api.twitter.com/1/oauth2/token";
        public const string URI_TWITTER_AUTHORISE = "https://api.twitter.com/oauth/authorize";
        public const string URI_TWITTER_ACCESS_TOKEN =  "https://api.twitter.com/oauth/access_token";

        public const string DATE_FORMAT = "yyyy-MM-dd hh:mm";

        public const string TVDB_API = "C4872D6A2AAE617A";

        public const int MAX_HISTORY = 20;

        // application settings keys
        public const string FAVORITES_KEY = "Favorites";                        // stores favorites
        public const string HISTORY_KEY = "Recents";                            // stores recents
        public const string INTERNAL_BROWSER_KEY = "InternalBrowser";           // 1 internal browser, 0 IE
        public const string SEARCH_LIMIT_KEY = "SearchLimit";                   // 50,100,200,500 search items
        public const string DISPLAY_NEWS_KEY = "DisplayNews";                   // Viewing News list
        public const string DEFAULT_NEWS_KEY = "DefaultNews";                   // Default News list
        public const string YOUTUBE_STREAM_KEY = "TouTubeStreamQuality";        // youtube streaming quality
        public const string YOUTUBE_DL_KEY = "YouTubeDLQuality";                // youtube download quality
        public const string WEBSITE_STYLE_KEY = "WebsiteStyle";                 // mobile or desktop
        public const string TWITTER_SEARCH_LIMIT_KEY = "TwitterSearchLimit";    // Twitter Search limit
        public const string TWITTER_SEARCH_TYPE_KEY = "TwitterSearchType";      // Twitter Search type
 
        // Mobile service credentials
        public static MobileServiceClient MobileService = new MobileServiceClient(
            "https://tvseries.azure-mobile.net/",
            "AAVLYVRvrOpVarwQSzjqesOBiTmUUv53"
        );

        private static MainViewModel viewModel = null;

        /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The MainViewModel object.</returns>
        public static MainViewModel ViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (viewModel == null)
                    viewModel = new MainViewModel();

                return viewModel;
            }
        }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {

#if DEBUG
            Utilities.Utilities.Instance.BeginRecording();
#endif
            // Global handler for uncaught exceptions.
            UnhandledException += Application_UnhandledException;

            // Standard XAML initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Language display initialization
            InitializeLanguage();

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = false;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode,
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Prevent the screen from turning off while under the debugger by disabling
                // the application's idle detection.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            App.ViewModel.Launch(e);
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            App.ViewModel.Activated(e);
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Ensure that required application state is persisted here.
            App.ViewModel.Deactivated(e);
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            // Ensure that required application state is persisted here.
            App.ViewModel.Closing(e);
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;
  
        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Handle reset requests for clearing the backstack
            RootFrame.Navigated += CheckForResetNavigation;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            // If the app has received a 'reset' navigation, then we need to check
            // on the next navigation to see if the page stack should be reset
            if (e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            // Unregister the event so it doesn't get called again
            RootFrame.Navigated -= ClearBackStackAfterReset;

            // Only clear the stack for 'new' (forward) and 'refresh' navigations
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;

            // For UI consistency, clear the entire page stack
            while (RootFrame.RemoveBackEntry() != null)
            {
                ; // do nothing
            }
        }

        #endregion

        // Initialize the app's font and flow direction as defined in its localized resource strings.
        //
        // To ensure that the font of your application is aligned with its supported languages and that the
        // FlowDirection for each of those languages follows its traditional direction, ResourceLanguage
        // and ResourceFlowDirection should be initialized in each resx file to match these values with that
        // file's culture. For example:
        //
        // AppResources.es-ES.resx
        //    ResourceLanguage's value should be "es-ES"
        //    ResourceFlowDirection's value should be "LeftToRight"
        //
        // AppResources.ar-SA.resx
        //     ResourceLanguage's value should be "ar-SA"
        //     ResourceFlowDirection's value should be "RightToLeft"
        //
        // For more info on localizing Windows Phone apps see http://go.microsoft.com/fwlink/?LinkId=262072.
        //
        private void InitializeLanguage()
        {
            try
            {
                // Set the font to match the display language defined by the
                // ResourceLanguage resource string for each supported language.
                //
                // Fall back to the font of the neutral language if the Display
                // language of the phone is not supported.
                //
                // If a compiler error is hit then ResourceLanguage is missing from
                // the resource file.
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                // Set the FlowDirection of all elements under the root frame based
                // on the ResourceFlowDirection resource string for each
                // supported language.
                //
                // If a compiler error is hit then ResourceFlowDirection is missing from
                // the resource file.
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }
            catch
            {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }
        }
    }
}