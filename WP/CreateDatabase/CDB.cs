using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data.SqlServerCe;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace CreateDatabase
{

    public class FlagItem
    {
        public string ISO { get; set; }
        public string CountryName { get; set; }
        public string FilePath { get; set; }

        public FlagItem()
        {
        }

        public FlagItem(string iso, string countryName, string filePath)
        {
            ISO = iso;
            CountryName = countryName;
            FilePath = filePath;
        }
    }

    [Serializable]
    public class CacheItem
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public CacheItem()
        {
        }

        public CacheItem(string k, string v)
        {
            Key = k;
            Value = v;
        }
    }

    // Note: CE SDF must be V3.5 compatible. Windows phone 8 does not work with V4.0!!!!!
    public class CDB
    {
        private const string CACHEFILE = "cache.xml";
        private const string ISOFILE = "flags\\iso.txt";

        private System.Windows.Forms.RichTextBox mRichTextBox;
        private System.Windows.Forms.ProgressBar mProgressBar;
        private string mConnStr = "Data Source = NewsPrint.sdf";
        private int mCountryID = 1;
        private int mRegionID = 1;
        private int mNewspaperID = 1;
        private int mErrors = 0;
        private bool mDeleteCache = false;
        private bool mDeleteDatabase = false;
        private string mProcessThisCountryOnly;

        private List<CacheItem> mCache = new List<CacheItem>();
        private List<FlagItem> mFlagItems = new List<FlagItem>();
        private Thread mThread = null;

        #region SQLCE helpers

        private List<int> MatchDataInTable(string table, string id, string match, string value)
        {
            string error = null;
            string sql = string.Format("SELECT {0} FROM {1} WHERE ({2} LIKE '%{3}%')", id, table, match, value);
            return ExecuteQuery(sql, id, out error);
        }

        private List<int> MatchCountry(string value)
        {
            string error = null;
            string command = string.Format("SELECT Country.CountryID FROM Country WHERE (CountryName LIKE '%{0}%')", value);
            List<int> rows = new List<int>();

            using (SqlCeConnection cn = new SqlCeConnection(mConnStr))
            {
                try
                {
                    cn.Open();
                    SqlCeCommand c = new SqlCeCommand(command, cn);
                    SqlCeDataReader rdr = c.ExecuteReader();
                    while (rdr.Read())
                    {
                        rows.Add((int)rdr["CountryID"]);
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
                    cn.Close();
                }
            }
        }

        private List<int> MatchRegion(int countryID, string name)
        {
            string error = null;
            string command = string.Format("SELECT Region.RegionID FROM Region INNER JOIN Country ON Country.CountryID = Region.CountryID WHERE (Region.RegionName LIKE '%{0}%') AND (Country.CountryID = {1})", name, countryID);
            List<int> rows = new List<int>();

            using (SqlCeConnection cn = new SqlCeConnection(mConnStr))
            {
                try
                {
                    cn.Open();
                    SqlCeCommand c = new SqlCeCommand(command, cn);
                    SqlCeDataReader rdr = c.ExecuteReader();
                    while (rdr.Read())
                    {
                        rows.Add((int)rdr["RegionID"]);
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
                    cn.Close();
                }
            }
        }

        private List<int> MatchNewspaper(int countryID, int regionID, string name)
        {
            string error = null;
            string command = string.Format("SELECT Newspaper.NewspaperID FROM Newspaper INNER JOIN Region ON Newspaper.RegionID = Region.RegionID WHERE (Newspaper.NewspaperName LIKE '%{0}%') AND (Newspaper.RegionID = {1})", name, regionID);
            List<int> rows = new List<int>();

            using (SqlCeConnection cn = new SqlCeConnection(mConnStr))
            {
                try
                {
                    cn.Open();
                    SqlCeCommand c = new SqlCeCommand(command, cn);
                    SqlCeDataReader rdr = c.ExecuteReader();
                    while (rdr.Read())
                    {
                        rows.Add((int)rdr["NewspaperID"]);
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
                    cn.Close();
                }
            }
        }

        private List<int> MatchNewspaper(int regionID)
        {
            string error = null;
            string command = string.Format("SELECT Newspaper.NewspaperID FROM Newspaper INNER JOIN Region ON Newspaper.RegionID = Region.RegionID WHERE (Newspaper.RegionID = {0})", regionID);
            List<int> rows = new List<int>();

            using (SqlCeConnection cn = new SqlCeConnection(mConnStr))
            {
                try
                {
                    cn.Open();
                    SqlCeCommand c = new SqlCeCommand(command, cn);
                    SqlCeDataReader rdr = c.ExecuteReader();
                    while (rdr.Read())
                    {
                        rows.Add((int)rdr["NewspaperID"]);
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
                    cn.Close();
                }
            }
        }

        private List<int> MatchRegions()
        {
            string error = null;
            string command = string.Format("SELECT RegionID FROM Region");
            List<int> rows = new List<int>();

            using (SqlCeConnection cn = new SqlCeConnection(mConnStr))
            {
                try
                {
                    cn.Open();
                    SqlCeCommand c = new SqlCeCommand(command, cn);
                    SqlCeDataReader rdr = c.ExecuteReader();
                    while (rdr.Read())
                    {
                        rows.Add((int)rdr["RegionID"]);
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
                    cn.Close();
                }
            }
        }

        // pass back list of id's matching query
        private List<int> ExecuteQuery(string command, string id, out string error)
        {
            error = null;
            List<int> rows = new List<int>();
            using (SqlCeConnection cn = new SqlCeConnection(mConnStr))
            {
                try
                {
                    cn.Open();
                    SqlCeCommand c = new SqlCeCommand(command, cn);
                    SqlCeDataReader rdr = c.ExecuteReader();
                    while (rdr.Read())
                    {
                        rows.Add((int)rdr[id]);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
                finally
                {
                    cn.Close();
                }
            }
            return rows;
        }

        private bool ExecuteNonQuery(SqlCeCommand c, out string error)
        {
            error = null;
            bool status = true;
            using (SqlCeConnection cn = new SqlCeConnection(mConnStr))
            {
                try
                {
                    cn.Open();
                    c.Connection = cn;
                    c.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    status = false;
                }
                finally
                {
                    cn.Close();
                }
            }
            return status;
        }

        private bool ExecuteNonQuery(string command, out string error)
        {
            error = null;
            SqlCeConnection conn = null;
            bool status = true;
            try
            {
                conn = new SqlCeConnection(mConnStr);
                conn.Open();
                SqlCeCommand cmd = conn.CreateCommand();
                cmd.CommandText = command;
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (SystemException ex)
            {
                error = ex.ToString();
                status = false;
            }
            finally
            {
                conn.Close();
            }
            return status;
        }

        #endregion

        private bool AddCountry(string name, byte[] imageItem, out int countryID)
        {
            countryID = -1;
            string error;
            SqlCeCommand ce = new SqlCeCommand();

            if (imageItem != null)
            {
                ce.CommandText = "INSERT INTO Country(CountryID,CountryName,ImageItem)" +
                    " values(@CountryID,@CountryName,@ImageItem)";

                ce.Parameters.Add("@CountryID", System.Data.SqlDbType.Int, 4);
                ce.Parameters.Add("@CountryName", System.Data.SqlDbType.NText);
                ce.Parameters.Add("@ImageItem", System.Data.SqlDbType.Image);

                countryID = mCountryID;
                ce.Parameters["@CountryID"].Value = mCountryID++;
                ce.Parameters["@CountryName"].Value = name;
                ce.Parameters["@ImageItem"].Value = imageItem;
            }
            else
            {
                ce.CommandText = "INSERT INTO Country(CountryID,CountryName)" +
                     " values(@CountryID,@CountryName)";

                ce.Parameters.Add("@CountryID", System.Data.SqlDbType.Int, 4);
                ce.Parameters.Add("@CountryName", System.Data.SqlDbType.NText);

                countryID = mCountryID;
                ce.Parameters["@CountryID"].Value = mCountryID++;
                ce.Parameters["@CountryName"].Value = name;
            }

            return ExecuteNonQuery(ce, out error);
        }

        private bool CheckAndAddCountry(string name, byte[] image, out int countryID, out bool matched)
        {
            bool v = false;
            matched = false;
            List<int> matches = MatchCountry(name);
            if (matches != null && matches.Count == 1)
            {
                countryID = matches[0];
                // keep keys unique
                if (mCountryID <= countryID)
                    mCountryID = countryID + 1;
                matched = true;
            }
            else
                v = AddCountry(name, image, out countryID);  // inner text if value of 'a'
            return v;
        }

        private bool AddRegion(int countryID, string name, out int regionID)
        {
            regionID = -1;
            string error;
            SqlCeCommand ce = new SqlCeCommand();

            ce.Parameters.Clear();

            ce.CommandText = "INSERT INTO Region(RegionID,RegionName,CountryID)" +
                " values(@RegionID,@RegionName,@CountryID)";

            ce.Parameters.Add("@RegionID", System.Data.SqlDbType.Int, 4);
            ce.Parameters.Add("@RegionName", System.Data.SqlDbType.NText);
            ce.Parameters.Add("@CountryID", System.Data.SqlDbType.Int, 4);

            regionID = mRegionID;
            ce.Parameters["@RegionID"].Value = mRegionID++;
            ce.Parameters["@RegionName"].Value = name;
            ce.Parameters["@CountryID"].Value = countryID;

            return ExecuteNonQuery(ce, out error);
        }

        private bool DeleteRegion(int regionID)
        {
            string error;
            SqlCeCommand ce = new SqlCeCommand();

            ce.Parameters.Clear();

            ce.CommandText = "DELETE FROM Region" +
                " WHERE RegionID=@RegionID";

            ce.Parameters.Add("@RegionID", System.Data.SqlDbType.Int, 4);
            ce.Parameters["@RegionID"].Value = regionID;

            return ExecuteNonQuery(ce, out error);
        }

        private bool CheckAndAddRegion(string countryName, int countryID, string regionName, out int regionID, out bool matched)
        {
            bool v = false;
            matched = false;
            regionID = -1;
            if (InvalidRegion(regionName))
                return false;  
            List<int> matches = MatchRegion(countryID, regionName);
            if (matches != null && matches.Count == 1)
            {
                regionID = matches[0];
                // keep keys unique
                if (mRegionID <= regionID)
                    mRegionID = regionID + 1;
                matched = true;
            }
            else
                v = AddRegion(countryID, regionName, out regionID);   // inner text if value of 'a'
            return v;
        }

        private bool AddNewspaper(int regionID, string name, string URL, out int newspaperID)
        {
            newspaperID = -1;
            string error;
            SqlCeCommand ce = new SqlCeCommand();

            ce.Parameters.Clear();

            ce.CommandText = "INSERT INTO Newspaper(NewspaperID,RegionID,NewspaperName,URL)" +
                " values(@NewspaperID,@RegionID,@NewspaperName,@URL)";

            ce.Parameters.Add("@NewspaperID", System.Data.SqlDbType.Int, 4);
            ce.Parameters.Add("@RegionID", System.Data.SqlDbType.Int, 4);
            ce.Parameters.Add("@NewspaperName", System.Data.SqlDbType.NText);
            ce.Parameters.Add("@URL", System.Data.SqlDbType.NText);

            newspaperID = mNewspaperID;
            ce.Parameters["@NewspaperID"].Value = mNewspaperID++;
            ce.Parameters["@RegionID"].Value = regionID;
            ce.Parameters["@NewspaperName"].Value = name;
            ce.Parameters["@URL"].Value = URL;

            return ExecuteNonQuery(ce, out error);
        }

        private bool CheckAndAddNewspaper(int countryID, int regionID, string name, string URL, out int newspaperID, out bool matched)
        {
            bool v = false;
            matched = false;
            newspaperID = -1;
            if (InvalidNewspaper(URL))
                return false;
            List<int> matches = MatchNewspaper(countryID, regionID, name);
            if (matches != null && matches.Count == 1)
            {
                newspaperID = matches[0];
                // keep keys unique
                if (mNewspaperID <= newspaperID)
                    mNewspaperID = newspaperID + 1;
                matched = true;
            }
            else
                v = AddNewspaper(regionID, name, URL, out newspaperID);   // inner text if value of 'a'
            return v;
        }

        public string NameRemoveInvalid(string name)
        {
            name = name.Replace("'", "");
            name = name.Replace("-", "");
            return name;
        }

        public CDB(System.Windows.Forms.RichTextBox richTextBox, System.Windows.Forms.ProgressBar progressBar, bool deleteCache, bool deleteDatabase, string processThisCountryOnly)
        {
            this.mProgressBar = progressBar;
            this.mRichTextBox = richTextBox;
            this.mDeleteCache = deleteCache;
            this.mDeleteDatabase = deleteDatabase;
            this.mProcessThisCountryOnly = processThisCountryOnly;

            mThread = new Thread(new ThreadStart(CreateDatabase));
            mThread.Name = "CreateDatabase";
            mThread.IsBackground = true;
            mThread.SetApartmentState(ApartmentState.STA);
            mThread.Start();
        }

        private void CreateDatabase()
        {
            string sdfFile = "NewsPrint.sdf";
            string sql;
            string error;

            var directoryName = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var fileName = System.IO.Path.Combine(directoryName, "NewsPrint.sdf");

            // Load ISO
            LoadIso();

            if (mDeleteCache && File.Exists(CACHEFILE))
                File.Delete(CACHEFILE);

            if (mDeleteDatabase & File.Exists(sdfFile))
                File.Delete(sdfFile);

            if (!File.Exists(sdfFile))
            {

                SqlCeEngine engine = new SqlCeEngine(mConnStr);
                engine.CreateDatabase();

                // create countries table
                sql = @"
            CREATE TABLE Country
            (
            CountryID int NOT NULL,
            CountryName ntext NULL,
            ImageItem image NULL,
            Location ntext NULL,
            CONSTRAINT CountryPk PRIMARY KEY (CountryID)
            )";

                if (!ExecuteNonQuery(sql, out error))
                    mRichTextBox.AppendText(error + "\r");

                // create regions (belonging to a country)
                sql = @"
            CREATE TABLE Region
            (
            RegionID int NOT NULL,
            CountryID int NOT NULL,
            RegionName ntext NULL,
            ImageItem image NULL,
            Location ntext NULL,
            CONSTRAINT RegionPk PRIMARY KEY (RegionID),
            CONSTRAINT CountryFk FOREIGN KEY (CountryID) REFERENCES Country(CountryID)
            )";

                if (!ExecuteNonQuery(sql, out error))
                    mRichTextBox.AppendText(error + "\r");

                // create newspapers table (belonging to region)
                sql = @"
            CREATE TABLE Newspaper
            (
            NewspaperID int NOT NULL,
            RegionID int NOT NULL,
            NewspaperName ntext NULL,
            ImageItem image NULL,
            URL ntext NULL,
            CONSTRAINT NewspaperPk PRIMARY KEY (NewspaperID),
            CONSTRAINT RegionFk FOREIGN KEY (RegionID) REFERENCES Region(RegionID)
            )";

                if (!ExecuteNonQuery(sql, out error))
                    mRichTextBox.AppendText(error + "\r");

            }

            // Now perform some web scaping

            mCountryID = 1;
            mRegionID = 1;
            mNewspaperID = 1;
            mErrors = 0;

            LoadCache();
            HtmlWeb webClient = new HtmlWeb();
            ParsePage1(webClient, "http://www.listofnewspapers.com/en/pages.html");
            CleanupDatabase();
            Message("Errors " + mErrors + "\r");
        }

        private void CleanupDatabase()
        {
            // get list of all regions
            List<int> regions = MatchRegions();
            // get newspapers in this region
            foreach (int r in regions)
            {
                List<int> newspapers = MatchNewspaper(r);
                if (newspapers.Count == 0)
                {
                    // we can drop the region from then table, no newspaper references it
                    DeleteRegion(r);
                }
            }
        }

        private void Message(string s)
        {

            this.mRichTextBox.BeginInvoke(new MethodInvoker(
            delegate()
            {
                mRichTextBox.AppendText(s);
                mRichTextBox.ScrollToCaret();

            }));

        }

        private void LoadIso()
        {
            FileStream fs = null;
            StreamReader reader = null;
            if (File.Exists(ISOFILE))
            {
                try
                {
                    fs = new FileStream(ISOFILE, FileMode.Open);
                    reader = new StreamReader(fs);
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] tokens = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (tokens.Length > 0)
                        {
                            string name = String.Join(" ", tokens, 1, tokens.Length - 1);
                            string targetIso = "Flags\\" + tokens[0] + ".png";
                            if (File.Exists(targetIso))
                                mFlagItems.Add(new FlagItem(tokens[0], name, targetIso));
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
            }
        }
        private void LoadCache()
        {
            FileStream fs = null;
            StreamReader reader = null;
            if (File.Exists(CACHEFILE))
            {
                XmlSerializer mySerializer = new XmlSerializer(typeof(List<CacheItem>));
                try
                {
                    fs = new FileStream(CACHEFILE, FileMode.Open);
                    reader = new StreamReader(fs);
                    mCache = mySerializer.Deserialize(reader) as List<CacheItem>;
                    fs.Close();
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
            }
        }

        private void SaveCache()
        {
            FileStream fs = null;
            TextWriter writer = null;
            try
            {
                XmlSerializer mySerializer = new XmlSerializer(typeof(List<CacheItem>));
                fs = new FileStream(CACHEFILE, FileMode.OpenOrCreate);
                writer = new StreamWriter(fs);
                mySerializer.Serialize(writer, mCache);
            }
            catch
            {
            }
            finally
            {
                if (writer != null)
                    writer.Close();
                if (fs != null)
                    fs.Close();
            }
        }

        private HtmlAgilityPack.HtmlDocument CachePage(HtmlWeb webClient, string page, out bool hit)
        {
            hit = false;
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

            hit = false;
            if (mCache.Count != 0)
            {
                foreach (var i in mCache)
                    if (i.Key == page)
                    {
                        hit = true;
                        fileName = i.Value;
                        break;
                    }
            }

            if (!hit)   // not in cache we must load from web
            {
                // load the page
                doc = webClient.Load(page);
                fileName = key;
                if (doc != null)
                {
                    doc.Save(fileName);
                }
                // add to cache
                mCache.Add(new CacheItem(page, fileName));
                // save cache
                SaveCache();
            }
            else
            {
                // in cache we can load from file
                doc = new HtmlAgilityPack.HtmlDocument();
                doc.Load(fileName);
            }
            return doc;
        }

        private void ParsePage1(HtmlWeb webClient, string page)
        {
            string name;
            string URL;
            try
            {
                int countryID;
                int level = 0;
                bool matched;
                bool cacheHit = false;

                // cache page
                HtmlAgilityPack.HtmlDocument doc = CachePage(webClient, page, out cacheHit);

                if (cacheHit)
                {
                    Message("Fetching " + page + " from cache\r");
                }

                // collect href attribute from 'a' node within a div with attribute class equal to divlistglobal
                // collect style attribute from 'p' node within a div with attribute class equal to divlistglobal
                HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class='divlistglobal']//a[@href] | //div[@class='divlistglobal']//p[@style]");

                // count the countries on pass 1
                int countries = 0;
                foreach (var node in nodes)
                {
                    name = NameRemoveInvalid(node.InnerText);
                    if (node.OuterHtml.Contains("15px"))        // outer region
                        level = 0;
                    else if (node.OuterHtml.Contains("30px"))   // country
                        level = 1;
                    else if (node.OuterHtml.Contains("60px"))   // region
                        level = 2;
                    if (node.Attributes["href"] != null)        // its a link ('a' href attribute)
                    {
                        if (level == 1)
                        {
                            countries++;                           
                        }
                    }
                }

                // fill in the progress bar 
                this.mRichTextBox.BeginInvoke(new MethodInvoker(
                       delegate()
                       {
                           mProgressBar.Minimum = 0;
                           mProgressBar.Maximum = countries;
                           mProgressBar.Value = 0;
                       }));     
                
                // second pass
                foreach (var node in nodes)
                {
                    name = NameRemoveInvalid(node.InnerText);
                    if (node.OuterHtml.Contains("15px"))        // outer region
                        level = 0;
                    else if (node.OuterHtml.Contains("30px"))   // country
                        level = 1;
                    else if (node.OuterHtml.Contains("60px"))   // region
                        level = 2;
                    if (node.Attributes["href"] != null)        // its a link ('a' href attribute)
                    {
                        URL = node.Attributes["href"].Value;
                        if (level == 1)
                        {
                            byte[] image = null;
                            name = ModifyCountryName(name);
                            this.mRichTextBox.BeginInvoke(new MethodInvoker(
                             delegate()
                             {
                                 mProgressBar.Value = mProgressBar.Value + 1;
                             }));
                            if (mProcessThisCountryOnly != "" && name != mProcessThisCountryOnly)
                                continue;
                            CheckAndFetchIso(name.ToUpper(), out image);
                            CheckAndAddCountry(name, image, out countryID, out matched);  // inner text if value of 'a'
                             Message("Country " + name + ((matched) ? " already in database\r" : " added to database\r"));
                            // parse recursively
                            ParsePage2(webClient, name, countryID, URL);
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

        private string ModifyCountryName(string name)
        {
            switch (name)
            {
                case "Cental African Republic":
                    return "Central African Republic";
            }
            return name;
        }

        private void CheckAndFetchIso(string name, out byte[] image)
        {
            image = null;
            bool match = false;
            FlagItem item = null;
 
            var query = (from i in mFlagItems
                         where i.CountryName.Contains(name)
                         select i).ToList<FlagItem>();

            if (query.Count == 1)
            {
                match = true;
                item = query[0];
            }
            else
            {
                if (query.Count > 0)
                {
                    // look for exact match
                    var query2 = (from i in query
                                  where i.CountryName.Contains(name)
                                  select i).ToList<FlagItem>();
                    if (query2.Count == 1)
                    {
                        match = true;
                        item = query2[0];
                    }
                }
            }

            if (match)   // match
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(item.FilePath, FileMode.Open, FileAccess.Read);
                    //a byte array to read the image                 
                    image = new byte[fs.Length];
                    fs.Read(image, 0, System.Convert.ToInt32(fs.Length));
                }
                catch
                {
                }
                finally
                {
                    if (fs != null)
                        fs.Close();
                }
            }

            else
            {
                Message("Country " + name + " flag not located\r");
            }

        }

        private void ParsePage2(HtmlWeb webClient, string countryName, int countryID, string page)
        {
            string name;
            string URL;
            try
            {
                int regionID = -1;
                int newspaperID;
                bool matched = false;
                HtmlNodeCollection nodes;
                bool cacheHit;
                string regionName;

                // cache page
                HtmlAgilityPack.HtmlDocument doc = CachePage(webClient, page, out cacheHit);

                if (cacheHit)
                {
                    Message("Fetching " + page + " from cache\r");
                }

                nodes = doc.DocumentNode.SelectNodes("//div[@class='divlistglobal']//a[@href] | //ul[@class='ulstates']");

                foreach (var node in nodes)
                {
                    name = NameRemoveInvalid(node.InnerText);
                    if (node.Attributes["href"] != null)
                    {
                        URL = node.Attributes["href"].Value;
                        if (URL.Contains("www.listofnewspapers.com")) // has sub-regions
                        {
                            regionName = name;
                            if (CheckAndAddRegion(countryName, countryID, regionName, out regionID, out matched))
                            {
                                Message("  Region " + name + ((matched) ? " already in database\r" : " added to database\r"));
                                // recurse
                                ParsePage3(webClient, countryName, regionName, countryID, regionID, URL);
                            }
                        }
                        else // contains newspapers
                        {
                            if (CheckAndAddNewspaper(countryID, regionID, name, URL, out newspaperID, out matched))
                            {
                                Message("    Newspaper " + name + ((matched) ? " already in database\r" : " added to database\r"));
                            }
                        }
                    }
                    else // ul contains the region ID
                    {
                        regionName = name;
                        if (CheckAndAddRegion(countryName, countryID, regionName, out regionID, out matched))
                        {
                            Message("  Region " + name + ((matched) ? " already in database\r" : " added to database\r"));
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

  
        private void ParsePage3(HtmlWeb webClient, string countryName, string regionName, int countryID, int regionID, string page)
        {
            string name;
            string URL;
            try
            {
                int newspaperID;
                bool matched = false;
                HtmlNodeCollection nodes;
                bool cacheHit;

                // cache page
                HtmlAgilityPack.HtmlDocument doc = CachePage(webClient, page, out cacheHit);

                if (cacheHit)
                {
                    Message("Fetching " + page + " from cache\r");
                }

                nodes = doc.DocumentNode.SelectNodes("//div[@class='divlistglobal']//a[@href] | //ul[@class='ulstates']");

                foreach (var node in nodes)
                {
                    name = NameRemoveInvalid(node.InnerText);
                    if (node.Attributes["href"] != null)
                    {
                        URL = node.Attributes["href"].Value;
                        if (!URL.Contains("www.listofnewspapers.com"))
                        {
                            if (CheckAndAddNewspaper(countryID, regionID, name, URL, out newspaperID, out matched))
                            {
                                Message("    Newspaper " + name + ((matched) ? " already in database\r" : " added to database\r"));
                            }
                        }
                    }
                    else // ul contains the region ID
                    {
                        string newRegionName = name;
                        if (InvalidRegion(newRegionName))
                            continue;
                        newRegionName = string.Format("{0}/{1}", regionName, newRegionName); // combine region name
                        if (CheckAndAddRegion(countryName, countryID, newRegionName, out regionID, out matched))
                        {
                            Message("  Region " + newRegionName + ((matched) ? " already in database\r" : " added to database\r"));
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

        internal void Close()
        {
            if (mThread != null)
                mThread.Abort();
        }

        private bool InvalidRegion(string name)
        {
            switch (name)
            {
                case "Wales":
                case "Scotland":
                case "Northern Ireland":
                case "OFFICIAL JOURNALS":
                    return true;
            }
            return false;
        }

        private bool InvalidNewspaper(string URL)
        {
            if (URL == null || URL == "")
                return true;
            return false;
        }

    }



}
