using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace HomeGenie
{
    public partial class About : PhoneApplicationPage
    {
        public About()
        {
            InitializeComponent();
        }

        private void ReviewButton_Click(object sender, RoutedEventArgs e)
        {
            new MarketplaceReviewTask().Show();
        }

        private void SupportButton_Click(object sender, RoutedEventArgs e)
        {
            var emailComposeTask = new EmailComposeTask
            {
                To = "info@generoso.info",
                Subject = "[HomeGenie] feedback and support"
            };
            emailComposeTask.Show();
        }

        private void TextBlockAbout_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask wbt = new Microsoft.Phone.Tasks.WebBrowserTask();
            wbt.Uri = new Uri("http://generoso.info/homegenie");
            wbt.Show();
        }
    }
}