using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace NewsPrint80
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        private bool mLoading = false;

        public SettingsPage()
        {
            InitializeComponent();

            mLoading = true;

            if (App.MainViewModel.ContainsSettingsKey(App.LOCATION_SERVICES_KEY))
            {
                bool lsOn = (bool)App.MainViewModel.LoadSettingsKey(App.LOCATION_SERVICES_KEY);
                if (lsOn)
                    this.LocationServicesButton.DataContext = "Location Services Off";
                else
                    this.LocationServicesButton.DataContext = "Location Services On";
            }

            if (App.MainViewModel.LoadSettingsKey(App.WEBSITE_STYLE_KEY) != null)
            {
                string value = App.MainViewModel.LoadSettingsKey(App.WEBSITE_STYLE_KEY) as string;
                if (value != null)
                {
                    switch (value)
                    {
                        case "Mobile":
                            iMobile.IsChecked = true;
                            iDesktop.IsChecked = false;
                            break;
                        case "Desktop":
                            iMobile.IsChecked = false;
                            iDesktop.IsChecked = true;
                            break;
                    }
                }
            }

            mLoading = false;

        }

        private void OnWebsiteStyleChecked(object sender, RoutedEventArgs e)
        {
            if (mLoading)
                return;
            RadioButton rb = sender as RadioButton;
            string value = rb.Content as string;
            if (value != null)
                App.MainViewModel.SaveSettingsKey(App.WEBSITE_STYLE_KEY, value);
        }

        private void OnLSButtonClick(object sender, RoutedEventArgs e)
        {
            if ((string)this.LocationServicesButton.DataContext == "Location Services Off")
            {
                App.MainViewModel.SaveSettingsKey(App.LOCATION_SERVICES_KEY, false);
                this.LocationServicesButton.DataContext = "Location Services On";
            }
            else if ((string)this.LocationServicesButton.DataContext == "Location Services On")
            {
                App.MainViewModel.SaveSettingsKey(App.LOCATION_SERVICES_KEY, true);
                this.LocationServicesButton.DataContext = "Location Services Off";
            }
        }

    }
}