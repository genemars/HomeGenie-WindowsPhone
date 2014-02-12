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
    public partial class WebBrowserPage : PhoneApplicationPage
    {
        public WebBrowserPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
           Uri uri = PhoneApplicationService.Current.State["WebBrowserPageUrl"] as Uri;
           Browser.Navigate(uri);
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void PhoneApplicationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (e.Orientation == PageOrientation.Landscape || e.Orientation == PageOrientation.LandscapeLeft || e.Orientation == PageOrientation.LandscapeRight)
            {
                SystemTray.IsVisible = false;
                ApplicationBar.IsVisible = false;
            }
            else
            {
                SystemTray.IsVisible = true;
                ApplicationBar.IsVisible = true;
            }
        }
    }
}