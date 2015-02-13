using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVSeries80.Models
{
    // must match columns of TVSeries.Administration table in SQL
    public class Administration
    {
        public int id { get; set; }
        public int Mode { get; set; }
        public DateTime LastUpdated { get; set; }
        public int Counter { get; set; }
    }
}
