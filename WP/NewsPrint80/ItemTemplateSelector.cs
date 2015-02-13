using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NewsPrint80.Models;

namespace NewsPrint80
{
    public class ItemTemplateSelector : TemplateSelector
    {
        public DataTemplate CountryAndRegionImage
        {
            get;
            set;
        }

        public DataTemplate CountryImage
        {
            get;
            set;
        }

        public DataTemplate RegionImage
        {
            get;
            set;
        }

        public DataTemplate NoImage
        {
            get;
            set;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // Determine which template to return;
            Grouping g = item as Grouping;
            if (g != null)
            {
                if (g.RegionImage != null && g.CountryImage != null)
                    return CountryAndRegionImage;
                else if (g.CountryImage != null && g.RegionImage == null)
                    return CountryImage;
                else if (g.CountryImage == null && g.RegionImage != null)
                    return RegionImage;
            }
            return NoImage;
        }
    }

}
