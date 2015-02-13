using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreateTVSeriesDatabase.Models
{
    // must match columns of TVSeries.Person table in SQL
    public class Person
    {
        public static Person CreateDefaults()
        {
            Person value = new Person()
            {
            };
            return value;
        }

        public int id { get; set; }                 // PK
        public string Image { get; set; }           
        public string Name { get; set; }
     }
}
