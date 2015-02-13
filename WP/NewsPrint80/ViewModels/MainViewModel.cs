using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using NewsPrint80.Models;

namespace NewsPrint80.ViewModels
{
    public class MainViewModel
    {

        // sub-models
        private static FavoritesViewModel mFavoritesViewModel = null;
        private static RecentsViewModel mRecentsViewModel = null;
        private static NewsPrintViewModel mNewsPrintViewModel = null;

        public MainViewModel()
        {
            // create the sub models
            if (mFavoritesViewModel == null)
                mFavoritesViewModel = new FavoritesViewModel();

            if (mRecentsViewModel == null)
                mRecentsViewModel = new RecentsViewModel();

            if (mNewsPrintViewModel == null)
                mNewsPrintViewModel = new NewsPrintViewModel();
        }

        public void SaveSettingsKey(string key, object value)
        {
            // add value to isolated storage
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(key))
                IsolatedStorageSettings.ApplicationSettings.Add(key, value);
            else
                IsolatedStorageSettings.ApplicationSettings[key] = value;
            // must save
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public bool ContainsSettingsKey(string key)
        {
            // add value to isolated storage
            return IsolatedStorageSettings.ApplicationSettings.Contains(key);
        }

        public object LoadSettingsKey(string key)
        {
            // add value to isolated storage
            if (IsolatedStorageSettings.ApplicationSettings.Contains(key))
                return IsolatedStorageSettings.ApplicationSettings[key];
            else
                return null;
        }

        public void RemoveKey(string key, object value)
        {
            // add value to isolated storage
            if (IsolatedStorageSettings.ApplicationSettings.Contains(key))
            {
                IsolatedStorageSettings.ApplicationSettings.Remove(key);
                // must save
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }


        // properties to return the sub models
        public FavoritesViewModel FavoritesViewModel
        {
            get
            {
                return mFavoritesViewModel;
            }
        }

        public RecentsViewModel RecentsViewModel
        {
            get
            {
                return mRecentsViewModel;
            }
        }

        public NewsPrintViewModel NewsPrintViewModel 
        { 
            get 
            { 
                return mNewsPrintViewModel; 
            } 
        }

        #region public properties to bind against

        public ObservableCollection<Favorite> FavoriteItems
        {
            get
            {
                return mFavoritesViewModel.Items;
            }
        }

        public ObservableCollection<Recent> RecentItems
        {
            get
            {
                return mRecentsViewModel.Items;
            }
        }

        public List<AlphaKeyGroup<Country>> GroupedCountryItems
        {
            get
            {
                return mNewsPrintViewModel.GroupedCountryItems;
            }
        }

        public ObservableCollection<Country> CountryItems
        {
            get
            {
                return mNewsPrintViewModel.CountryItems;
            }
        }

        #endregion

    }
}
