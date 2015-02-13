using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.IO;

namespace NewsPrint80.Models
{

    // Groupings are used for recents, Favorites and such like
    public class Recent : INotifyPropertyChanged
    {
        private string mNewspaperName = "";
        private string mRegionName = "";
        private string mCountryName = "";
        private string mURL = "";
        private DateTime mDateTime;

        public Recent()
        {
        }

        public Recent(Grouping mGroup)
        {
            this.mNewspaperName = mGroup.NewspaperName;
            this.mRegionName = mGroup.RegionName;
            this.mCountryName = mGroup.CountryName;
            this.mURL = mGroup.URL;
            this.mDateTime = DateTime.Now;
        }

        public string NewspaperName
        {
            get
            {
                return mNewspaperName;
            }
            set
            {
                mNewspaperName = value;
                RaisePropertyChanged("NewspaperName");
            }
        }

        public string RegionName
        {
            get
            {
                return mRegionName;
            }
            set
            {
                mRegionName = value;
                RaisePropertyChanged("RegionName");
            }
        }

        public string CountryName
        {
            get
            {
                return mCountryName;
            }
            set
            {
                mCountryName = value;
                RaisePropertyChanged("CountryName");
            }
        }

        public string URL
        {
            get
            {
                return mURL;
            }
            set
            {
                mURL = value;
                RaisePropertyChanged("URL");
            }
        }

        public DateTime DateTime
        {
            get
            {
                return mDateTime;
            }
            set
            {
                mDateTime = value;
                RaisePropertyChanged("DateTime");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string Key
        {
            get { return string.Format("R+{0}+{1}+{2}", mNewspaperName, mCountryName, mRegionName); }
        }
    }
}
