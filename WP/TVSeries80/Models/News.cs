using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVSeries80.Models
{
    public class News
    {
        public string NewsName { get; set; }
        public string URI { get; set; }
        public int Images { get; set; }
        public bool Enabled { get; set; }
        public string IsVisible
        {
            get
            {
                if (!Enabled)
                    return "Collapsed";
                else
                    return "Visible";
            }
        }
    }
}
