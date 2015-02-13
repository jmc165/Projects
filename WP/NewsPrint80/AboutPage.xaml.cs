using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Reflection;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace NewsPrint80
{
    public partial class AboutPage : PhoneApplicationPage
    {
        private const string mAuthor = "jmc165";

        public AboutPage()
        {
            this.DataContext = App.MainViewModel.NewsPrintViewModel;

            InitializeComponent();

            // fill in about
            var nameHelper = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
            var version = nameHelper.Version;
            var appName = nameHelper.Name;
            // TODO: extract author

            // fill in the about text
            this.AboutInfo1.Text = String.Format("Author: {0}\rVersion: {1}", mAuthor, version);
            this.AboutInfo2.Text = String.Format("This app maintains a searchable database of online newspapers and periodicals ({0} items) gathered from public areas of the web\r", App.MainViewModel.NewsPrintViewModel.NewspaperItems.Count);
            this.AboutInfo3.Text = "Acknowledgements to www.gosquared.com for use of their freeware flag icons\r";
        }

  
    }
}