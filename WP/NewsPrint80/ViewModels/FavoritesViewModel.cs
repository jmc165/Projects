using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using NewsPrint80.Resources;
using NewsPrint80.Models;
using System.IO.IsolatedStorage;

namespace NewsPrint80.ViewModels
{
    public class FavoritesViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Favorite> mItems;

        public FavoritesViewModel()
        {
            // create an observable list of Favorites
            Items = new ObservableCollection<Favorite>();
        }

        /// <summary>
        /// A collection for Favorite objects.
        /// </summary>
        public ObservableCollection<Favorite> Items 
        {
            get
            {
                return mItems;
            }
            set
            {
                mItems = value;
                RaisePropertyChanged("Items");
            }
        }

        // load Favorites from isolated storage
        public void Load()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Count > 0)
            {
                Items.Clear();
                foreach (Object o in IsolatedStorageSettings.ApplicationSettings.Values)
                {
                    Favorite f = o as Favorite;
                    if (f != null)
                    {
                        Items.Add(f);
                        RaisePropertyChanged("Items");
                    }
                }
            }
        }

        // save Favorite to isolated storage/observable items 
        public bool AddItem(Favorite f)
        {
            string key = f.Key;
            // save to isolated storage
            App.MainViewModel.SaveSettingsKey(key, f);
            var query = from i in Items
                        where i.Key == key
                        select i;
            if (query.Count<Favorite>() == 0) // not found
            {
                Items.Add(f);
                RaisePropertyChanged("Items");
                return true;
            }
            else
                return false;
        }

        // locate item
        public bool LocateItem(string url)
        {
            var query = (from i in Items
                         where i.URL == url
                         select i).ToList<Favorite>();
            return (query.Count == 1);
        }

        public bool RemoveItem(Favorite f)
        {
            string key = f.Key;
            // remove from isolated storage
            App.MainViewModel.RemoveKey(key, f);
            // remove from observable items
            foreach (var i in Items)
            {
                if (i.Key == key)
                {
                    Items.Remove(i);
                    RaisePropertyChanged("Items");
                    break;
                }
            }
            return true;
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