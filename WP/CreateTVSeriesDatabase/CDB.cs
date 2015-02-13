using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using CreateTVSeriesDatabase.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CreateTVSeriesDatabase
{
 
    public class CDB
    {
        private string ROOT = "http://watchseries.lt";
        private string IMDB = "http://www.imdb.com";
        private string URI_IMDB_NAME = "http://www.imdb.com/name/{0}";
        private string TVDB = "http://thetvdb.com";
        private string URI_IMDB_JSON_NAME = "http://www.imdb.com/xml/find?json=1&nr=1&nm=on&q={0}";
        private string API = "C4872D6A2AAE617A";
        private const string CACHEDIR = "C:\\TFSProjects\\WP\\CreateTVSeriesDatabase\\cache";
        private const int MATCH_LIMIT = 25;

        // SQL Azure information
        private static string mUserName = "SQLAdmin";
        private static string mPassword = "SQLk3yb0ar";
        private static string mDataSource = "q6swzcvnjq.database.windows.net";
        private static string mSampleDatabaseName = "TVSeries";
        private SqlConnectionStringBuilder mConnStringBuilder;
        private List<string> mTables = new List<string>();
        private System.Windows.Forms.RichTextBox mRichTextBox;
        private System.Windows.Forms.TextBox mInformationTextBox;
        private System.Windows.Forms.TextBox mInputTextBox;
        private bool mVerbose = false;
        private bool mDoSeries = false;
        private bool mDoEpisode = false;
        private bool mDoPerson = false;
        private bool mDoBanner = false;
        private bool mDoGenre = false;
        private Thread mThread = null;
        private CustomWebClient mWebClient = null;
        private int mErrors = 0;
        private int mSeriesID = 0;
        private int mEpisodeID = 0;
        private int mGenreID = 0;
        private int mGenreInstanceID = 0;
        private int mPersonInstanceID = 0;
        private int mBannerID = 0;
        private int mPersonID = 0;
        private int mSeriesCount = 0;

        public CDB(System.Windows.Forms.RichTextBox richTextBox, System.Windows.Forms.TextBox informationTextBox, System.Windows.Forms.TextBox inputTextBox, bool[] flags, string option)
        {
            this.mRichTextBox = richTextBox;
            this.mInformationTextBox = informationTextBox;
            this.mInputTextBox = inputTextBox;

            mVerbose = flags[0];
            mDoSeries = flags[1];
            mDoEpisode = flags[2];
            mDoPerson = flags[3];
            mDoBanner = flags[4];
            mDoGenre = flags[5];

            if (mDoGenre)
                mTables.Add("GenreInstance");
            if (mDoPerson)
                mTables.Add("PersonInstance");
            if (mDoEpisode)
                mTables.Add("Episode");
            if (mDoBanner)
                mTables.Add("Banner");
            if (mDoSeries)
                mTables.Add("Series");
            if (mDoGenre)
                mTables.Add("Genre");
            if (mDoPerson)
                mTables.Add("Person");
   
            // Create a connection string for the sample database
            mConnStringBuilder = new SqlConnectionStringBuilder();
            mConnStringBuilder.DataSource = mDataSource;
            mConnStringBuilder.InitialCatalog = mSampleDatabaseName;
            mConnStringBuilder.Encrypt = true;
            mConnStringBuilder.TrustServerCertificate = false;
            mConnStringBuilder.UserID = mUserName;
            mConnStringBuilder.Password = mPassword;

            // create cache directory
            if (!Directory.Exists(CACHEDIR))
                Directory.CreateDirectory(CACHEDIR);
 
            switch(option)
            {
                case "Download":
                    mThread = new Thread(new ThreadStart(Download));
                    mThread.Name = option;
                    mThread.IsBackground = true;
                    mThread.SetApartmentState(ApartmentState.STA);
                    mThread.Priority = ThreadPriority.Highest;
                    mThread.Start();
                    break;
                case "DownloadUpdate":
                    mThread = new Thread(new ThreadStart(DownloadUpdate));
                    mThread.Name = option;
                    mThread.IsBackground = true;
                    mThread.SetApartmentState(ApartmentState.STA);
                    mThread.Priority = ThreadPriority.Highest;
                    mThread.Start();
                    break;
                case "Unzip":
                    mThread = new Thread(new ThreadStart(Unzip));
                    mThread.Name = option;
                    mThread.IsBackground = true;
                    mThread.SetApartmentState(ApartmentState.STA);
                    mThread.Priority = ThreadPriority.Highest;
                    mThread.Start();
                    break;
                case "CreateDatabase":
                    mThread = new Thread(new ThreadStart(CreateDatabase));
                    mThread.Name = option;
                    mThread.IsBackground = true;
                    mThread.SetApartmentState(ApartmentState.STA);
                    mThread.Priority = ThreadPriority.Highest;
                    mThread.Start();
                    break;
                case "CreateUpdateDatabase":
                    mThread = new Thread(new ThreadStart(CreateUpdateDatabase));
                    mThread.Name = option;
                    mThread.IsBackground = true;
                    mThread.SetApartmentState(ApartmentState.STA);
                    mThread.Priority = ThreadPriority.Highest;
                    mThread.Start();
                    break;
                case "SearchAndUpdate":
                    mThread = new Thread(new ThreadStart(SearchAndUpdate));
                    mThread.Name = option;
                    mThread.IsBackground = true;
                    mThread.SetApartmentState(ApartmentState.STA);
                    mThread.Priority = ThreadPriority.Highest;
                    mThread.Start();
                    break;
                case "PersonPictures":
                    mThread = new Thread(new ThreadStart(PersonPictures));
                    mThread.Name = option;
                    mThread.IsBackground = true;
                    mThread.SetApartmentState(ApartmentState.STA);
                    mThread.Priority = ThreadPriority.Highest;
                    mThread.Start();
                    break;
                case "DeleteTables":
                    DialogResult dr = MessageBox.Show("Are you sure you want to delete tables?", "Warning", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes)
                    {
                        mThread = new Thread(new ThreadStart(DeleteTables));
                        mThread.Name = option;
                        mThread.IsBackground = true;
                        mThread.SetApartmentState(ApartmentState.STA);
                        mThread.Priority = ThreadPriority.Highest;
                        mThread.Start();
                    }
                    break;
            }
        }

        // create database is used to create the database from scratch, it makes uise of the cache
        // really only during developent
        private void Download()
        {
            // get time from TVDV (this is the tiemstamp relating to all the
            // data we are about to download)
            int timeStamp = GetTVDMTimeStamp();
            // set maintainance mode
            SetMaintainenceMode(timeStamp, 1, DateTime.Now);
             // open root page
            HtmlWeb webClient = new HtmlWeb();
            mSeriesCount = 0; // diagnostics
            // parse root page
            ParseRootPage(webClient, ROOT);
            // clear maintainence mode, but set timesatamp in the admin table
            // for later use
            SetDiagnostics(mSeriesCount);
            SetMaintainenceMode(timeStamp, 0, DateTime.Now);
            Message("Finished!\r");
        }

        private void DownloadUpdate()
        {
            List<Series> seriesList;
            List<Episode> episodeList;
            HtmlWeb webClient = new HtmlWeb();

            // get last timestamp from admin
            long previousTimeStamp = GetAdministrationTimeStamp();

            // set maintainance mode
            SetMaintainenceMode(previousTimeStamp, 1, DateTime.Now);

            long newTimeStamp = previousTimeStamp;

            // return list of changed series/episodes and new time stamp
            GetTVDBUpdates(previousTimeStamp, out seriesList, out episodeList, out newTimeStamp);

            // download each update series into its own XML
            int count = 0;
            foreach (var series in seriesList)
            {
                count++;
                // store the update in a seperate update directory
                string URI = string.Format("{0}/api/{1}/series/{2}/en.xml", TVDB, API, series.TVDB_ID);
                string dirName = string.Format("{0}\\{1}.updateSeries", CACHEDIR, series.TVDB_ID);
                string fileName = string.Format("{0}\\{1}.updateSeries\\en.xml", CACHEDIR, series.TVDB_ID);
                if (!Directory.Exists(dirName))
                    Directory.CreateDirectory(dirName);
                Message(string.Format("{0}\r", dirName));
                XmlDocument doc = CachePage(webClient, URI, fileName);
            }

            // download each update episode into its own XML
            count = 0;
            foreach (var episode in episodeList)
            {
                count++;
                // store the update in a seperate update directory
                string URI = string.Format("{0}/api/{1}/episodes/{2}/en.xml", TVDB, API, episode.TVDB_ID);
                string dirName = string.Format("{0}\\{1}.updateEpisode", CACHEDIR, episode.TVDB_ID);
                string fileName = string.Format("{0}\\{1}.updateEpisode\\en.xml", CACHEDIR, episode.TVDB_ID);
                if (!Directory.Exists(dirName))
                    Directory.CreateDirectory(dirName);
                Message(string.Format("{0}\r", dirName));
                XmlDocument doc = CachePage(webClient, URI, fileName);
            }

            // clear maintainence mode but set the new time stamp (comment this in when finished)
            SetMaintainenceMode(newTimeStamp, 0, DateTime.Now);
            Message("Finished!\r");
        }

        // unzip
        private void Unzip()
        {
            // get last timestamp from admin
            long previousTimeStamp = GetAdministrationTimeStamp();
            // set maintainance mode
            SetMaintainenceMode(previousTimeStamp, 1, DateTime.Now);
            mSeriesCount = 0; // diagnostics
            // traverse the cache
            DirectoryInfo di = new DirectoryInfo(CACHEDIR);
            mSeriesCount = 0;
            foreach (FileInfo fi in di.GetFiles())
            {
                if (fi.Name.Contains(".zip")) // each series has 1 zipExtracted directory
                {
                    if (mSeriesCount % 10 == 0)
                        SetDiagnostics(mSeriesCount);
                    UnzipFile(fi.FullName);
                    Information(string.Format("Extracting Zip Series: #{0}", ++mSeriesCount));
                }
            }
            // clear maintainence mode, but set timesatamp in the admin table
            // for later use
            SetDiagnostics(mSeriesCount);
            SetMaintainenceMode(previousTimeStamp, 0, DateTime.Now);
            Message("Finished!\r");
        }

        // delete the database tables - use as absolute last resort
        private void DeleteTables()
        {
            // get last timestamp from admin
            long previousTimeStamp = GetAdministrationTimeStamp();
            // set maintainance mode
            SetMaintainenceMode(previousTimeStamp, 1, DateTime.Now);
            DeleteTableContents();
            // clear maintainence mode
            SetMaintainenceMode(previousTimeStamp, 0, DateTime.Now);
            Message("Finished!\r");
        }

        // create the database tables
        private void CreateDatabase()
        {
            // get last timestamp from admin
            long previousTimeStamp = GetAdministrationTimeStamp();
            // set maintainance mode
            SetMaintainenceMode(previousTimeStamp, 1, DateTime.Now);
            // set diagnostics
            SetDiagnostics(0);
            // get unique id's for any possible new entries
            GetInsertCounts();
            // reset series count
            mSeriesCount = 0;

            // if there is a file specified as input, assumed there is a valid location on each line
            // and process only that
            int counter = int.MinValue;

            if (mInputTextBox.Text != "")
            {
                string file = mInputTextBox.Text;
                if (File.Exists(file))
                {
                    FileStream fs = null;
                    StreamReader reader = null;
                    try
                    {
                        fs = new FileStream(file, FileMode.Open);
                        reader = new StreamReader(fs);
                        mSeriesCount = 0;
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            if (Directory.Exists(line) && line.Contains("zipExtracted"))
                            {
                                if (mSeriesCount % 10 == 0)
                                    SetDiagnostics(mSeriesCount);
                                ProcessFullUnzippedSeries(line, ++mSeriesCount);
                            }
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        if (reader != null)
                            reader.Close();
                        if (fs != null)
                            fs.Close();
                    }
                    SetDiagnostics(mSeriesCount);
                    // clear maintainence mode but don't change timestamp
                    SetMaintainenceMode(previousTimeStamp, 0, DateTime.Now);
                    Message("Finished!\r");
                    return;
                }
                else if (int.TryParse(mInputTextBox.Text, out counter)) // start from offset counter
                {
                }
            }
   
            int count = 0;
            mSeriesCount = 0;
            // traverse the cache
            DirectoryInfo di = new DirectoryInfo(CACHEDIR);
            foreach (DirectoryInfo di2 in di.GetDirectories())
            {
                if (di2.Name.Contains("zipExtracted")) // each series has 1 zipExtracted directory
                {
                    count++;
                    if (count >= counter)   // assume counter is specifed start point
                    {
                        if (mSeriesCount % 10 == 0)
                            SetDiagnostics(mSeriesCount);
                        ProcessFullUnzippedSeries(di2.FullName, ++mSeriesCount);
                    }
                }
            }
   

            SetDiagnostics(mSeriesCount);
            // clear maintainence mode but don't change timestamp
            SetMaintainenceMode(previousTimeStamp, 0, DateTime.Now);
            Message("Finished!\r");
        }

        // create the database tables
        private void CreateUpdateDatabase()
        {
            // get last timestamp from admin
            long previousTimeStamp = GetAdministrationTimeStamp();
            // set maintainance mode
            SetMaintainenceMode(previousTimeStamp, 1, DateTime.Now);
            // set diagnostics
            SetDiagnostics(0);
            // get unique id's for any possible new entries
            GetInsertCounts();
            // traverse the cache
            DirectoryInfo di = new DirectoryInfo(CACHEDIR);
            int count = 0;

            foreach (DirectoryInfo di2 in di.GetDirectories())
            {
                if (di2.Name.Contains("updateSeries"))
                {
                    count++;
                    if (count % 10 == 0)
                        SetDiagnostics(count);
                    ProcessUpdateSeries(di2.FullName, count);
                }
            }

            foreach (DirectoryInfo di2 in di.GetDirectories())
            {
                if (di2.Name.Contains("updateEpisode"))
                {
                    count++;
                    if (count % 10 == 0)
                        SetDiagnostics(count);
                    ProcessUpdateEpisode(di2.FullName, count);
                }
            }

            SetDiagnostics(mSeriesCount);
            // clear maintainence mode but don't change timestamp
            SetMaintainenceMode(previousTimeStamp, 0, DateTime.Now);
            Message("Finished!\r");
        }

        // SearchAndUpdate searches for a title by name and incrementally adds it to the database (if it does not exist)
        private void SearchAndUpdate()
        {
            if (mInputTextBox.Text != "")
            {
                List<Series> seriesList;
                // get last timestamp from admin
                long previousTimeStamp = GetAdministrationTimeStamp();
                // set maintainance mode
                SetMaintainenceMode(previousTimeStamp, 1, DateTime.Now);
                // set diagnostics
                SetDiagnostics(0);
                // get unique id's for any possible new entries
                GetInsertCounts();
                HtmlWeb webClient = new HtmlWeb();
                // download all the zips to cache that match series name (no limit)
                GetSeriesFromTVDBAndStoreInCache(webClient, mInputTextBox.Text, int.MaxValue, out seriesList);
                mSeriesCount = 0;
                // unzip
                foreach (var series in seriesList)
                {
                    Message(String.Format("Series '{0}' {1}\r", series.SeriesName, series.TVDB_ID));
                    string zipFile = String.Format("{0}\\{1}.zip", CACHEDIR, series.TVDB_ID);
                    if (File.Exists(zipFile))
                    {
                        UnzipFile(zipFile);
                    }
                }
                Thread.Sleep(3000);
                // ProcessFullUnzippedSeries
                foreach (var series in seriesList)
                {
                    string dirName = String.Format("{0}\\{1}.zipExtracted", CACHEDIR, series.TVDB_ID);
                    if (Directory.Exists(dirName))
                    {
                        ProcessFullUnzippedSeries(dirName, ++mSeriesCount);
                    }
                }
                SetDiagnostics(mSeriesCount);
                // clear maintainence mode, but set timesatamp in the admin table
                // for later use
                SetMaintainenceMode(previousTimeStamp, 0, DateTime.Now);
            }
            Message("Finished!\r");
        }

        private void PersonPictures()
        {
            // get last timestamp from admin
            long previousTimeStamp = GetAdministrationTimeStamp();
            // set maintainance mode
            SetMaintainenceMode(previousTimeStamp, 1, DateTime.Now);
            // set diagnostics
            SetDiagnostics(0);
            HtmlWeb webClient = new HtmlWeb();
            List<Person> blanks = MatchPersonNoImage();
            int count = 0;
            int counter = int.MinValue;

            if (mInputTextBox.Text != "")
                int.TryParse(mInputTextBox.Text, out counter); // start from offset counter

            Message(String.Format("{0} persons with no picture found!\r", blanks.Count));

            if (counter == -1)    // randomise
            {
                while (blanks.Count > 0)
                {
                    Random r = new Random(DateTime.Now.Second);
                    count = r.Next(blanks.Count);
                    GetPicture(blanks[count], count);
                    blanks.RemoveAt(count);
                }
            }
            else
            {
                foreach (Person p in blanks)
                {
                    count++;
                    if (count > counter)
                    {
                        GetPicture(p, count);
                    }
                }
            }

            // clear maintainence mode, but set timesatamp in the admin table
            // for later use
            SetMaintainenceMode(previousTimeStamp, 0, DateTime.Now);
            Message("Finished!\r");
        }

        public void GetPicture(Person p, int count)
        {
            LoadIMDBSearchItems(string.Format(URI_IMDB_JSON_NAME, p.Name), p);
            if (p.Image != "") // changed
            {
                if (mDoPerson)
                    UpdatePerson(p.id, p);
                string message = string.Format("Person: #{0} ID {1} {2} Image found in IMDB", count, p.id, p.Name);
                Information(message);
                Message(message + "\r");
            }
            else
                Information(string.Format("Person: #{0} ID {1} {2} Image not found in IMDB", count, p.id, p.Name));
            Thread.Sleep(1000);
        }

        public void LoadIMDBSearchItems(string URI, Person p)
        {
            string response = null;
            bool cancelled = false;
            string subURI;

            try
            {
                mWebClient = new CustomWebClient(URI);
                response = mWebClient.DownloadString();
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
                    bool found = false;
                    PersonRootobject obj = null;
                    try
                    {
                        obj = JsonConvert.DeserializeObject<PersonRootobject>(response);
                    }
                    catch
                    {
                        obj = null;
                    }
                    finally
                    {
                        if (obj != null && obj.name_exact != null && !found)
                        {
                            foreach (var i in obj.name_exact)
                            {
                                subURI = String.Format(URI_IMDB_NAME, i.id);
                                LoadIMDBPerson(subURI, p);
                                found = true;
                                break;
                            }
                        }
                    }
                    /*
                    if (obj != null && obj.name_popular != null && !found)
                    {
                        foreach (var i in obj.name_popular)
                        {
                            subURI = String.Format(URI_IMDB_NAME, i.id);
                            LoadIMDBPerson(subURI, p);
                            found = true;
                            break;
                        }
                    }
                     * */
                }
            }
        }

        public void LoadIMDBPerson(string URI, Person p)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            try
            {
                // load the page
                HtmlWeb webClient = new HtmlWeb();
                doc = webClient.Load(URI);
            }
            catch
            {
            }
            finally
            {
                if (doc != null)
                {
                    HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//img");
                    if (nodes != null)
                    {
                        foreach (var node in nodes)
                        {
                            if (node.Attributes["id"] != null &&
                                node.Attributes["src"] != null)      
                            {
                                string image = node.Attributes["src"].Value as string;
                                if (image != null && image.Contains(".jpg"))
                                {
                                    // update image
                                    p.Image = image;
                                }
                                break;                    
                            }
                        }
                    }
                }
            }
        }

        private int GetTVDMTimeStamp()
        {
            string response = MakeWebRequest(TVDB + "/" + "api/Updates.php?type=none");
            if (response != null && response != "")
            {
                XmlDocument doc = new XmlDocument();        // Create an XML document object
                doc.LoadXml(response);                      // Load the XML document from the specified file
                XmlNodeList nodes = doc.SelectNodes("//Items");
                foreach (XmlNode node in nodes)
                {
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        switch (childNode.Name)
                        {
                            case "Time":
                                return int.Parse(childNode.InnerText);
                        }
                    }
                }
            }
            return 0;
        }

        private void GetTVDBUpdates(long timeStamp, out List<Series> seriesList, out List<Episode> episodeList, out long newTimeStamp)
        {
            List<XmlDocument> docs = new List<XmlDocument>();
            seriesList = new List<Series>();
            episodeList = new List<Episode>();
            newTimeStamp = -1;

            string response = MakeWebRequest(TVDB + "/" + "api/Updates.php?type=all&time=" + timeStamp.ToString());
            if (response != null && response != "")
            {
                XmlDocument doc = new XmlDocument();        // Create an XML document object
                doc.LoadXml(response);                      // Load the XML document from the specified file

                // select series
                XmlNodeList items = doc.SelectNodes("//Time");

                foreach (XmlNode node in items)
                {
                    if (node.Name == "Time")
                    {
                        newTimeStamp = long.Parse(node.InnerText);
                        break;
                    }
                }

                // select series
                items = doc.SelectNodes("//Series");

                foreach (XmlNode node in items)
                {
                    if (node.Name == "Series")
                    {
                        Series series = new Series()
                        {
                            TVDB_ID = int.Parse(node.InnerText),
                        };
                        seriesList.Add(series);
                    }
                }

                // select episodes
                items = doc.SelectNodes("//Episode");
                foreach (XmlNode node in items)
                {
                    if (node.Name == "Episode")
                    {
                        Episode episode = new Episode()
                        {
                            TVDB_ID = int.Parse(node.InnerText),
                        };
                        episodeList.Add(episode);
                    }
                }
        
            }
        }

        private void ProcessFullUnzippedSeries(string unpackDirectory, int seriesCount)
        {
            bool matched;
            string file;
            XmlDocument doc;
            int seriesID = -1;
            int episodeID = -1;
            int bannerID = -1;
            int genreID = -1;
            int genreInstanceID = -1;

            // create a dummy placeholder for the series
            List<XmlDocument> docs = new List<XmlDocument>();
            List<Episode> episodes = new List<Episode>();
            List<Banner> banners = new List<Banner>();
            List<SeriesPerson> persons = new List<SeriesPerson>();
            List<Genre> genres = new List<Genre>();
            bool okToContinue = true;

            try
            {
                file = unpackDirectory + "\\en.xml";
                if (File.Exists(file))
                {
                    doc = new XmlDocument();                             // Create an XML document object
                    doc.Load(file);                                      // Load the XML document from the specified file
                    docs.Add(doc);
                }
                file = unpackDirectory + "\\banners.xml";
                if (File.Exists(file))
                {
                    doc = new XmlDocument();                             // Create an XML document object
                    doc.Load(file);                                      // Load the XML document from the specified file
                    docs.Add(doc);
                }
                file = unpackDirectory + "\\actors.xml";
                if (File.Exists(file))
                {
                    doc = new XmlDocument();                             // Create an XML document object
                    doc.Load(file);                                      // Load the XML document from the specified file
                    docs.Add(doc);
                }
            }
            catch
            {
                okToContinue = false;
            }

            if (!okToContinue)
                return;

            Series series = Series.CreateDefaults();
   
            // reparse into lists (no SQL here pure XML crunching)
            if (docs.Count >= 1)
                ParseSeries(docs[0], series, genres);

            // parse episodes
            if (docs.Count >= 1)
                ParseEpisode(docs[0], series, episodes);

                // parse banner
            if (docs.Count >= 2)
                ParseBanner(docs[1], series, banners);

            // parse regular series Person
            if (docs.Count >= 3)
                ParseActor(docs[2], persons);

            // use the TVDB as an ID
            CheckAndAddSeries(series.TVDB_ID, out seriesID, out matched);

            // something terrible has happened, ignore entire series
            if (seriesID == -1)
                return;

            series.id = seriesID;

            if (mDoSeries)
                // overwrite contents
                UpdateSeries(seriesID, series);

            // put out information
            Information(string.Format("Series: #{0} ID {1} TVDB {2} {3}", seriesCount, seriesID, series.TVDB_ID, series.SeriesName));

            // we may as well load banners, persons and personinstances now
            if (mDoPerson)
            {
                foreach (SeriesPerson sp in persons)
                {
                    AddPersonInstance(sp.Name, sp.Role, sp.SortOrder, sp.Image, seriesID, -1, "", PersonInstance.MAIN_ACTOR);
                }
            }

            if (mDoBanner)
            {
                // we may as well load banners, Persons and Personinstances now
                foreach (Banner b in banners)
                {
                    CheckAndAddBanner(b.TVDB_ID, out bannerID, out matched);
                    if (bannerID != -1)
                    {
                        b.id = bannerID;
                        // overwrite contents
                        UpdateBanner(bannerID, b);
                    }
                }
            }

            if (mDoGenre)
            {
                foreach (Genre g in genres)
                {
                    CheckAndAddGenre(g.Name, out genreID, out matched);
                    if (genreID != -1)
                    {
                        g.id = genreID;
                        GenreInstance gi = new GenreInstance();
                        gi.GenreID = g.id;
                        gi.SeriesID = seriesID;
                        CheckAndAddGenreInstance(gi.GenreID, gi.SeriesID, out genreInstanceID, out matched);
                        if (genreInstanceID != -1)
                        {
                            gi.id = genreInstanceID;
                            if (mDoGenre)
                                // overwrite contents
                                UpdateGenreInstance(genreInstanceID, gi);
                        }
                    }
                }
            }

            // do episodes last
            foreach (Episode ep in episodes)
            {
                ep.SeriesID = seriesID;
                ep.Banner = series.Banner;

                CheckAndAddEpisode(ep.TVDB_ID, out episodeID, out matched);

                if (episodeID != -1)
                {
                    ep.id = episodeID;

                    if (mDoEpisode)
                        UpdateEpisode(episodeID, ep);

                    if (mDoPerson)
                    {
                        // update guest stars as persons
                        if (ep.GuestStars != "Unknown")
                        {
                            string[] guestStars = ep.GuestStars.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string s in guestStars)
                            {
                                AddPersonInstance(s, "Guest Artist", 0, "", seriesID, episodeID, ep.CombinedName, PersonInstance.GUEST_STAR);
                            }
                        }

                        // update writers as persons
                        if (ep.Writer != "Unknown")
                        {
                            string[] writers = ep.Writer.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string s in writers)
                            {
                                AddPersonInstance(s, "Writer", 0, "", seriesID, episodeID, ep.CombinedName, PersonInstance.WRITER);
                            }
                        }

                        // update directors as persons
                        if (ep.Director != "Unknown")
                        {
                            string[] directors = ep.Director.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string s in directors)
                            {
                                AddPersonInstance(s, "Director", 0, "", seriesID, episodeID, ep.CombinedName, PersonInstance.DIRECTOR);
                            }
                        }
                    }
                }
            }
        }

        private void ProcessUpdateSeries(string unpackDirectory, int count)
        {
            bool matched = false;
            int seriesID;
            int genreID;
            int genreInstanceID;
            XmlDocument doc = null;
            List<Genre> genres = new List<Genre>();
            List<SeriesPerson> persons = new List<SeriesPerson>();

            try
            {
                string file = unpackDirectory + "\\en.xml";
                if (File.Exists(file))
                {
                    doc = new XmlDocument();                             // Create an XML document object
                    doc.Load(file);                                      // Load the XML document from the specified file
                }
            }
            catch
            {
            }

            if (doc == null)
                return;

            Series series = Series.CreateDefaults();
 
            // reparse into lists (no SQL here pure XML crunching)
            ParseSeries(doc, series, genres);

            // use the TVDB as an ID
            CheckAndAddSeries(series.TVDB_ID, out seriesID, out matched);

            // something terrible has happened, ignore entire series
            if (seriesID == -1)
                return;

            series.id = seriesID;

            if (mDoSeries)
                // overwrite contents
                UpdateSeries(seriesID, series);

            // the update has actors in the series (not a seperate Actors file)
            if (mDoPerson)
            {
                if (!string.IsNullOrEmpty(series.Actors) && series.Actors != "||")
                {
                    string[] mainCast = series.Actors.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in mainCast)
                    {
                        AddPersonInstance(s, "", 0, "", seriesID, -1, "", PersonInstance.MAIN_ACTOR);
                    }
                }
            }

            if (mDoGenre)
            {
                foreach (Genre g in genres)
                {
                    CheckAndAddGenre(g.Name, out genreID, out matched);
                    if (genreID != -1)
                    {
                        g.id = genreID;
                        GenreInstance gi = new GenreInstance();
                        gi.GenreID = g.id;
                        gi.SeriesID = seriesID;
                        CheckAndAddGenreInstance(gi.GenreID, gi.SeriesID, out genreInstanceID, out matched);
                        if (genreInstanceID != -1)
                        {
                            gi.id = genreInstanceID;
                            if (mDoGenre)
                                // overwrite contents
                                UpdateGenreInstance(genreInstanceID, gi);
                        }
                    }
                }
            }

            // put out information
            Information(string.Format("Series: #{0} ID {1} TVDB {2} {3}", count, seriesID, series.TVDB_ID, series.SeriesName));

        }

        private void ProcessUpdateEpisode(string unpackDirectory, int count)
        {
            bool matched = false;
            int seriesID = -1;
            int episodeID = -1;
            XmlDocument doc = null;
            List<Episode> episodes = new List<Episode>();

            try
            {
                string file = unpackDirectory + "\\en.xml";
                if (File.Exists(file))
                {
                    doc = new XmlDocument();                             // Create an XML document object
                    doc.Load(file);                                      // Load the XML document from the specified file
                }
            }
            catch
            {
            }

            if (doc == null)
                return;
 
            if (mDoEpisode)
            {
                // reparse into lists (no SQL here pure XML crunching)
                ParseEpisode(doc, null, episodes);

                // do episodes last
                foreach (Episode ep in episodes)
                {

                    // get information about series
                    List<Series> matches = MatchSeries(ep.SeriesID, true);
                    if (matches == null)
                        continue;
                    if (matches.Count != 1)
                        continue;

                    ep.SeriesID = matches[0].id;
                    ep.Banner = matches[0].Banner;

                    CheckAndAddEpisode(ep.TVDB_ID, out episodeID, out matched);

                    if (episodeID != -1)
                    {
                        ep.id = episodeID;

                        if (mDoEpisode)
                            UpdateEpisode(episodeID, ep);

                        if (mDoPerson)
                        {
                            // update guest stars as persons
                            if (ep.GuestStars != "Unknown")
                            {
                                string[] guestStars = ep.GuestStars.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string s in guestStars)
                                {
                                    AddPersonInstance(s, "", 0, "", seriesID, episodeID, ep.CombinedName, PersonInstance.GUEST_STAR);
                                }
                            }

                            // update writers as persons
                            if (ep.Writer != "Unknown")
                            {
                                string[] writers = ep.Writer.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string s in writers)
                                {
                                    AddPersonInstance(s, "", 0, "", seriesID, episodeID, ep.CombinedName, PersonInstance.WRITER);
                                }
                            }

                            // update directors as persons
                            if (ep.Director != "Unknown")
                            {
                                string[] directors = ep.Director.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string s in directors)
                                {
                                    AddPersonInstance(s, "", 0, "", seriesID, episodeID, ep.CombinedName, PersonInstance.DIRECTOR);
                                }
                            }
                        }
                    }

                    // put out information
                    Information(string.Format("Episode: #{0} ID {1} TVDB {2}", count, episodeID, ep.TVDB_ID));

                }
            }
        }

        private void AddPersonInstance(string name, string role, int sortOrder, string image, int seriesID, int episodeID, string combinedName, int castType)
        {
            bool matched;
            int personID = -1;
            string personImage = "";
            int personInstanceID = -1;
            string personInstanceRole = "";

            if (name == null)
                return;         // nothing we can do

            if (image == null)
                image = "";

            if (role == null)
                role = "";

            if (combinedName == null)
                combinedName = "";

            Person person = new Person()
            {
                Name = name.Trim(),
                Image = image,
            };
            CheckAndAddPerson(person.Name, out personID, out personImage, out matched);
            if (personID != -1)
            {
                person.id = personID;
                if (person.Image == "")
                {
                    // use the image stored with the person in preference
                    if (!string.IsNullOrEmpty(personImage))
                        person.Image = personImage;
                }
                // try last attempt IMDB seach for Image
                //if (person.Image == "")
                    //LoadIMDBSearchItems(string.Format(URI_IMDB_JSON_NAME, person.Name), person);
                // no update the person
                UpdatePerson(personID, person);
                PersonInstance pi = new PersonInstance()
                {
                    PersonID = personID,
                    SortOrder = sortOrder,
                    CastType = castType,
                    SeriesID = seriesID,
                    Role = role,               
                    EpisodeID = episodeID,          // episode ID
                    PersonImage = person.Image,           // image
                    CombinedName = combinedName,
                };
                CheckAndAddPersonInstance(pi.PersonID, pi.SeriesID, pi.EpisodeID, pi.CastType, out personInstanceID, out personInstanceRole, out matched);
                if (personInstanceID != -1)
                {
                    // don't overwrite the person instance role
                    if (role == "" && personInstanceRole != "")
                        pi.Role = personInstanceRole;
                    pi.id = personInstanceID;
                    UpdatePersonInstance(personInstanceID, pi);
                }
            }
        }

        #region page parsers

        private void ParseRootPage(HtmlWeb webClient, string page)
        {
            string URI = null;

            try
            {
                // cache page to disk
                HtmlAgilityPack.HtmlDocument doc = CachePage(webClient, page + "/letters/09");
                Message("Fetching " + page + "/letters/09" + " from web\r");

                HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//ul[@class='pagination']//a[@href]");

                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        switch (node.Name)
                        {
                            case "a":
                                if (node.Attributes["href"] != null)        // its a link ('a' href attribute)
                                {
                                    URI = node.Attributes["href"].Value;
                                    if (!URI.Contains("latest"))
                                        ParseLetterPage(webClient, page, URI);
                                }
                                break;
                        }
                    }
                }
            }
            catch
            {
                mErrors++;
                Message("Site " + page + "/letters/A unreachable\r");
            }
            finally
            {
            }
        }

        private void ParseLetterPage(HtmlWeb webClient, string root, string page)
        {
            string name = null;
            string URI = null;

            try
            {
                // cache page
                HtmlAgilityPack.HtmlDocument doc = CachePage(webClient, root + page);
                Message("Fetching " + root + page + " from web\r");

                // collect href attribute from 'a' node within a div with attribute class equal to divlistglobal
                // collect style attribute from 'p' node within a div with attribute class equal to divlistglobal
                HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//ul[@class='listings']//a[@href] | //ul[@class='listings']//span");
                List<Series> seriesList;

                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        switch (node.Name)
                        {
                            case "a":
                                if (node.Attributes["href"] != null)        // its a link ('a' href attribute)
                                {
                                    URI = node.Attributes["href"].Value;
                                }
                                if (node.Attributes["title"] != null)        // its a link ('a' href attribute)
                                {
                                    name = node.Attributes["title"].Value;
                                }
                                break;
                            case "span":
                                GetSeriesFromTVDBAndStoreInCache(webClient, name, MATCH_LIMIT, out seriesList);
                                break;
                        }
                    }
                }

            }
            catch
            {
                mErrors++;
                Message("Site " + page + " unreachable\r");
            }
            finally
            {
            }
        }

        private void GetSeriesFromTVDBAndStoreInCache(HtmlWeb webClient, string seriesName, int limit, out List<Series> seriesList)
        {
            seriesList = null;

            try
            {
                seriesName = ExceptionSeriesName(seriesName);

                TVDBGetSeries(webClient, seriesName, limit, out seriesList);

                if (seriesList.Count == 0) // TVDB could not identify it - ignore it
                    return;

                if (seriesList != null)
                {
                    foreach (Series series in seriesList)
                    {
                        if (mSeriesCount % 10 == 0)
                            SetDiagnostics(mSeriesCount);
                        // put out information
                        string fileName = String.Format("{0}\\{1}.zip", CACHEDIR, series.TVDB_ID);
                        if (!File.Exists(fileName))
                        {
                            Message(string.Format("{0}\r", fileName));
                            Information(string.Format("Downloading Zip: #{0} {1} {2}", ++mSeriesCount, series.TVDB_ID, series.SeriesName));
                            string URI = string.Format("{0}/api/{1}/series/{2}/all/en.zip", TVDB, API, series.TVDB_ID);
                            CachePageZip(URI, fileName);
                        }
                    }
                }
            }
            catch
            {
                mErrors++;
                Message("TVDB For " + seriesName + " unreachable\r");
            }
            finally
            {
            }
  
        }

        // one off lookup
        private string ExceptionSeriesName(string seriesName)
        {
            if (seriesName.Contains("My Dad Says"))
                return "My Dad Says";
            else
                return seriesName;
        }

        private string CommaCharacters(string name)
        {
            string[] tokens = name.Split(new char[] { '|', ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < tokens.Length; i++)
            {
                // check for JARDIN,JIM format and change to JIM JARDIN
                if (tokens[i].Contains(','))
                {
                    string[] subTokens = tokens[i].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (subTokens.Length == 2)
                    {
                        string temp = subTokens[0];
                        subTokens[0] = subTokens[1];
                        subTokens[1] = temp;
                        tokens[i] = String.Join(" ", subTokens);
                    }
                    else
                        tokens[i] = tokens[i].Replace(',', ' ');
                }
            }
            return String.Join(",", tokens);
        }

        public string MakeValidFileName(string fileName)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        #endregion

        #region general SQL helpers

        private void GetInsertCounts()
        {
            // get unique id's for any possible new entries
            mSeriesID = GetMaxCount("Series") + 1;
            mEpisodeID = GetMaxCount("Episode") + 1;
            mGenreID = GetMaxCount("Genre") + 1;
            mPersonInstanceID = GetMaxCount("PersonInstance") + 1;
            mGenreInstanceID = GetMaxCount("GenreInstance") + 1;
            mPersonID = GetMaxCount("Person") + 1;
            mBannerID = GetMaxCount("Banner") + 1;
        }

        private void DeleteTableContents()
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            foreach (string table in mTables)
            {
                try
                {
                    conn = new SqlConnection(mConnStringBuilder.ToString());
                    conn.Open();
                    command = conn.CreateCommand();
                    command.CommandText = String.Format("DELETE FROM TVSeries.{0}", table);
                    int rowsDeleted = command.ExecuteNonQuery();
                    if (rowsDeleted > 0)
                        Message(String.Format("Deleted {0} rows from {1}\r", rowsDeleted, table));
                }
                catch
                {
                }
                finally
                {
                    if (conn != null)
                        conn.Close();
                }
            }
        }

        private int GetMaxCount(string tableName)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            int c = 0;
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.CommandText = string.Format("SELECT MAX(id) AS Count FROM TVSeries.{0}", tableName);
                SqlDataReader rdr = command.ExecuteReader();
                while (rdr.Read())
                {
                    c = (int)rdr["Count"];
                }
                rdr.Close();
            }
            catch (Exception)
            {
                c = -1;
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return c;
        }

        private int RemoveSeries(int id)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            int rowsAffected = 0;
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.Parameters.Clear();
                command.CommandText = "DELETE TVSeries.Series " +
                    "WHERE id=@ID";
                command.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                command.Parameters["@ID"].Value = id;
                rowsAffected = command.ExecuteNonQuery();
            }
            catch
            {
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return rowsAffected;
        }

        private int RemoveSeriesEpisodes(int id)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            int rowsAffected = 0;
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.Parameters.Clear();
                command.CommandText = "DELETE TVSeries.Episode " +
                    "WHERE SeriesID=@SeriesID";
                command.Parameters.Add("@SeriesID", System.Data.SqlDbType.Int);
                command.Parameters["@SeriesID"].Value = id;
                rowsAffected = command.ExecuteNonQuery();
            }
            catch
            {
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return rowsAffected;
        }

        private int SetMaintainenceMode(long timeStamp, int mode, DateTime dateTime)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            int rowsAffected = 0;
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.Parameters.Clear();
                command.CommandText = "UPDATE TVSeries.Administration " +
                    "SET Mode=@Mode,Timestamp=@Timestamp,LastUpdated=@LastUpdated " +
                    "WHERE id=0";
                command.Parameters.Add("@Mode", System.Data.SqlDbType.Int);
                command.Parameters.Add("@Timestamp", System.Data.SqlDbType.BigInt);
                command.Parameters.Add("@LastUpdated", System.Data.SqlDbType.DateTime);
                command.Parameters["@Timestamp"].Value = timeStamp;
                command.Parameters["@Mode"].Value = mode;
                command.Parameters["@LastUpdated"].Value = dateTime;
                rowsAffected = command.ExecuteNonQuery();
            }
            catch
            {
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return rowsAffected;
        }

        private int SetDiagnostics(int counter)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            int rowsAffected = 0;
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.Parameters.Clear();
                command.CommandText = "UPDATE TVSeries.Administration " +
                    "SET Mode=@Mode,Counter=@Counter " +
                    "WHERE id=0";
                command.Parameters.Add("@Mode", System.Data.SqlDbType.Int);
                command.Parameters.Add("@Counter", System.Data.SqlDbType.Int);
                command.Parameters["@Mode"].Value = 1; // set updating status
                command.Parameters["@Counter"].Value = counter;
                rowsAffected = command.ExecuteNonQuery();
            }
            catch
            {
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return rowsAffected;
        }
        private long GetAdministrationTimeStamp()
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            string error = null;
            long timeStamp = -1;
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.CommandText = string.Format("SELECT Timestamp FROM TVSeries.Administration");
                SqlDataReader rdr = command.ExecuteReader();
                while (rdr.Read())
                {
                    timeStamp = ((long)rdr["Timestamp"]);
                }
                rdr.Close();
                return timeStamp;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return -1;
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }
        #endregion

        #region Genre SQL helpers

        private int CheckAndAddGenre(string name, out int ID, out bool matched)
        {
            ID = -1;
            int v = 0;
            matched = false;
            List<int> matches = MatchGenre(name);
            if (matches != null && matches.Count == 1)
            {
                ID = matches[0];
                matched = true;
            }
            else
                v = AddGenre(name, out ID);
            return v;
        }

        private List<int> MatchGenre(string name)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            string error = null;
            List<int> rows = new List<int>();
            try
            {
                name = name.Replace("'", "''");
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.CommandText = string.Format("SELECT Genre.id AS GenreID FROM TVSeries.Genre WHERE (Name LIKE '{0}')", name);
                SqlDataReader rdr = command.ExecuteReader();
                while (rdr.Read())
                {
                    rows.Add((int)rdr["GenreID"]);
                }
                rdr.Close();
                return rows;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        private int AddGenre(string name, out int ID)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            int rowsAffected = 0;
            ID = mGenreID++; 
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO TVSeries.Genre(id,Name)" +
                    " values(@id,@Name)";
                command.Parameters.Add("@id", System.Data.SqlDbType.Int, 4);
                command.Parameters.Add("@Name", System.Data.SqlDbType.VarChar);
                command.Parameters["@id"].Value = (int)ID;
                command.Parameters["@Name"].Value = name;
                rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 1 && mVerbose)
                    Message(String.Format("Added '{0}' to Genre\r", name));
            }
            catch
            {
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return rowsAffected;
        }

        #endregion

        #region Series SQL helpers

        private int CheckAndAddSeries(int TVDB_ID, out int ID, out bool matched)
        {
            ID = -1;
            int v = 0;
            matched = false;
            List<int> matches = MatchSeries(TVDB_ID);
            if (matches != null && matches.Count == 1)
            {
                ID = matches[0];
                matched = true;
            }
            else
                v = AddSeries(out ID);
            return v;
        }

        private List<int> MatchSeries(int TVDB_ID)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            string error = null;
            List<int> rows = new List<int>();
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.CommandText = string.Format("SELECT Series.id AS SeriesID FROM TVSeries.Series WHERE (TVDB_ID = {0})", TVDB_ID);
                SqlDataReader rdr = command.ExecuteReader();
                while (rdr.Read())
                {
                    rows.Add((int)rdr["SeriesID"]);
                }
                rdr.Close();
                return rows;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        private List<Series> MatchSeries(int TVDB_ID, bool getBanner)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            string error = null;
            List<Series> rows = new List<Series>();
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.CommandText = string.Format("SELECT Series.id AS SeriesID, Banner FROM TVSeries.Series WHERE (TVDB_ID = {0})", TVDB_ID);
                SqlDataReader rdr = command.ExecuteReader();
                while (rdr.Read())
                {
                    rows.Add(
                        new Series()
                        {
                            id = (int)rdr["SeriesID"],
                            Banner = (string)rdr["Banner"],
                        });
                }
                rdr.Close();
                return rows;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        private int AddSeries(out int ID)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            int rowsAffected = 0;
            ID = mSeriesID++;
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO TVSeries.Series(id)" +
                    " values(@id)";
                command.Parameters.Add("@id", System.Data.SqlDbType.Int, 4);
                command.Parameters["@id"].Value = ID;
                rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 1 && mVerbose)
                    Message(String.Format("Added '{0}' to Series\r", ID));
            }
            catch
            {
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return rowsAffected;
        }

        private int UpdateSeries(int ID, Series series)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            int rowsAffected = 0;
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.Parameters.Clear();
                command.CommandText = "UPDATE TVSeries.Series " +
                    "SET FirstAired=@FirstAired,Overview=@Overview,AlphaKey=@AlphaKey,IMDB_ID=@IMDB_ID,Banner=@Banner,FanArt=@FanArt,TVDB_ID=@TVDB_ID,SeriesName=@SeriesName,Runtime=@Runtime,Status=@Status,Rating=@Rating,Network=@Network,Poster=@Poster,ContentRating=@ContentRating,Zap2it_ID=@Zap2it_ID  " +
                    "WHERE id=@id";
                command.Parameters.Add("@id", System.Data.SqlDbType.Int, 4);
                command.Parameters.Add("@FirstAired", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@TVDB_ID", System.Data.SqlDbType.Int, 4);
                command.Parameters.Add("@Overview", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@AlphaKey", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@IMDB_ID", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@Banner", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@FanArt", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@SeriesName", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@Runtime", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@Status", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@Rating", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@Network", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@Poster", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@ContentRating", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@Zap2it_ID", System.Data.SqlDbType.VarChar);
                command.Parameters["@id"].Value = ID;
                command.Parameters["@FirstAired"].Value = series.FirstAired;
                command.Parameters["@Overview"].Value = series.Overview;
                command.Parameters["@AlphaKey"].Value = series.AlphaKey;
                command.Parameters["@IMDB_ID"].Value = series.IMDB_ID;
                command.Parameters["@Banner"].Value = series.Banner;
                command.Parameters["@Fanart"].Value = series.FanArt;
                command.Parameters["@SeriesName"].Value = series.SeriesName;
                command.Parameters["@TVDB_ID"].Value = series.TVDB_ID;
                command.Parameters["@Runtime"].Value = series.Runtime;
                command.Parameters["@Status"].Value = series.Status;
                command.Parameters["@Rating"].Value = series.Rating;
                command.Parameters["@Network"].Value = series.Network;
                command.Parameters["@Poster"].Value = series.Poster;
                command.Parameters["@ContentRating"].Value = series.ContentRating;
                command.Parameters["@Zap2it_ID"].Value = series.Zap2it_ID;
                rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 1 && mVerbose)
                    Message(String.Format("Updated Series '{0}'\r", series.id));
            }
            catch
            {
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return rowsAffected;
        }

        #endregion

        #region Episode SQL helpers

        private int CheckAndAddEpisode(int TVDB_ID, out int ID, out bool matched)
        {
            ID = -1;
            int v = 0;
            matched = false;
            List<int> matches = MatchEpisode(TVDB_ID);
            if (matches != null && matches.Count == 1)
            {
                ID = matches[0];
                matched = true;
            }
            else
                v = AddEpisode(out ID);
            return v;
        }

        private List<int> MatchEpisode(int TVDB_ID)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            string error = null;
            List<int> rows = new List<int>();
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.CommandText = string.Format("SELECT Episode.id AS EpisodeID FROM TVSeries.Episode WHERE Episode.TVDB_ID = {0}", TVDB_ID);
                SqlDataReader rdr = command.ExecuteReader();
                while (rdr.Read())
                {
                    rows.Add((int)rdr["EpisodeID"]);
                }
                rdr.Close();
                return rows;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        private int AddEpisode(out int ID)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            int rowsAffected = 0;
            ID = mEpisodeID++;
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO TVSeries.Episode(id)" +
                    " values(@id)";
                command.Parameters.Add("@id", System.Data.SqlDbType.Int, 4);
                command.Parameters["@id"].Value = ID;
                 rowsAffected = command.ExecuteNonQuery();
                 if (rowsAffected == 1 && mVerbose)
                    Message(String.Format("Added '{0}' to Episode\r", ID));
            }
            catch
            {
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return rowsAffected;
        }

        private int UpdateEpisode(int ID, Episode episode)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            int rowsAffected = 0;
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.Parameters.Clear();
                command.CommandText = "UPDATE TVSeries.Episode " +
                     "SET SeriesID=@SeriesID,Overview=@Overview,FirstAired=@FirstAired,Code=@Code,IMDB_ID=@IMDB_ID,SeasonNum=@SeasonNum,EpisodeNum=@EpisodeNum,GuestStars=@GuestStars,Writer=@Writer,Director=@Director,FileName=@FileName,Banner=@Banner,TVDB_ID=@TVDB_ID,Rating=@Rating,EpisodeName=@EpisodeName " +
                     "WHERE id=@id";
                command.Parameters.Add("@id", System.Data.SqlDbType.Int, 4);
                command.Parameters.Add("@SeriesID", System.Data.SqlDbType.Int, 4);
                command.Parameters.Add("@Overview", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@FirstAired", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@Code", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@IMDB_ID", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@SeasonNum", System.Data.SqlDbType.Int, 4);
                command.Parameters.Add("@EpisodeNum", System.Data.SqlDbType.Int, 4);
                command.Parameters.Add("@GuestStars", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@Writer", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@Director", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@FileName", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@Banner", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@TVDB_ID", System.Data.SqlDbType.Int, 4);
                command.Parameters.Add("@Rating", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@EpisodeName", System.Data.SqlDbType.VarChar);
                command.Parameters["@id"].Value = ID;
                command.Parameters["@SeriesID"].Value = episode.SeriesID;
                command.Parameters["@Overview"].Value = episode.Overview;
                command.Parameters["@FirstAired"].Value = episode.FirstAired;
                command.Parameters["@Code"].Value = episode.Code;
                command.Parameters["@IMDB_ID"].Value = episode.IMDB_ID;
                command.Parameters["@SeasonNum"].Value = episode.SeasonNum;
                command.Parameters["@EpisodeNum"].Value = episode.EpisodeNum;
                command.Parameters["@GuestStars"].Value = episode.GuestStars;
                command.Parameters["@Writer"].Value = episode.Writer;
                command.Parameters["@Director"].Value = episode.Director;
                command.Parameters["@FileName"].Value = episode.FileName;
                command.Parameters["@Banner"].Value = episode.Banner;
                command.Parameters["@TVDB_ID"].Value = episode.TVDB_ID;
                command.Parameters["@Rating"].Value = episode.Rating;
                command.Parameters["@EpisodeName"].Value = episode.EpisodeName;
                rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 1 && mVerbose)
                    Message(String.Format("Updated Episode '{0}'\r", episode.id));
            }
            catch
            {
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return rowsAffected;
        }

        #endregion

        #region Banner SQL helpers

        private int CheckAndAddBanner(int bannerID, out int ID, out bool matched)
        {
            ID = -1;
            int v = 0;
            matched = false;
            List<int> matches = MatchBanner(bannerID);
            if (matches != null && matches.Count == 1)
            {
                ID = matches[0];
                matched = true;
            }
            else
                v = AddBanner(out ID);
            return v;
        }

        private List<int> MatchBanner(int bannerID)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            string error = null;
            List<int> rows = new List<int>();
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.CommandText = string.Format("SELECT Banner.id AS BannerID FROM TVSeries.Banner WHERE (Banner.TVDB_ID = {0})", bannerID);
                SqlDataReader rdr = command.ExecuteReader();
                while (rdr.Read())
                {
                    rows.Add((int)rdr["BannerID"]);
                }
                rdr.Close();
                return rows;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        private int AddBanner(out int ID)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            int rowsAffected = 0;
            ID = mBannerID++;
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO TVSeries.Banner(id)" +
                    " values(@id)";
                command.Parameters.Add("@id", System.Data.SqlDbType.Int, 4);
                command.Parameters["@id"].Value = ID;
                rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 1 && mVerbose)
                    Message(String.Format("Added '{0}' to Banner\r", ID));
            }
            catch
            {
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return rowsAffected;
        }

        private int UpdateBanner(int ID, Banner banner)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            int rowsAffected = 0;
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.Parameters.Clear();
                command.CommandText = "UPDATE TVSeries.Banner " +
                     "SET SeriesID=@SeriesID,ThumbnailPath=@ThumbnailPath,BannerPath=@BannerPath,BannerType=@BannerType,BannerType2=@BannerType2,TVDB_ID=@TVDB_ID " +
                     "WHERE id=@id";
                command.Parameters.Add("@id", System.Data.SqlDbType.Int, 4);
                command.Parameters.Add("@SeriesID", System.Data.SqlDbType.Int, 4);
                command.Parameters.Add("@ThumbnailPath", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@BannerPath", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@BannerType", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@BannerType2", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@TVDB_ID", System.Data.SqlDbType.Int, 4);
                command.Parameters["@id"].Value = ID;
                command.Parameters["@SeriesID"].Value = banner.SeriesID;
                command.Parameters["@ThumbnailPath"].Value = banner.ThumbnailPath;
                command.Parameters["@BannerPath"].Value = banner.BannerPath;
                command.Parameters["@BannerType"].Value = banner.BannerType;
                command.Parameters["@BannerType2"].Value = banner.BannerType2;
                command.Parameters["@TVDB_ID"].Value = banner.TVDB_ID;
                rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 1 && mVerbose)
                    Message(String.Format("Updated Banner '{0}'\r", banner.id));
            }
            catch
            {
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return rowsAffected;
        }

        #endregion

        #region Person SQL helpers

        private int CheckAndAddPerson(string name, out int ID, out string image, out bool matched)
        {
            ID = -1;
            image = "";
            int v = 0;
            matched = false;
            List<Person> matches = MatchPerson(name);
            if (matches != null && matches.Count == 1)
            {
                ID = matches[0].id;
                image = matches[0].Image;
                matched = true;
            }
            else
                v = AddPerson(out ID);
            return v;
        }

        private List<Person> MatchPerson(string name)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            string error = null;
            List<Person> rows = new List<Person>();
            try
            {
                name = name.Replace("'", "''");
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.CommandText = string.Format("SELECT Person.id AS PersonID, Person.Image AS Image FROM TVSeries.Person WHERE (Person.Name LIKE '{0}')", name);
                SqlDataReader rdr = command.ExecuteReader();
                while (rdr.Read())
                {
                    rows.Add(
                       new Person
                       {
                           id = (int)rdr["PersonID"],
                           Image = (string)rdr["Image"],
                       });
                }
                rdr.Close();
                return rows;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        private List<Person> MatchPersonNoImage()
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            string error = null;
            List<Person> rows = new List<Person>();
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.CommandText = string.Format("SELECT Person.id AS PersonID, Person.Image AS Image,Person.Name as Name FROM TVSeries.Person WHERE (Person.Image = '')");
                SqlDataReader rdr = command.ExecuteReader();
                while (rdr.Read())
                {
                    rows.Add(
                       new Person
                       {
                           id = (int)rdr["PersonID"],
                           Name = (string)rdr["Name"],
                           Image = (string)rdr["Image"],
                       });
                }
                rdr.Close();
                return rows;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        private int AddPerson(out int ID)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            int rowsAffected = 0;
            ID = mPersonID++;
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO TVSeries.Person(id)" +
                    " values(@id)";
                command.Parameters.Add("@id", System.Data.SqlDbType.Int, 4);
                command.Parameters["@id"].Value = ID;
                rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 1 && mVerbose)
                    Message(String.Format("Added '{0}' to Person\r", ID));
            }
            catch
            {
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return rowsAffected;
        }

        private int UpdatePerson(int ID, Person Person)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            int rowsAffected = 0;
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.Parameters.Clear();
                command.CommandText = "UPDATE TVSeries.Person " +
                     "SET Image=@Image,Name=@Name " +
                     "WHERE id=@id";
                command.Parameters.Add("@id", System.Data.SqlDbType.Int, 4);
                command.Parameters.Add("@Image", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@Name", System.Data.SqlDbType.VarChar);
                command.Parameters["@id"].Value = ID;
                command.Parameters["@Image"].Value = Person.Image;
                command.Parameters["@Name"].Value = Person.Name;
                rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 1 && mVerbose)
                    Message(String.Format("Updated Person '{0}'\r", Person.id));
            }
            catch
            {
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return rowsAffected;
        }

        #endregion

        #region PersonInstance SQL helpers

        private int CheckAndAddPersonInstance(int personID, int seriesID, int episodeID, int castType, out int ID, out string role, out bool matched)
        {
            ID = -1;
            role = "";
            int v = 0;
            matched = false;
            List<PersonInstance> matches = MatchPersonInstance(personID, seriesID, episodeID, castType);
            if (matches != null && matches.Count == 1)
            {
                ID = matches[0].id;
                role = matches[0].Role;
                matched = true;
            }
            else
                v = AddPersonInstance(out ID);
            return v;
        }

        private List<PersonInstance> MatchPersonInstance(int personID, int seriesID, int episodeID, int castType)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            string error = null;
            List<PersonInstance> rows = new List<PersonInstance>();
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.CommandText = string.Format("SELECT PersonInstance.id AS PersonInstanceID,PersonInstance.Role AS Role FROM TVSeries.PersonInstance WHERE (PersonInstance.PersonID = {0}) AND (PersonInstance.SeriesID = {1}) AND (PersonInstance.EpisodeID = {2}) AND (PersonInstance.CastType = {3})", personID, seriesID, episodeID, castType);
                SqlDataReader rdr = command.ExecuteReader();
                while (rdr.Read())
                {
                    PersonInstance pi = new PersonInstance();
                    pi.id = (int)rdr["PersonInstanceID"];
                    pi.Role = (string)rdr["Role"];
                    rows.Add(pi);
                }
                rdr.Close();
                return rows;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        private int AddPersonInstance(out int ID)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            int rowsAffected = 0;
            ID = mPersonInstanceID++; // unique
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO TVSeries.PersonInstance(id)" +
                    " values(@id)";
                command.Parameters.Add("@id", System.Data.SqlDbType.Int, 4);
                command.Parameters["@id"].Value = ID;
                rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 1 && mVerbose)
                    Message(String.Format("Added '{0}' to PersonInstance\r", ID));
            }
            catch
            {
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return rowsAffected;
        }

        private int UpdatePersonInstance(int ID, PersonInstance PersonInstance)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            int rowsAffected = 0;
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.Parameters.Clear();
                command.CommandText = "UPDATE TVSeries.PersonInstance " +
                     "SET PersonID=@PersonID,SeriesID=@SeriesID,EpisodeID=@EpisodeID,Role=@Role,SortOrder=@SortOrder,Image=@Image,CastType=@CastType,CombinedName=@CombinedName " +
                     "WHERE id=@id";
                command.Parameters.Add("@id", System.Data.SqlDbType.Int);
                command.Parameters.Add("@PersonID", System.Data.SqlDbType.Int);
                command.Parameters.Add("@SeriesID", System.Data.SqlDbType.Int);
                command.Parameters.Add("@EpisodeID", System.Data.SqlDbType.Int);
                command.Parameters.Add("@Role", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@SortOrder", System.Data.SqlDbType.Int);
                command.Parameters.Add("@Image", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@CombinedName", System.Data.SqlDbType.VarChar);
                command.Parameters.Add("@CastType", System.Data.SqlDbType.Int);
                command.Parameters["@id"].Value = ID;
                command.Parameters["@PersonID"].Value = PersonInstance.PersonID;
                command.Parameters["@SeriesID"].Value = PersonInstance.SeriesID;
                command.Parameters["@EpisodeID"].Value = PersonInstance.EpisodeID;
                command.Parameters["@Role"].Value = PersonInstance.Role;
                command.Parameters["@SortOrder"].Value = PersonInstance.SortOrder;
                command.Parameters["@Image"].Value = PersonInstance.PersonImage;
                command.Parameters["@CombinedName"].Value = PersonInstance.CombinedName;
                command.Parameters["@CastType"].Value = PersonInstance.CastType;
                rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 1 && mVerbose)
                    Message(String.Format("Updated PersonInstance '{0}'\r", PersonInstance.id));
            }
            catch
            {
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return rowsAffected;
        }

        #endregion

        #region GenreInstance SQL helpers

        private int CheckAndAddGenreInstance(int genreID, int seriesID, out int ID, out bool matched)
        {
            ID = -1;
            int v = 0;
            matched = false;
            List<int> matches = MatchGenreInstance(genreID, seriesID);
            if (matches != null && matches.Count == 1)
            {
                ID = matches[0];
                matched = true;
            }
            else
                v = AddGenreInstance(out ID);
            return v;
        }

        private List<int> MatchGenreInstance(int genreID, int seriesID)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            string error = null;
            List<int> rows = new List<int>();
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.CommandText = string.Format("SELECT GenreInstance.id AS GenreInstanceID FROM TVSeries.GenreInstance WHERE (GenreInstance.GenreID = '{0}') AND (GenreInstance.SeriesID = {1})", genreID, seriesID);
                SqlDataReader rdr = command.ExecuteReader();
                while (rdr.Read())
                {
                    rows.Add((int)rdr["GenreInstanceID"]);
                }
                rdr.Close();
                return rows;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        private int AddGenreInstance(out int ID)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            int rowsAffected = 0;
            ID = mGenreInstanceID++; // unique
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO TVSeries.GenreInstance(id)" +
                    " values(@id)";
                command.Parameters.Add("@id", System.Data.SqlDbType.Int, 4);
                command.Parameters["@id"].Value = ID;
                rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 1 && mVerbose)
                    Message(String.Format("Added '{0}' to GenreInstance\r", ID));
            }
            catch
            {
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return rowsAffected;
        }

        private int UpdateGenreInstance(int ID, GenreInstance genreInstance)
        {
            SqlConnection conn = null;
            SqlCommand command = null;
            int rowsAffected = 0;
            try
            {
                conn = new SqlConnection(mConnStringBuilder.ToString());
                conn.Open();
                command = conn.CreateCommand();
                command.Parameters.Clear();
                command.CommandText = "UPDATE TVSeries.GenreInstance " +
                     "SET GenreID=@GenreID,SeriesID=@SeriesID " +
                     "WHERE id=@id";
                command.Parameters.Add("@id", System.Data.SqlDbType.Int);
                command.Parameters.Add("@GenreID", System.Data.SqlDbType.Int);
                command.Parameters.Add("@SeriesID", System.Data.SqlDbType.Int);
                command.Parameters["@id"].Value = ID;
                command.Parameters["@GenreID"].Value = genreInstance.GenreID;
                command.Parameters["@SeriesID"].Value = genreInstance.SeriesID;
                rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 1 && mVerbose)
                    Message(String.Format("Updated GenreInstance '{0}'\r", genreInstance.id));
            }
            catch
            {
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return rowsAffected;
        }

        #endregion

        private void Message(string s)
        {
            this.mRichTextBox.BeginInvoke(new MethodInvoker(
            delegate()
            {
                mRichTextBox.AppendText(s);
                mRichTextBox.ScrollToCaret();
            }));
        }

        private void Information(string s)
        {
            this.mInformationTextBox.BeginInvoke(new MethodInvoker(
            delegate()
            {
                mInformationTextBox.Text = s;
            }));
        }

        private HtmlAgilityPack.HtmlDocument CachePage(HtmlWeb webClient, string page)
        {
            // search for the page in the cache
            string key = null;
            string[] tokens = page.Split(new char[] { '/' });
            string fileName = null;
            HtmlAgilityPack.HtmlDocument doc = null;

            // use last part of page path as key
            if (tokens.Length > 0)
            {
                key = tokens[tokens.Length - 1];
            }

            fileName = MakeValidFileName(key);
            fileName = CACHEDIR + "\\" + fileName;
 
            // load the page
            doc = webClient.Load(page);
            if (doc != null)
                doc.Save(fileName);

            return doc;
        }

        private XmlDocument CachePage(HtmlWeb webClient, string page, string fileName)
        {
            XmlDocument doc = null;
            string responseFromServer = MakeWebRequest(page);
            if (responseFromServer != null)
            {
                try
                {
                    doc = new XmlDocument();            // Create an XML document object
                    doc.LoadXml(responseFromServer);    // Load the XML document from the specified file
                    doc.Save(fileName);
                }
                catch
                {
                    doc = null;
                }
            }
            return doc;
        }

        private XmlDocument CachePage(string page)
        {
            // search for the page in the cache
            string key = null;
            string[] tokens = page.Split(new char[] { '/' });
            string fileName = null;
            XmlDocument doc = null;

            // use last part of page path as key
            if (tokens.Length > 0)
            {
                key = tokens[tokens.Length - 1];
            }

            fileName = MakeValidFileName(key);
            fileName = CACHEDIR + "\\" + fileName;

            string responseFromServer = MakeWebRequest(page);
            if (responseFromServer != null)
            {
                try
                {
                    doc = new XmlDocument();            // Create an XML document object
                    doc.LoadXml(responseFromServer);    // Load the XML document from the specified file
                    doc.Save(fileName);
                }
                catch
                {
                    doc = null;
                }
            }
   
            return doc;
        }

        private string MakeWebRequest(string page)
        {
            WebRequest request = WebRequest.Create(page);
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            if (((HttpWebResponse)response).StatusDescription == "OK")
            {
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                return responseFromServer;
            }
            return null;
        }

        private void CachePageZip(string page, string file)
        {
            // make request
            WebRequest request = WebRequest.Create(page);
            // Get the response.
            WebResponse response = request.GetResponse();
            // process the response.
            if (((HttpWebResponse)response).StatusDescription == "OK")
            {
                // Get the stream containing content returned by the server.
                FileStream fs = null;
                Stream ds = response.GetResponseStream();
                try
                {
                    fs = new FileStream(file, FileMode.OpenOrCreate);
                    ds.CopyTo(fs);
                }
                catch
                {
                }
                finally
                {
                    if (fs != null)
                        fs.Close();
                    if (ds != null)
                        ds.Close();
                }
            }
        }

        private void UnzipFile(string file)
        {
            var z = new Ionic.Zip.ZipFile(file);
            string zipToUnpack = file;
            string unpackDirectory = file + "Extracted";
            if (!Directory.Exists(unpackDirectory)) Directory.CreateDirectory(unpackDirectory);
            using (Ionic.Zip.ZipFile zip1 = Ionic.Zip.ZipFile.Read(zipToUnpack))
            {
                // here, we extract every entry, but we could extract conditionally
                // based on entry name, size, date, checkbox status, etc.  
                foreach (Ionic.Zip.ZipEntry e in zip1)
                {
                    e.Extract(unpackDirectory, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                }
            }
        }

        internal void Close()
        {
            if (mThread != null)
                mThread.Abort();
        }

        private void TVDBGetSeries(HtmlWeb webClient, string name, int limit, out List<Series> seriesList)
        {
            XmlDocument doc = null;
            seriesList = new List<Series>();
 
            try
            {
                doc = CachePage(TVDB + "/api/GetSeries.php?seriesname=" + name);
            }
            catch
            {
                seriesList = new List<Series>();            
                return;
            }
            if (doc == null)
                return;

            XmlNodeList items = doc.SelectNodes("//Series");

            if (items.Count > 1)
                Message(String.Format("Series '{0}' matched with {1} items\r", name, items.Count));

            int count = 0;
            // pass one parse, to get series id
            foreach (XmlNode node in items)
            {
                if (node.Name == "Series")
                {
                    count++;
                    if (count > limit)
                    {
                        Message("Skipping " + name + " - more than 25 matches\n");
                        return;     // if more than 25 matches give up
                    }

                    Series series = Series.CreateDefaults();
 
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        switch (childNode.Name)
                        {
                            case "id":
                                series.TVDB_ID = int.Parse(childNode.InnerText);
                                bool found = false;
                                // ensure we haveno duplicates
                                foreach (var s in seriesList)
                                {
                                    if (s.TVDB_ID == series.TVDB_ID)
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found)
                                    seriesList.Add(series);
                                break;
                            case "SeriesName":
                                series.SeriesName = childNode.InnerText;
                                break;
                        }
                    }
                }
            }
        }

        private void ParseSeries(XmlDocument doc, Series series, List<Genre> genres)
        {
            XmlNodeList Series = doc.SelectNodes("//Series");
            foreach (XmlNode node in Series)
            {
                if (node.Name == "Series")
                {
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        if (childNode.InnerText != "")
                        {
                            switch (childNode.Name)
                            {
                                case "id":
                                    series.TVDB_ID = int.Parse(childNode.InnerText);
                                    break;
                                case "IMDB_ID":
                                    series.IMDB_ID = IMDB + "/title/" + childNode.InnerText;
                                    break;
                                case "Overview":
                                    series.Overview = childNode.InnerText;
                                    break;
                                case "FirstAired":
                                    series.FirstAired = childNode.InnerText;
                                    break;
                                case "SeriesName":
                                    series.SeriesName = childNode.InnerText;
                                    // index alphakey
                                    series.AlphaKey = series.SeriesName.ToLower();
                                    series.AlphaKey = series.AlphaKey.Substring(0, 1);
                                    if (!char.IsLetter(series.AlphaKey[0]))
                                        series.AlphaKey = "*";
                                    break;
                                case "banner":
                                    series.Banner = TVDB + "/banners/" + childNode.InnerText;
                                    break;
                                case "fanart":
                                    series.FanArt = TVDB + "/banners/" + childNode.InnerText;
                                    break;
                                 case "Status":
                                    series.Status = childNode.InnerText;
                                    break;
                                case "Rating":
                                    series.Rating = childNode.InnerText;
                                    break;
                                case "Runtime":
                                    series.Runtime = childNode.InnerText;
                                    break;
                                case "Network":
                                    series.Network = childNode.InnerText;
                                    break;
                                case "Genre":
                                    {
                                        string[] genreStr = childNode.InnerText.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                        foreach (string genre in genreStr)
                                        {
                                            Genre g = new Genre();
                                            g.Name = genre.Trim();
                                            genres.Add(g);
                                        }
                                    }
                                    break;
                                case "Actors":
                                    series.Actors = childNode.InnerText;
                                    break;
                                case "poster":
                                    series.Poster = TVDB + "/banners/" + childNode.InnerText;
                                    break;
                                case "ContentRating":
                                    series.ContentRating = childNode.InnerText;
                                    break;
                                case "zap2it_id":
                                    series.Zap2it_ID = childNode.InnerText;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void ParseEpisode(XmlDocument doc, Series series, List<Episode> episodes)
        {
            XmlNodeList Episodes = doc.SelectNodes("//Episode");
            foreach (XmlNode node in Episodes)
            {
                if (node.Name == "Episode")
                {
                    Episode e = Episode.CreateDefaults();
                    if (series != null)
                        e.SeriesID = series.TVDB_ID;
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        if (childNode.InnerText != "")
                        {
                            switch (childNode.Name)
                            {
                                case "id":
                                    e.TVDB_ID = int.Parse(childNode.InnerText);
                                    break;
                                case "filename":
                                    e.FileName = TVDB + "/banners/" + childNode.InnerText;
                                    break;
                                case "EpisodeName":
                                    e.EpisodeName = childNode.InnerText;
                                    break;
                                case "Combined_season":
                                    e.SeasonNum = int.Parse(childNode.InnerText);
                                    break;
                                case "EpisodeNumber":
                                    e.EpisodeNum = int.Parse(childNode.InnerText);
                                    break;
                                case "FirstAired":
                                    e.FirstAired = childNode.InnerText;
                                    break;
                                case "Overview":
                                    e.Overview = childNode.InnerText;
                                    break;
                                case "GuestStars":
                                    e.GuestStars = CommaCharacters(childNode.InnerText); // standardise commas between entries
                                    break;
                                case "Writer":
                                    e.Writer = CommaCharacters(childNode.InnerText);    // standardise commas between entries
                                    break;
                                case "Director":
                                    e.Director = CommaCharacters(childNode.InnerText);  // standardise commas between entries
                                    break;
                                case "IMDB_ID":
                                    e.IMDB_ID = IMDB + "/title/" + childNode.InnerText;
                                    break;
                                case "Rating":
                                    e.Rating = childNode.InnerText;
                                    break;
                                case "seriesid":
                                    e.SeriesID = int.Parse(childNode.InnerText);
                                    break;
                            }
                        }
                    }
                    // other stuff
                    if (e.SeasonNum != -1 && e.EpisodeNum != -1)
                        e.Code = string.Format("S{0:00}E{1:00}", e.SeasonNum, e.EpisodeNum);
                    else
                        e.Code = "S--E--";
                    episodes.Add(e);
                }
            }
        }

        private void ParseBanner(XmlDocument doc, Series series, List<Banner> banners)
        {
            XmlNodeList Banners = doc.SelectNodes("//Banner");
            foreach (XmlNode node in Banners)
            {
                if (node.Name == "Banner")
                {
                    Banner b = Banner.CreateDefaults();
                    if (series != null)
                        b.SeriesID = series.TVDB_ID;
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        if (childNode.InnerText != "")
                        {
                            switch (childNode.Name)
                            {
                                case "id":
                                    b.TVDB_ID = int.Parse(childNode.InnerText);
                                    break;
                                case "BannerPath":
                                    b.BannerPath = TVDB + "/banners/" + childNode.InnerText;
                                    break;
                                case "BannerType2":
                                    b.BannerType2 = childNode.InnerText;
                                    break;
                                case "BannerType":
                                    b.BannerType = childNode.InnerText;
                                    break;
                                case "ThumbnailPath":
                                    b.ThumbnailPath = TVDB + "/banners/" + childNode.InnerText;
                                    break;
                            }
                        }
                    }
                    banners.Add(b);
                }
            }
        }

        private void ParseActor(XmlDocument doc, List<SeriesPerson> persons)
        {
            XmlNodeList Persons = doc.SelectNodes("//Actor");
            foreach (XmlNode node in Persons)
            {
                if (node.Name == "Actor")
                {
                    SeriesPerson sa = SeriesPerson.CreateDefaults();
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        if (childNode.InnerText != "")
                        {
                            switch (childNode.Name)
                            {
                                case "Name":
                                    sa.Name = childNode.InnerText.Trim();
                                    break;
                                case "Image":
                                    sa.Image = TVDB + "/banners/" + childNode.InnerText;
                                    break;
                                case "Role":
                                    sa.Role = childNode.InnerText;
                                    break;
                                case "SortOrder":
                                    sa.SortOrder = int.Parse(childNode.InnerText);
                                    break;
                            }
                        }
                    }
                    persons.Add(sa);
                }
            }
        }

    }
}
