using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace HomeGenie.ViewModel.Objects
{
    public class Group
    {
        private string _name = "";
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        //
        private ObservableCollection<Module> _modules;
        public ObservableCollection<Module> Modules 
        {
            get { return _modules; }
            set { _modules = value; /* SetField(ref _modules, value, "Modules"); */ } 
        }
        //
        public Group()
        {
            Modules = new ObservableCollection<Module>();
        }
    }
}
