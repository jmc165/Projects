using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreateTVSeriesDatabase.Models
{
    public class EpisodeRatingComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == "Unrated" && y == "Unrated")
                return 0;
            else if (x == "Unrated" && y != "Unrated")
                return -1;
            else if (x != "Unrated" && y == "Unrated")
                return 1;
            else
            {
                double xD;
                double yD;
                if (double.TryParse(x, out xD) && double.TryParse(y, out yD))
                {
                    if (xD == yD)
                        return 0;
                    else if (xD > yD)
                        return 1;
                    else if (xD < yD)
                        return -1;
                }
             }
            return 0;
        }
    }

    // must match columns of TVSeries.Episode table in SQL
    public class Episode
    {
        public static Episode CreateDefaults()
        {
            Episode value = new Episode()
            {
                FirstAired = "Unknown",
                Overview = "No synopsis available",
                Rating = "Unrated",
                GuestStars = "Unknown",
                Writer = "Unknown",
                Director = "Unknown",
                SeasonNum = -1,
                EpisodeNum = -1,
                SeriesID = -1,
                IMDB_ID = "",
                FileName = "",      // optional, better as "" then no image
                EpisodeName = "",   // cannot be null
            };
            return value;
        }

        // columns
        public int id { get; set; }             // PK
        public string Overview { get; set; }
        public string EpisodeName { get; set; }
        public int SeriesID { get; set; }       // FK
        public string Code { get; set; }
        public string FirstAired { get; set; }
        public string FormatFirstAired
        {
            get
            {
#if (DEBUG)
                return String.Format("({0},{1}) {2}", id, TVDB_ID, FirstAired);
#else
                return FirstAired;
#endif
            }
        }        public string Director { get; set; }
        public string Writer { get; set; }
        public string GuestStars { get; set; }
        public string IMDB_ID { get; set; }
        public string Banner { get; set; }
        public string FileName { get; set; }
        public int SeasonNum { get; set; }
        public int EpisodeNum { get; set; }
        public int TVDB_ID { get; set; }
        public string Rating { get; set; }

  
        // helpers
        public string CombinedName 
        { 
            get 
            { 
                return string.Format("{0} {1}", Code, EpisodeName); 
            } 
        }

        public double RatingAsDouble 
        {
            get
            {
                double d;
                if (Rating != "Unrated" && double.TryParse(Rating, out d))
                    return d;
                return double.MinValue;
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

        public string IsFileNameVisible
        {
            get
            {
                if (string.IsNullOrEmpty(FileName))
                    return "Collapsed";
                else
                    return "Visible";
            }
        }

     }
}
