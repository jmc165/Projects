using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NewsPrint80.Resources;
using NewsPrint80.Models;
using System.IO.IsolatedStorage;

namespace NewsPrint80.ViewModels
{
    public class RecentsViewModel : INotifyPropertyChanged
    {
        private const int MAX_RECENTS = 16;
        private ObservableCollection<Recent> mItems;

        public RecentsViewModel()
        {
            // create an observable list of recents
            this.Items = new ObservableCollection<Recent>();
        }

        public ObservableCollection<Recent> Items
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

        // load recents from isolated storage (max MAX_RECENTS)
        public void Load()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Count > 0)
            {
                List<Recent> a = new List<Recent>();
                foreach (Object o in IsolatedStorageSettings.ApplicationSettings.Values)
                {
                    Recent r = o as Recent;
                    if (r != null)
                        a.Add(r);
                }

                // sort
                var b = (from g in a
                         orderby g.DateTime descending
                         select g).ToList();

                Items.Clear();
                foreach (Recent d in b)
                {
                    Items.Add(d);
                    RaisePropertyChanged("Items");
                }
             }
        }

        // save recent to isolated storage/observable items
        public bool AddItem(Recent r)
        {
            string key = r.Key;
            // add to observal items, forces remap, if not there
            var query = (from i in Items
                        where i.Key == key
                        select i).ToList<Recent>();
            if (query.Count == 0) // not found
            {
                // only store up to maximum
                if (Items.Count < MAX_RECENTS)
                {
                    // save to isolated storage
                    App.MainViewModel.SaveSettingsKey(key, r);
                    Items.Insert(0, r); // add to start
                    RaisePropertyChanged("Items");
                    return true;
                }
                else if (Items.Count == MAX_RECENTS)
                {
                    // remove the first
                    Items.RemoveAt(0);
                    // save to isolated storage
                    App.MainViewModel.SaveSettingsKey(key, r);
                    // add the first
                    Items.Insert(0, r);
                    RaisePropertyChanged("Items");
                    return true;
                }
            }
            else
            {
                // found, remove it
                Recent rr = query[0];
                Items.Remove(rr);
                rr.DateTime = DateTime.Now;
                App.MainViewModel.SaveSettingsKey(key, rr);
                // put it at head of queue
                Items.Insert(0, r);
                RaisePropertyChanged("Items");
                return true;
            }
            return false;
        }

        public bool RemoveItem(Recent r)
        {
            string key = r.Key;
            // remove from isolated storage
            App.MainViewModel.RemoveKey(key, r);
            // remove from observable items
            var query = (from i in Items
                         where i.Key == key
                         select i).ToList<Recent>();
            if (query.Count == 1)
            {
                Recent rr = query[0];
                Items.Remove(rr);
                RaisePropertyChanged("Items");
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