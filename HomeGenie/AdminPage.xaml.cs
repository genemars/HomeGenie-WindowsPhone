using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;

namespace HomeGenie
{
    public partial class AdminPage : PhoneApplicationPage
    {
        public AdminPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
           Uri adminuri = new Uri(("http://" + (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerAddress"]));
           if (IsolatedStorageSettings.ApplicationSettings.Contains("RemoteServerUsername") &&
               (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerUsername"] != "" &&
               IsolatedStorageSettings.ApplicationSettings.Contains("RemoteServerPassword") &&
               (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerPassword"] != "")
           {
               adminuri = new Uri(("http://" + (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerUsername"] + ":" + (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerPassword"] + "@" + (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerAddress"]));
           }

           Browser.Navigate(adminuri);
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            this.NavigationService.GoBack();
        }
    }
}