using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreateTVSeriesDatabase.Models
{
    // must match columns of TVSeries.Genre table in SQL
    public class Genre
    {
        public static Genre CreateDefaults()
        {
            Genre genre = new Genre()
            {
            };
            return genre;
        }
        public int id { get; set; }         // PK
        public string Name { get; set; }
    }
}
