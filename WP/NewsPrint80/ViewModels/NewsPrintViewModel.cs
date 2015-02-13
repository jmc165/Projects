using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewsPrint80.Models;

namespace NewsPrint80.ViewModels
{
    public class NewsPrintViewModel
    {
        private NewsPrintDataContext mContext;

        public NewsPrintViewModel()
        {
#if (DEBUG)
            // DEVELOPMENT on EMULATOR
            mContext = new NewsPrintDataContext("Data Source=appdata:/NewsPrint.sdf");
#else
            // RELEASE to STORE or DEBUG on device
            mContext = new NewsPrintDataContext("Data Source=appdata:/NewsPrint.sdf;File Mode=Read Only;");
#endif
         }

        public NewsPrintDataContext Context
        {
            get { return mContext; }
        }

        public ObservableCollection<Country> CountryItems { get; set; }

        public List<AlphaKeyGroup<Country>> GroupedCountryItems { get; set; }

        public ObservableCollection<Newspaper> NewspaperItems { get; set; }

        public void Load()
        {
            var countries = from Country c in mContext.Countries
                            select c;

            // create a flat list of countries and a grouped list of countries (alpha sorted)
            // we can decide which we want to use
            CountryItems = new ObservableCollection<Country>(countries);

            GroupedCountryItems = AlphaKeyGroup<Country>.CreateGroups(countries,
    System.Threading.Thread.CurrentThread.CurrentUICulture,
    (Country s) => { return s.CountryName; }, true);       

            var newspapers = from Newspaper n in mContext.Newspapers
                            select n;

            NewspaperItems = new ObservableCollection<Newspaper>(newspapers);

        }

    }
}
