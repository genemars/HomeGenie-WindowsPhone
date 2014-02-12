using HomeGenie.ViewModel.Objects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows.Controls;
using System.Windows.Media;

namespace HomeGenie.ViewModel.Converters
{
    public class LatestPropertyDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string updated = "";
            try
            {
                DateTime lastdate = DateTime.MinValue;
                foreach (ModuleParameter p in (ObservableCollection<ModuleParameter>)value)
                {
                    if (p.UpdateTime > lastdate)
                    {
                        lastdate = p.UpdateTime;
                    }
                }
                if (lastdate != DateTime.MinValue)
                {
                    return lastdate.ToLocalTime().ToLongDateString() + " " + lastdate.ToLocalTime().ToLongTimeString();
                }
            }
            catch (Exception)
            {
            }
            return updated;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HsbColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color color = new Color();
            try
            {
                foreach (ModuleParameter p in (ObservableCollection<ModuleParameter>)value)
                {
                    if (p.Name == (string)parameter && p.Value != null && p.Value != "")
                    {
                        string[] colors = p.Value.Split(',');
                        if (colors.Length == 3)
                        {
                            double h = 0;
                            double s = 0;
                            double b = 0;
                            if (double.TryParse(colors[0], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out h) &&
                                double.TryParse(colors[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out s) &&
                                double.TryParse(colors[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out b))
                            {
                                Utility.HSBColor hsbcolor = new Utility.HSBColor();
                                hsbcolor.A = 255;
                                hsbcolor.H = h * 360d;
                                hsbcolor.S = s;
                                hsbcolor.B = b;
                                color = hsbcolor.ToColor();
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PropertyAddinVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility resvis = Visibility.Collapsed;
            string types = ":" + ((string)parameter) + ":";
            try
            {
                bool hidesecuritylevel = false;
                foreach (ModuleParameter p in (ObservableCollection<ModuleParameter>)value)
                {
                    if (p.Name == (string)parameter && p.Value != null && p.Value != "")
                    {
                        resvis = Visibility.Visible;
                        //break;
                    }
                    else if (p.Name == "HomeGenie.SecurityArmed" && (string)parameter == "Status.Level")
                    {
                        hidesecuritylevel = true;
                    }
                }
                if (hidesecuritylevel) resvis = Visibility.Collapsed;
            }
            catch (Exception)
            {
            }
            return resvis;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SecurityArmedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isarmed = false;
            bool isarming = false;
            string returnstring = "DISARMED";

            foreach (ModuleParameter p in (ObservableCollection<ModuleParameter>)value)
            {
                if (p.Name == "HomeGenie.SecurityArmed")
                {
                    isarmed = (p.Value == "1");
                }
                else if (p.Name == "Status.Level")
                {
                    isarming = (p.Value == "1");
                }
            }
            if (isarmed) returnstring = "ARMED";
            else if (isarming) returnstring = "ARMING...";

            return returnstring;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ModuleVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Module module = (Module)value;
            Visibility resvis = Visibility.Visible;
            if (module.DeviceType == Module.DeviceTypes.Program)
            {
                foreach (ModuleParameter p in module.Properties)
                {
                    if (p.Name == "Widget.DisplayModule" && (p.Value == "" || p.Value == "homegenie/generic/program"))
                    {
                        resvis = Visibility.Collapsed;
                        break;
                    }
                }
            }
            return resvis;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ModuleDomainConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string domain = (string)value;
            if (domain.IndexOf('.') > 0)
            {
                domain = domain.Substring(domain.LastIndexOf('.') + 1);
            }
            return domain;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ModuleAddinVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility resvis = Visibility.Collapsed;
            string types = ":" + ((string)parameter) + ":";
            if (types.Contains(value.ToString()))
            {
                resvis = Visibility.Visible;
            }
            return resvis;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ModulePropertyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object param, CultureInfo culture)
        {
            bool addunitsymbol = false;
            object resval = null;
            try
            {
                string parameter = (string)param;
                if (parameter.EndsWith("+"))
                {
                    addunitsymbol = true;
                    parameter = parameter.TrimEnd('+');
                }
                foreach (ModuleParameter p in (ObservableCollection<ModuleParameter>)value)
                {
                    if (p.Name == parameter)
                    {
                        double val = 0;
                        if (double.TryParse(p.Value, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out val))
                        {
                            resval = Math.Round(val, 2);
                            if (addunitsymbol)
                            {
                                switch (p.Name)
                                {
                                    case "Sensor.Temperature":
                                        resval = resval.ToString() + " °C";
                                        break;
                                    case "Sensor.TemperatureF":
                                        resval = resval.ToString() + " F";
                                        break;
                                    case "Meter.Watts":
                                        resval = resval.ToString() + " W";
                                        break;
                                    case "Status.Level":
                                        resval = (double)resval * 100d;
                                        switch ((int)((double)resval))
                                        {
                                            case 0:
                                                resval = "OFF";
                                                break;
                                            case 100:
                                                resval = "ON";
                                                break;
                                            default:
                                                resval = resval.ToString() + " %";
                                                break;
                                        }
                                        break;
                                    case "Status.Battery":
                                    case "Sensor.Luminance":
                                    case "Sensor.Humidity":
                                        resval = resval.ToString() + " %";
                                        break;
                                }
                            }
                        }
                        else
                        {
                            resval = p.Value;
                        }
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }
            return resval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ModuleIconUrlConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string url = (string)values[1];
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {

                return new BitmapImage(new Uri(url));
            }
            else if (url.StartsWith("/Assets/"))
            {
                return new BitmapImage(new Uri(url, UriKind.Relative));
            }
            else
            {
                string baseurl = "http://" + (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerAddress"];
                HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create(baseurl + url);
                if (IsolatedStorageSettings.ApplicationSettings.Contains("RemoteServerUsername") &&
                    (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerUsername"] != "" &&
                    IsolatedStorageSettings.ApplicationSettings.Contains("RemoteServerPassword") &&
                    (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerPassword"] != "")
                {
                    wreq.Credentials = new NetworkCredential((string)IsolatedStorageSettings.ApplicationSettings["RemoteServerUsername"], (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerPassword"]);
                }
                //wreq.AllowWriteStreamBuffering = true;

                wreq.BeginGetResponse((IAsyncResult result) => {
                    Image img = (Image)result.AsyncState;
                    try
                    {
                        HttpWebResponse res = (HttpWebResponse)wreq.EndGetResponse(result);
                        img.Dispatcher.BeginInvoke(() =>
                        {
                            BitmapImage bi = new BitmapImage();
                            bi.SetSource(res.GetResponseStream());
                            img.Source = bi;
                        });
                    }
                    catch { }
                }, (Image)values[0]);

                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }     
    }
}
