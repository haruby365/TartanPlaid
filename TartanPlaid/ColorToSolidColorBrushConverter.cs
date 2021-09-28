// © 2021 Jong-il Hong

using System;
using System.Windows.Data;
using System.Windows.Media;

namespace Haruby.TartanPlaid
{
    public class ColorToSolidColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is Color color ? new SolidColorBrush(color) : throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is SolidColorBrush brush ? brush.Color : throw new NotSupportedException();
        }
    }
}
