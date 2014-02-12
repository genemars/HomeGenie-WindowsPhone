#define DEBUG_AGENT

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
using System.Threading;
using HomeGenie.ViewModel.Objects;
using System.Windows.Threading;
using HomeGenie.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Phone.Tasks;

using HomeGenie.Resources;
using HomeGenie.ViewModel.Converters;
using System.Windows.Media;
using System.Globalization;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Notification;
using System.Text;
using Microsoft.Phone.Info;
using Coding4Fun.Toolkit.Controls;
using System.Windows.Media.Imaging;

namespace HomeGenie
{
    public partial class GroupsPage : PhoneApplicationPage
    {
        private Timer _statupdate;
        private int _updateinterval = 5000;
        private string _lastsavedgroup;
        private bool _appbaropen;
        private bool _juststarted = true;

        public GroupsPage()
        {

            InitializeComponent();

            DataContext = App.ViewModel;

            _lastsavedgroup = App.ViewModel.CurrentGroup;
            App.ViewModel.ModulesUpdating += ViewModel_ModulesUpdating;
            App.ViewModel.ModulesUpdated += ViewModel_ModulesUpdated;

            _setupViewModel();
            _setupLoadingDialog();

            this.Loaded += GroupsPage_Loaded;

            _statupdate = new Timer(_systemStatusPoll);

        }


        private void ViewModel_ModulesUpdating(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                LoaderTop.Visibility = Visibility.Visible;
            });
        }

        private void ViewModel_ModulesUpdated(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                LoaderTop.Visibility = Visibility.Collapsed;
                //
                _updateActionBarMenu();
            });
        }

        private void _updateActionBarMenu()
        {
            if (_appbaropen) return;
            ApplicationBar.MenuItems.Clear();
            foreach (Module m in App.ViewModel.Items[PivotView.SelectedIndex].Modules)
            {
                if (m.DeviceType == Module.DeviceTypes.Program)
                {
                    foreach (ModuleParameter p in m.Properties)
                    {
                        if (p.Name == "Widget.DisplayModule" && (p.Value == "" || p.Value == "homegenie/generic/program"))
                        {
                            ApplicationBarMenuItem appBarMenuAdd = new ApplicationBarMenuItem(m.Name);
                            Module modref = m;
                            appBarMenuAdd.Click += new EventHandler((object sender, EventArgs e) =>
                            {
                                ApplicationBarMenuItem item = (ApplicationBarMenuItem)sender;
                                string url = "/api/HomeAutomation.HomeGenie/Automation/Programs.Run/" + modref.Address + "/" + App.ViewModel.CurrentGroup + "/" + DateTime.Now.Ticks.ToString();
                                App.HttpManager.AddToQueue("Programs.Run[" + modref.Address + "]", url, (WebRequestCompletedArgs args) =>
                                {
                                    this.Dispatcher.BeginInvoke(() =>
                                    {
                                        App.ViewModel.UpdateCurrentGroup();
                                    });
                                });
                            });
                            ApplicationBar.MenuItems.Add(appBarMenuAdd);
                        }
                    }
                }
            }
        }

        #region View Model Events

        private void _setupViewModel()
        {
            App.ViewModel.LoadDataComplete += ViewModel_LoadDataComplete;
            App.ViewModel.LoadDataError += ViewModel_LoadDataError;
        }

        private void ViewModel_LoadDataError(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                _loadDataError();
            });
        }

        private void ViewModel_LoadDataComplete(object sender, EventArgs e)
        {
            LoadingDialog.Dispatcher.BeginInvoke(() =>
            {
                _loadDataComplete();

                if (_lastsavedgroup != "")
                {
                    Group lastgroup = null;
                    try
                    {
                        lastgroup = App.ViewModel.Items.First(g => g.Name == _lastsavedgroup);
                    }
                    catch { }
                    if (lastgroup != null)
                    {
                        PivotView.SelectedItem = lastgroup;
                    }
                    _lastsavedgroup = "";
                }
            });
        }

        #endregion


        #region Data Loading

        private void _setupLoadingDialog()
        {
            
        }

        private void _loadData()
        {
            LoadingDialogMessage.Text = AppResources.ResourceManager.GetString("MainPage_Popup_Connecting", AppResources.Culture);
            LoadingDialogProgress.Visibility = Visibility.Visible;
            LoadingDialogButtons.Visibility = Visibility.Collapsed;
            LoadingDialog.Visibility = Visibility.Visible;
            App.ViewModel.LoadData();
        }
        private void _loadDataError()
        {
            LoadingDialogMessage.Text = AppResources.ResourceManager.GetString("MainPage_Popup_ConnectionError", AppResources.Culture);
            LoadingDialogProgress.Visibility = Visibility.Collapsed;
            LoaderTop.Visibility = Visibility.Collapsed;
            LoadingDialogButtons.Visibility = Visibility.Visible;
            LoadingDialog.Visibility = Visibility.Visible;
        }
        private void _loadDataComplete()
        {
            LoadingDialog.Visibility = Visibility.Collapsed;
        }

        private void LoadingDialogSetupButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/SetupPage.xaml", UriKind.Relative));
        }

        private void LoadingDialogRetryButton_Click(object sender, RoutedEventArgs e)
        {
            _loadData();
        }

        #endregion

        private void GroupsPage_Loaded(object sender, RoutedEventArgs e)
        {
            _loadData();

            if (IsolatedStorageSettings.ApplicationSettings.Contains("EnableNotifications") && (bool)IsolatedStorageSettings.ApplicationSettings["EnableNotifications"])
            {
                App.SetupPushChannel();
            } 

            this.Dispatcher.BeginInvoke(() =>
            {
                SplashImageAnim.Begin();
            });
        }


        private void _systemStatusPoll(object target)
        {
            App.ViewModel.UpdateCurrentGroup();
            _statupdate.Change(_updateinterval, System.Threading.Timeout.Infinite);
        }

        
        private void _updateModulesData()
        {
            //PivotView.Dispatcher.BeginInvoke(() =>
            //{
                string currentgroup = App.ViewModel.Items[PivotView.SelectedIndex].Name;
                App.ViewModel.CurrentGroup = currentgroup;
                _systemStatusPoll(null);
                //
                if (_juststarted)
                {
                    _juststarted = false;
                    //foreach (Group g in App.ViewModel.Items)
                    //{
                    //    if (g.Name != currentgroup)
                    //    {
                    //        App.ViewModel._updateGroupModules(g.Name);
                    //    }
                    //}
                    App.SubscribePushNotifications();
                }
            //});
        }


        #region Page Controls Events

        private void PivotView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoaderTop.Value = 0;
            _updateModulesData();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listbox = (ListBox)sender;
            if (listbox.SelectedItem is Module)
            {
                Module module = (Module)listbox.SelectedItem;
                //
                if (module.DeviceType == Module.DeviceTypes.Dimmer || module.DeviceType == Module.DeviceTypes.Siren || module.DeviceType == Module.DeviceTypes.Switch || module.DeviceType == Module.DeviceTypes.Light)
                {
                    ModuleControls modulepopup = new ModuleControls();
                    modulepopup.Open(LayoutRoot, module);
                }
                else
                {
                    foreach (ModuleParameter p in module.Properties)
                    {
                        if (p.Name == "FavouritesLink.Url")// && (p.Value == "" || p.Value == "homegenie/generic/link"))
                        {
                            WebBrowserTask task = new WebBrowserTask();
                            task.Uri = new Uri(p.Value);
                            task.Show();
                            break;
                        }
                    }
                }
                listbox.SelectedItem = null;
            }
        }
        private void LoadingDialogHelpButton_Click(object sender, RoutedEventArgs e)
        {
            _openHelpPage();
        }

        private void ArmDisarmButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!(((FrameworkElement)sender).DataContext is Module) || ((FrameworkElement)sender).DataContext == null) return;
            Module module = (Module)((FrameworkElement)sender).DataContext;
            //
            string url = "/api/" + module.Domain + "/" + module.Address + "/Control.On//" + DateTime.Now.Ticks.ToString(); ;
            App.HttpManager.AddToQueue("Control.On", url, (WebRequestCompletedArgs args) =>
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    App.ViewModel.UpdateCurrentGroup();
                });
            });
        }

        private void ArmDisarmButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(((FrameworkElement)sender).DataContext is Module) || ((FrameworkElement)sender).DataContext == null) return;
            Module module = (Module)((FrameworkElement)sender).DataContext;
            //
            string url = "/api/" + module.Domain + "/" + module.Address + "/Control.Off//" + DateTime.Now.Ticks.ToString(); ;
            App.HttpManager.AddToQueue("Control.Off", url, (WebRequestCompletedArgs args) =>
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    App.ViewModel.UpdateCurrentGroup();
                });
            });
        }

        #endregion


        #region Application Bar events handling

        private void OpenHelp_Click(object sender, EventArgs e)
        {
            _openHelpPage();
        }

        private void OpenInfo_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private void OpenSettings_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/SetupPage.xaml", UriKind.Relative));
        }

        private void OpenAdmin_Click(object sender, EventArgs e)
        {
            Uri adminuri = new Uri(("http://" + (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerAddress"]));
            if (IsolatedStorageSettings.ApplicationSettings.Contains("RemoteServerUsername") &&
                (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerUsername"] != "" &&
                IsolatedStorageSettings.ApplicationSettings.Contains("RemoteServerPassword") &&
                (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerPassword"] != "")
            {
                adminuri = new Uri(("http://" + (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerUsername"] + ":" + (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerPassword"] + "@" + (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerAddress"]));
            }
            PhoneApplicationService.Current.State["WebBrowserPageUrl"] = adminuri;
            this.NavigationService.Navigate(new Uri("/WebBrowserPage.xaml", UriKind.Relative));
        }

        #endregion


        private void _openHelpPage()
        {
            Uri helpuri = new Uri("http://generoso.info/homegenie/learn.html");
            //PhoneApplicationService.Current.State["WebBrowserPageUrl"] = helpuri;
            //this.NavigationService.Navigate(new Uri("/WebBrowserPage.xaml", UriKind.Relative));
            WebBrowserTask task = new WebBrowserTask();
            task.Uri = helpuri;
            task.Show();
        }

        private void SplashImageAnim_Completed(object sender, EventArgs e)
        {
            ApplicationBar.IsVisible = true;
            SplashImage.Visibility = Visibility.Collapsed;
        }

        private void ApplicationBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
            if (e.IsMenuVisible)
            {
                _appbaropen = true;
            }
            else
            {
                _appbaropen = false;
            }
        }



    }
}