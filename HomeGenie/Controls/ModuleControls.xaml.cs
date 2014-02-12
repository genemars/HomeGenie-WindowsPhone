using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HomeGenie.ViewModel.Objects;
using System.IO.IsolatedStorage;
using System.Globalization;
using HomeGenie.ViewModel.Converters;
using System.Windows.Media;
using System.Windows.Threading;

namespace HomeGenie.Controls
{
    public partial class ModuleControls : UserControl
    {
        private DispatcherTimer _submitcommanddelay;

        public ModuleControls()
        {
            InitializeComponent();
            //
            _submitcommanddelay = new DispatcherTimer();
            _submitcommanddelay.Interval = TimeSpan.FromMilliseconds(100);
            _submitcommanddelay.Tick += _submitcommanddelay_Tick;
            //
            this.SizeChanged += ModuleControls_SizeChanged;
        }

        void ModuleControls_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ColorPickerBox.Clip = new RectangleGeometry() { Rect = new Rect(0, 0, ColorPickerBox.ActualWidth, ColorPickerBox.ActualHeight) };
        }

        private void ModuleOn_Click(object sender, RoutedEventArgs e)
        {
            if (!(((FrameworkElement)sender).DataContext is Module) || ((FrameworkElement)sender).DataContext == null) return;
            Module module = (Module)((FrameworkElement)sender).DataContext;
            //
            string url = "/api/" + module.Domain + "/" + module.Address + "/Control.On//" + DateTime.Now.Ticks.ToString(); ;
            App.HttpManager.AddToQueue("Control.On", url, (WebRequestCompletedArgs args) =>
            {
                //this.Dispatcher.BeginInvoke(() =>
                //{
                    App.ViewModel.UpdateCurrentGroup();
                //});
            });
        }

        private void ModuleOff_Click(object sender, RoutedEventArgs e)
        {
            if (!(((FrameworkElement)sender).DataContext is Module) || ((FrameworkElement)sender).DataContext == null) return;
            Module module = (Module)((FrameworkElement)sender).DataContext;
            //
            string url = "/api/" + module.Domain + "/" + module.Address + "/Control.Off//" + DateTime.Now.Ticks.ToString(); ;
            App.HttpManager.AddToQueue("Control.Off", url, (WebRequestCompletedArgs args) =>
            {
                //this.Dispatcher.BeginInvoke(() =>
                //{
                    App.ViewModel.UpdateCurrentGroup();
                //});
            });
        }

        private void ModuleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!(((FrameworkElement)sender).DataContext is Module) || ((FrameworkElement)sender).DataContext == null) return;
            Module module = (Module)((FrameworkElement)sender).DataContext;
            //
            foreach (ModuleParameter p in module.Properties)
            {
                if (p.Name == "Status.Level")
                {
                    p.Value = e.NewValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
                module.OnPropertyChanged("Properties");
            }
        }

        private void ModuleSlider_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!(((FrameworkElement)sender).DataContext is Module) || ((FrameworkElement)sender).DataContext == null) return;
            Module module = (Module)((FrameworkElement)sender).DataContext;
            e.Handled = true;
            //
            Slider slider = (Slider)sender;
            string url = "/api/" + module.Domain + "/" + module.Address + "/Control.Level/" + Math.Round(slider.Value * 100).ToString() + "/" + DateTime.Now.Ticks.ToString(); ;
            App.HttpManager.AddToQueue("Control.Level", url, (WebRequestCompletedArgs args) =>
            {
                //this.Dispatcher.BeginInvoke(() =>
                //{
                    App.ViewModel.UpdateCurrentGroup();
                //});
            });
        }

        private void ColorPicker_ColorChanged(object sender, Color color)
        {
            if (!(((FrameworkElement)sender).DataContext is Module) || ((FrameworkElement)sender).DataContext == null) return;
            //
            ColorSlider.Color = ColorPicker.Color;
            //
            _submitColorChange();
        }

        private void ColorSlider_ColorChanged(object sender, Color color)
        {
            Utility.HSBColor hsbcolorcp = Utility.HSBColor.FromColor(ColorPicker.Color);
            Utility.HSBColor hsbcolorcs = Utility.HSBColor.FromColor(color);
            hsbcolorcp.H = hsbcolorcs.H;
            //hsbcolorcp.S = hsbcolorcs.S;
            ColorPicker.Color = hsbcolorcp.ToColor();
            //
            _submitColorChange();
        }

        private void _submitColorChange()
        {
            _submitcommanddelay.Stop();
            _submitcommanddelay.Start();
        }

        private void _submitcommanddelay_Tick(object sender, EventArgs e)
        {
            _submitcommanddelay.Stop();
            Module module = (Module)((FrameworkElement)this).DataContext;
            //
            Utility.HSBColor hsbcolor = Utility.HSBColor.FromColor(ColorPicker.Color);
            //
            string url = "/api/" + module.Domain + "/" + module.Address + "/Control.ColorHsb/" +
                (hsbcolor.H / 360d).ToString(CultureInfo.InvariantCulture) + ',' +
                (hsbcolor.S).ToString(CultureInfo.InvariantCulture) + ',' +
                (hsbcolor.B).ToString(CultureInfo.InvariantCulture) + "/" + DateTime.Now.Ticks.ToString();
            App.HttpManager.AddToQueue("Control.ColorHsb", url, (WebRequestCompletedArgs args) =>
            {
                //this.Dispatcher.BeginInvoke(() =>
                //{
                    App.ViewModel.UpdateCurrentGroup();
                //});
            });
        }


        public void Open(Panel parent, Module module)
        {
            this.DataContext = module;
            HsbColorConverter cc = new HsbColorConverter();
            Color lightcolor = (Color)cc.Convert(module.Properties, null, "Status.ColorHsb", CultureInfo.InvariantCulture);
            if (lightcolor != null)
            {
                this.ColorPicker.Color = lightcolor;
                this.ColorSlider.Color = lightcolor;
            }
            //
            LayoutRoot.Opacity = 0;
            parent.Children.Add(this);
            //
            this.Dispatcher.BeginInvoke(() =>
            {
                ModuleControlPopupAnim.Begin();
            });
        }

        private void PopupClose_Click(object sender, RoutedEventArgs e)
        {
            //this.Visibility = System.Windows.Visibility.Collapsed;
            ((Panel)this.Parent).Children.Remove(this);
        }

    }
}
