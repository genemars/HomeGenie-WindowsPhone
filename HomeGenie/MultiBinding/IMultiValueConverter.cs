// This code is licensed under a Creative Commons Attribution 3.0 Unported License. Any work that includes this code must provide attribution
// by means of a link. It is suggested that commercial applications that make use of code from this blog include a reference to the relevant blog post in their 'about' page.


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
using System.Globalization;

namespace System.Windows.Data
{
  /// <summary>
  /// see: http://msdn.microsoft.com/en-us/library/system.windows.data.imultivalueconverter.aspx
  /// </summary>
  public interface IMultiValueConverter
  {
    object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);

    object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture);

  }
}
