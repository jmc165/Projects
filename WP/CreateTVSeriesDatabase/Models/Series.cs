using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreateTVSeriesDatabase.Models
{
    // must match columns of TVSeries.Series table in SQL
    public class Series : IComparable<Series>
    {
        public static Series CreateDefaults()
        {
            Series value = new Series()
            {
                Overview = "No synopsis available",
                FirstAired = "Unknown",
                Banner = "",
                Runtime = "Unknown",
                Status = "Unknown",
                Network = "Unknown",
                Rating = "Unrated",
                FanArt = "",
                IMDB_ID = "",
                Poster = "",
                ContentRating = "Unknown",
                SeriesName = "Unknown",
                AlphaKey = "u",
                Zap2it_ID = "",
            };
            return value;
        }

        public int id { get; set; }             // PK
        public string Overview { get; set; }
        public string SeriesName { get; set; }
        public string AlphaKey { get; set; }
        public string FirstAired { get; set; }
        public string IMDB_ID { get; set; }
        public string Banner { get; set; }
        public int TVDB_ID { get; set; }
        public string Rating { get; set; }
        public string Runtime { get; set; }
        public string Status { get; set; }
        public string FanArt { get; set; }
        public string Network { get; set; }
        public string FormatNetwork 
        {
            get
            {
#if (DEBUG)
                return String.Format("({0},{1}) {2}", id, TVDB_ID, Network);
#else
                    return Network;
#endif
            }
        }
        public string Actors { get; set; }
        public string Poster { get; set; }
        public string ContentRating { get; set; }
        public string Zap2it_ID { get; set; }

        public string FormatRuntime
        {
            get
            {
                if (Runtime != null && Runtime != "")
                    return Runtime + " minutes";
                else
                    return Runtime;
            }
        }

        public int CompareTo(Series other)
        {
            return this.SeriesName.CompareTo(other.SeriesName);
        }

        public string Information
        {
            get
            {
                return String.Format("{0}", Network);                  
            }
        }

        public string BannerHeight
        {
            get
            {
                if (string.IsNullOrEmpty(Banner) || Banner.Contains("no.image"))
                    return "0";
                else
                    return "120";
            }
        }

    }
}
