using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using System.Net;
using System.IO.IsolatedStorage;

using System.Windows.Threading;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Linq.Expressions;

using Coding4Fun.Toolkit.Controls;

using HomeGenie.ViewModel.Objects;
using Microsoft.Phone.Shell;

namespace HomeGenie.ViewModel
{

    public class MainViewModel
    {
        public event LoadDataCompleteHandler LoadDataComplete;
        public delegate void LoadDataCompleteHandler(object sender, EventArgs e);

        public delegate void LoadDataErrorHandler(object sender, EventArgs e);
        public event LoadDataErrorHandler LoadDataError;

        public delegate void ModulesUpdatingHandler(object sender, EventArgs e);
        public event ModulesUpdatingHandler ModulesUpdating;

        public delegate void ModulesUpdatedHandler(object sender, EventArgs e);
        public event ModulesUpdatedHandler ModulesUpdated;
        
//        public delegate void LoadDataProgressHandler(object sender, EventArgs e);
//        public event LoadDataProgressHandler LoadDataProgress;

        /// <summary>
        /// Raccolta per oggetti ItemViewModel.
        /// </summary>
        public ObservableCollection<Group> Items { get; private set; }

        private Dispatcher _uidispatcher = null;
//        private int _loadingprogress = 0;

        public MainViewModel()
        {

            this.Items = new ObservableCollection<Group>();
            if (IsolatedStorageSettings.ApplicationSettings.Contains("CurrentGroupName"))
            {
                _currentgroup = IsolatedStorageSettings.ApplicationSettings["CurrentGroupName"].ToString();
            }

        }

        /// <summary>
        /// Crea e aggiunge alcuni oggetti ItemViewModel nella raccolta di elementi.
        /// </summary>
        public void LoadData()
        {

            if (!IsolatedStorageSettings.ApplicationSettings.Contains("RemoteServerAddress"))
            {
                IsolatedStorageSettings.ApplicationSettings["RemoteServerAddress"] = "127.0.0.1";
            }
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("RemoteServerUsername"))
            {
                IsolatedStorageSettings.ApplicationSettings["RemoteServerUsername"] = "admin";
            }
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("RemoteServerPassword"))
            {
                IsolatedStorageSettings.ApplicationSettings["RemoteServerPassword"] = "";
            }
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("RemoteServerUpdateInterval"))
            {
                IsolatedStorageSettings.ApplicationSettings["RemoteServerUpdateInterval"] = "20";
            }
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("EnableNotifications"))
            {
                IsolatedStorageSettings.ApplicationSettings["EnableNotifications"] = true;
            }

            Refresh();

        }

        public void Refresh()
        {
            _updateGroups();
        }

        private void _updateGroups()
        {
//            _loadingprogress = 0;
            _calljsonapi("UpdateGroups", "/api/HomeAutomation.HomeGenie/Config/Groups.List", (string jsongroups) =>
            {
                ObservableCollection<Group> groups = JsonConvert.DeserializeObject<ObservableCollection<Group>>(jsongroups);
                //
                if (groups != null)
                {
                    List<Group> newgroups = new List<Group>();
                    foreach (Group g in groups)
                    {
                        Group existinggroup = null;
                        try { existinggroup = this.Items.First(eg => eg.Name == g.Name); }
                        catch { }
                        if (existinggroup != null)
                        {
                            existinggroup.Name = g.Name;
                        }
                        else
                        {
                            Group newgroup = g;
                            newgroup.Modules.Clear();
                            //
                            newgroups.Add(newgroup);
                        }
                    }
                    //
                    this._uidispatcher.BeginInvoke(() =>
                    {
                        foreach(Group g in newgroups)
                        {
                            this.Items.Add(g);
                        }
                    });
                }
                //
                IsDataLoaded = true;
                if (LoadDataComplete != null)
                {
                    LoadDataComplete(this, new EventArgs());
                }
            });
        }

        internal void _updateGroupModules(string groupname)
        {
            if (ModulesUpdating != null)
            {
                ModulesUpdating(this, new EventArgs());
            }
            _calljsonapi("UpdateGroupModules[" + groupname + "]", "/api/HomeAutomation.HomeGenie/Config/Groups.ModulesList/" + groupname, (string jsonmodules) =>
            {
                List<Module> modules = JsonConvert.DeserializeObject<List<Module>>(jsonmodules);
                Group g = this.Items.First(hz => hz.Name == groupname);
                if (modules != null)
                this._uidispatcher.BeginInvoke(() =>
                {
                    foreach (Module m in modules)
                    {
                        Module existinmodule = null;
                        try { existinmodule = g.Modules.First(gm => gm.Domain == m.Domain && gm.Address == m.Address); }
                        catch { }
                        if (existinmodule != null)
                        {
                            existinmodule.Name = m.Name;
                            existinmodule.Properties = m.Properties;
                        }
                        else
                        {
                            g.Modules.Add(m);
                        }
                    }
                    if (ModulesUpdated != null)
                    {
                        ModulesUpdated(this, new EventArgs());
                    }
                });
            });
        }

 
        private void _calljsonapi(string reqid, string apiurl, Action<string> callback)
        {
            string url = apiurl + "/" + DateTime.Now.Ticks.ToString(); 
            App.HttpManager.AddToQueue(reqid, url, (WebRequestCompletedArgs args) =>
            {
                if (args.RequestStatus == WebRequestStatus.Completed)
                {
                    callback(args.ResponseText);
//                    _loadingprogress++;
//                    if (LoadDataProgress != null)
//                    {
//                        LoadDataProgress(this, new EventArgs());
//                    }
                    //if (_loadingprogress == this.Items.Count && LoadDataComplete != null)
                    //{
                    //    LoadDataComplete(this, new EventArgs());
                    //}
                }
                else
                {
                    if (LoadDataError != null)
                    {
                        LoadDataError(this, new EventArgs());
                    }
                }
            });
        }

        internal void SetDispatcher(Dispatcher dispatcher)
        {
            _uidispatcher = dispatcher;
        }

        internal Dispatcher Dispatcher
        {
            get { return _uidispatcher; }
        }

        public bool IsDataLoaded { get; set; }

        private string _currentgroup;
        public string CurrentGroup
        {
            get
            {
                return _currentgroup;
            }
            set
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("CurrentGroupName"))
                {
                    IsolatedStorageSettings.ApplicationSettings["CurrentGroupName"] = value;
                }
                else 
                {
                    IsolatedStorageSettings.ApplicationSettings.Add("CurrentGroupName", value);
                }
                _currentgroup = value;
            }
        }

        internal void UpdateCurrentGroup()
        {
            _updateGroupModules(CurrentGroup);
        }
    }

}