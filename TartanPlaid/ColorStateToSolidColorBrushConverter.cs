// © 2021 Jong-il Hong

using System;
using System.Windows.Data;
using System.Windows.Media;
using ColorPicker.Models;

namespace Haruby.TartanPlaid
{
    public class ColorStateToSolidColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is ColorState color ? new SolidColorBrush(Color.FromArgb((byte)(color.A * byte.MaxValue), (byte)(color.RGB_R * byte.MaxValue), (byte)(color.RGB_G * byte.MaxValue), (byte)(color.RGB_B * byte.MaxValue))) : throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is not SolidColorBrush brush)
            {
                throw new NotSupportedException();
            }
            Color color = brush.Color;
            ColorState colorState = new();
            colorState.SetARGB(color.A / (double)byte.MaxValue, color.R / (double)byte.MaxValue, color.G / (double)byte.MaxValue, color.B / (double)byte.MaxValue);
            return colorState;
        }
    }
}
