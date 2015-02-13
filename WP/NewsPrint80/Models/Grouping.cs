using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.IO;

namespace NewsPrint80.Models
{
    public class GroupCaseInsensitiveComparer : IComparer<Grouping>
    {
        public int Compare(Grouping x, Grouping y)
        {
            return string.Compare(x.CountryName, y.CountryName, StringComparison.OrdinalIgnoreCase);
        }
    }

    // Groupings are used for recents, Favorites and such like
    public class Grouping : INotifyPropertyChanged
    {
        private int mNewspaperID = -1;
        private string mNewspaperName = "";
        private int mRegionID = -1;
        private string mRegionName = "";
        private int mCountryID = -1;
        private string mCountryName = "";
        private string mURL = "";
        private byte[] mCountryImage = null;
        private byte[] mRegionImage = null;
 
        public Grouping()
        {
        }

        public Grouping(string value)
        {
            string[] tokens = value.Split(new char[] { ',' });
            if (tokens.Length == 7)
            {
                mNewspaperID = int.Parse(tokens[0]);
                mNewspaperName = tokens[1];
                mRegionID = int.Parse(tokens[2]);
                mRegionName = tokens[3];
                mCountryID = int.Parse(tokens[4]);
                mCountryName = tokens[5];
                mURL = tokens[6];
            }
        }

        public Grouping(Recent r)
        {
            mNewspaperName = r.NewspaperName;
            mRegionName = r.RegionName;
            mCountryName = r.CountryName;
            mURL = r.URL;
        }

        public Grouping(Favorite f)
        {
            mNewspaperName = f.NewspaperName;
            mRegionName = f.RegionName;
            mCountryName = f.CountryName;
            mURL = f.URL;
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6}",
                NewspaperID,
                NewspaperName,
                RegionID,
                RegionName,
                CountryID,
                CountryName,
                URL);
        }

        public int NewspaperID
        {
            get
            {
                return mNewspaperID;
            }
            set
            {
                mNewspaperID = value;
                RaisePropertyChanged("NewspaperID");
            }
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

        public int RegionID
        {
            get
            {
                return mRegionID;
            }
            set
            {
                mRegionID = value;
                RaisePropertyChanged("RegionID");
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

        public int CountryID
        {
            get
            {
                return mCountryID;
            }
            set
            {
                mCountryID = value;
                RaisePropertyChanged("CountryID");
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

        public BitmapImage CountryBitmapImage
        {
            get
            {
                if (mCountryImage != null && mCountryImage.Length > 0)
                {
                    using (MemoryStream stream = new MemoryStream(mCountryImage))
                    {
                        BitmapImage image = new BitmapImage();
                        image.SetSource(stream);
                        return image;
                    }
                }
                else
                    return null;
            }
        }

        public byte[] CountryImage
        {
            get
            {
                return mCountryImage;
            }
            set
            {
                mCountryImage = value;
                RaisePropertyChanged("CountryImage");
            }
        }

        public BitmapImage RegionBitmapImage
        {
            get
            {
                if (mRegionImage != null && mRegionImage.Length > 0)
                {
                    using (MemoryStream stream = new MemoryStream(mRegionImage))
                    {
                        BitmapImage image = new BitmapImage();
                        image.SetSource(stream);
                        return image;
                    }
                }
                else
                    return null;
            }
        }

        public byte[] RegionImage
        {
            get
            {
                return mRegionImage;
            }
            set
            {
                mRegionImage = value;
                RaisePropertyChanged("RegionImage");
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

    }
}
