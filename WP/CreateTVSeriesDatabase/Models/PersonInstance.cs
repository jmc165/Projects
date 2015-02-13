using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreateTVSeriesDatabase.Models
{
  
    // must match columns of TVSeries.PersonInstance table in SQL
    public class PersonInstance
    {
        public static PersonInstance CreateDefaults()
        {
            PersonInstance value = new PersonInstance()
            {
            };
            return value;
        }

        // cast type constants
        public const int MAIN_ACTOR = 0;
        public const int GUEST_STAR = 1;
        public const int WRITER = 2;
        public const int DIRECTOR = 3;

        public int id { get; set; }         // PK
        public string PersonName { get; set; }
        public int PersonID { get; set; }   // FK
        public int SeriesID { get; set; }   // FK
        public int EpisodeID { get; set; }  // FK
        public string Role { get; set; }
        public int SortOrder { get; set; }
        public string PersonImage { get; set; }
        public string PersonInstanceImage { get; set; }
        public int CastType { get; set; }        // 0 = Main Role, 1 = Guest Artist, 2 = Writer, 3 = Director
        public string CombinedName { get; set; } // Episode name (explicit, difficult to join on conditional EpisodeID)

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

    }
}
