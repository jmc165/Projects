using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Controls;
using System.Windows;
using CreateTVSeriesDatabase.Models;

namespace TVSeries80.Models
{
    // used by web server API to gather custom query results
    public class PersonQuery : IComparable<PersonQuery>
    {
        public int PersonID { get; set; }
        public string PersonName { get; set; }
        public string PersonImage { get; set; }
        public int PersonInstanceID { get; set; }
        public int SeriesID { get; set; }
        public int EpisodeID { get; set; }
        public string PersonInstanceImage  { get; set; }
        public string Role { get; set; }
        public int SortOrder { get; set; }
        public int CastType { get; set; }
        public string Banner { get; set; }
        public string SeriesName { get; set; }
        public string Network { get; set; }
        public string CombinedName { get; set; }

        public string FormatRole
        {
            get
            {
                if (Role == "")
                {
                    if (CastType == 0)
                        return "Lead Actor";
                }
                return Role;
            }
        }

        public string FormatCastType
        {
            get
            {
                switch (CastType)
                {
                    case PersonInstance.MAIN_ACTOR:
                        return "Lead Actor";
                    case PersonInstance.GUEST_STAR:
                        return "Guest Artist";
                    case PersonInstance.WRITER:
                        return "Writer";
                    case PersonInstance.DIRECTOR:
                        return "Director";
                    default:
                        return "";
                }
            }
        }

        // return the empty person image in this configuration (to fill gap in grid)
        public string DefaultPersonInstanceImage
        {
            get
            {
                // try person instance, then person for an image
                if (string.IsNullOrEmpty(PersonInstanceImage))
                {
                    if (string.IsNullOrEmpty(PersonImage))
                    {
                        return "/Images/no.person.png";
                    }
                    else
                        return PersonImage;
                }
                else
                    return PersonInstanceImage;
            }
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

        public string IsCombinedNameVisible
        {
            get
            {
                if (CastType == PersonInstance.MAIN_ACTOR)
                    return "Collapsed";
                else
                    return "Visible";
            }
        }

        public int CompareTo(PersonQuery other)
        {
            if (this.SortOrder > other.SortOrder) return 1;
            else if (this.SortOrder < other.SortOrder) return -1;
            else return 0;
        }
    }
}