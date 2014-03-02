using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.ObjectModel;

namespace HomeGenie.ViewModel.Objects
{


    public class Module : Data
    {
        private string _name;
        private DeviceTypes _devicetype;
        public string Name { 
            get
            {
                return _name;
            }
            set
            {
                SetField(ref _name, value, "Name");
            }
        }
        public string Description { get; set; }
        //[JsonConverter(typeof(StringEnumConverter))]
        //public Types Type { get; set; } //physical control type (on/off, 0-100, Hot/Cold, InputSensor, etc.)
        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceTypes DeviceType 
        { 
            get
            {
                return _devicetype;
            }
            set
            {
                _devicetype = value;
                OnPropertyChanged("DeviceType");
                OnPropertyChanged("IconUrl");
            }
        } //will indicate actual device (lamp, fan, dimmer light, etc.)

        // location in actual physical Control-topology
        public string Domain { get; set; } // only Domain is used. Interface should be used instead?
        //public string Interface { get; set; }
        public string Address { get; set; }
        //
        private ObservableCollection<ModuleParameter> _properties;
        public ObservableCollection<ModuleParameter> Properties 
        { 
            get
            {
                return _properties;
            }
            set
            {
                SetField(ref _properties, value, "Properties");
                OnPropertyChanged("IconUrl");
            }
        }
        //
        public string IconUrl
        {
            get
            {
                //
                // collects module fields from module.Properties
                //
                double level = 0;
                string statussuffix = "";
                string widget = "";
                double doorwindow = 0;
                try
                {
                    foreach (ModuleParameter p in this.Properties)
                    {
                        if (p.Name == "Status.Level")
                        {
                            double.TryParse(p.Value, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out level);
                        }
                        else if (p.Name == "Widget.DisplayModule")
                        {
                            widget = p.Value;
                        }
                        else if (p.Name == "Sensor.DoorWindow")
                        {
                            double.TryParse(p.Value, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out doorwindow);
                        }
                    }
                    //
                    if (level != 0D || doorwindow != 0D)
                    {
                        statussuffix = "on";
                    }
                    else
                    {
                        statussuffix = "off";
                    }
                }
                catch (Exception)
                {
                }


                string imageurl = "/hg/html/pages/control/widgets/homegenie/generic/images/program.png";

                if (widget != "")
                {
                    switch (widget)
                    {
                        case "weather/earthtools/sundata":
                            imageurl = "/hg/html/pages/control/widgets/weather/earthtools/images/earthtools.png";
                            break;
                        case "weather/wunderground/conditions":
                            ModuleParameter weathericon = null;
                            try { weathericon = Properties.First(mp => mp.Name == "Conditions.IconUrl"); }
                            catch { }
                            if (weathericon != null)
                            {
                                string fname = weathericon.Value.Substring(weathericon.Value.LastIndexOf('/') + 1);
                                fname = fname.Replace(".gif", "");
                                if (fname.StartsWith("nt_"))
                                {
                                    fname = "/Assets/WeatherIcons/night/" + fname.Replace("nt_", "") + ".png";
                                }
                                else
                                {
                                    fname = "/Assets/WeatherIcons/day/" + fname + ".png";
                                }
                                imageurl = fname;
                            }
                            break;
                        case "homegenie/generic/link":
                            imageurl = "/hg/html/pages/control/widgets/homegenie/generic/images/link.png";
                            break;
                        case "homegenie/generic/sensor":
                            imageurl = "/hg/html/pages/control/widgets/homegenie/generic/images/sensor.png";
                            break;
                        case "homegenie/generic/doorwindow":
                            if (level != 0 || doorwindow != 0)
                                imageurl = "/hg/html/pages/control/widgets/homegenie/generic/images/door_open.png";
                            else
                                imageurl = "/hg/html/pages/control/widgets/homegenie/generic/images/door_closed.png";
                            break;
                        case "homegenie/generic/siren":
                            imageurl = "/hg/html/pages/control/widgets/homegenie/generic/images/siren.png";
                            break;
                        case "homegenie/generic/temperature":
                            imageurl = "/hg/html/pages/control/widgets/homegenie/generic/images/temperature.png";
                            break;
                        case "homegenie/generic/securitysystem":
                            imageurl = "/hg/html/pages/control/widgets/homegenie/generic/images/securitysystem.png";
                            break;
                        case "homegenie/generic/switch":
                            imageurl = "/hg/html/pages/control/widgets/homegenie/generic/images/socket_" + statussuffix + ".png";
                            break;
                        case "homegenie/generic/light":
                        case "homegenie/generic/dimmer":
                        case "homegenie/generic/colorlight":
                            imageurl = "/hg/html/pages/control/widgets/homegenie/generic/images/light_" + statussuffix + ".png";
                            break;
                    }
                }
                else
                {
                    // no widget specified, device type fallback
                    var type = this.DeviceType;
                    switch (type)
                    {
                        case Module.DeviceTypes.Light:
                        case Module.DeviceTypes.Dimmer:
                            imageurl = "/hg/html/pages/control/widgets/homegenie/generic/images/light_" + statussuffix + ".png";
                            break;
                        case Module.DeviceTypes.Switch:
                            imageurl = "/hg/html/pages/control/widgets/homegenie/generic/images/socket_" + statussuffix + ".png";
                            break;
                        case Module.DeviceTypes.Sensor:
                            imageurl = "/hg/html/pages/control/widgets/homegenie/generic/images/sensor.png";
                            break;
                        case Module.DeviceTypes.DoorWindow:
                            if (statussuffix == "on") statussuffix = "open"; else statussuffix = "closed";
                            imageurl = "/hg/html/pages/control/widgets/homegenie/generic/images/door_" + statussuffix + ".png";
                            break;
                        case Module.DeviceTypes.Temperature:
                            imageurl = "/hg/html/pages/control/widgets/homegenie/generic/images/temperature.png";
                            break;
                        case Module.DeviceTypes.Siren:
                            imageurl = "/hg/html/pages/control/widgets/homegenie/generic/images/siren.png";
                            break;
                    }
                }
                return imageurl;
            }
        }
        //
        public string RoutingNode { get; set; } // "<ip>:<port>" || ""
        //
        public Module()
        {
            Name = "";
            Properties = new ObservableCollection<ModuleParameter>();
            RoutingNode = "";
        }
        /*
        public enum Types
        {
            Generic = -1,
            BinarySwitch,
            MultiLevelSwitch,
            Thermostat,
            InputSensor
        }
        */
        public enum DeviceTypes
        {
            Generic = -1,
            Program,
            Switch,
            Light,
            Dimmer,
            Sensor,
            Temperature,
            Siren,
            Fan,
            Thermostat,
            Shutter,
            DoorWindow
            //siren, alarm, motion sensor, door sensor, thermal sensor, etc.
        }

    }

}
