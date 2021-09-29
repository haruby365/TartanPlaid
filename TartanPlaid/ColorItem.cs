// © 2021 Jong-il Hong

using System.Windows;
using System.Windows.Media;

namespace Haruby.TartanPlaid
{
    public class ColorItem : DependencyObject
    {
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            nameof(Color), typeof(Color), typeof(ColorItem));
        public static readonly DependencyProperty HexProperty = DependencyProperty.Register(
            nameof(Hex), typeof(string), typeof(ColorItem));

        public Color Color { get => (Color)GetValue(ColorProperty); private set => SetValue(ColorProperty, value); }
        public string Hex { get => (string)GetValue(HexProperty); private set => SetValue(HexProperty, value); }

        public ColorItem(Color color)
        {
            Color = color;
            Hex = Util.RgbColorToHex(color);
        }
    }
}
