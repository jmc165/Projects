using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CreateTVSeriesDatabase.Models
{
    // used for parsing purposes only
    public class SeriesPerson
    {
        public static SeriesPerson CreateDefaults()
        {
            SeriesPerson value = new SeriesPerson()
            {
                Image = "",
                Role = "",
                SortOrder = 0,
            };
            return value;
        }
        
        public string Name { get; set; }
        public string Image { get; set; }
        public string Role { get; set; }
        public int SortOrder { get; set; }
    }
}