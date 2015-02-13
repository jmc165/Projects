using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreateTVSeriesDatabase.Models
{
    // must match columns of TVSeries.GenreInstance table in SQL
    public class GenreInstance
    {
        public static GenreInstance CreateDefaults()
        {
            GenreInstance value = new GenreInstance()
            {
            };
            return value;
        }

        public int id { get; set; }         // PK
        public int GenreID { get; set; }    // FK
        public int SeriesID { get; set; }   // FK
     }
}
