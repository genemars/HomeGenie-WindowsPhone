using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Globalization;

namespace HomeGenie.ViewModel.Converters
{
    public class IntToVisibilityConverter : IValueConverter
    {

        /// <exception cref="ArgumentException">TargetType must be Visibility</exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int))
                throw new ArgumentException("Source must be of type int");

            if (targetType != typeof(Visibility))
                throw new ArgumentException("TargetType must be Visibility");

            int v = (int)value;

            if (v > 0)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }    
    
    }
}

