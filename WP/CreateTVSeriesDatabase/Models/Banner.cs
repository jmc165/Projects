using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CreateTVSeriesDatabase.Models
{
    // must match columns of TVSeries.Banner table in SQL
    public class Banner
    {
        public static Banner CreateDefaults()
        {
            Banner value = new Banner()
            {
                ThumbnailPath = "",
                BannerPath = "",
                SeriesID = -1,
            };
            return value;
        }

        // columns
        public int id { get; set; }                 // PK
        public int SeriesID { get; set; }           // FK
        public string ThumbnailPath { get; set; }
        public string BannerPath { get; set; }
        public string BannerType { get; set; }
        public string BannerType2 { get; set; }
        public int TVDB_ID { get; set; }
    }
}
