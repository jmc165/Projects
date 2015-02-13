using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Controls;
using System.Windows;

namespace TVSeries80.Models
{
    // used by web server API to gather custom query results
    public class EpisodeQuery
    {
        public int EpisodeID { get; set; }               
        public int SeriesID { get; set; }         
        public string EpisodeName { get; set; }
        public string SeriesName { get; set; }
        public string Banner { get; set; }
        public string Overview { get; set; }
        public string Code { get; set; }
        public string CombinedName { get { return string.Format("{0} {1}", Code, EpisodeName); } }
        public string Director { get; set; }
        public string Writer { get; set; }
        public string GuestStars { get; set; }
        public string Network { get; set; }

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