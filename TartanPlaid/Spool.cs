// © 2021 Jong-il Hong

using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json;

namespace Haruby.TartanPlaid
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Spool : PropertyNotifyObject
    {
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            nameof(Color), typeof(Color), typeof(Spool), new PropertyMetadata(Colors.White, PropertyChangedHandler));
        public static readonly DependencyProperty CountProperty = DependencyProperty.Register(
            nameof(Count), typeof(int), typeof(Spool), new PropertyMetadata(1, PropertyChangedHandler));

        public Color Color { get => (Color)GetValue(ColorProperty); set => SetValue(ColorProperty, value); }
        [JsonProperty]
        public int Count { get => (int)GetValue(CountProperty); set => SetValue(CountProperty, Math.Max(1, value)); }

        [JsonProperty]
        public byte R
        {
            get => Color.R;
            set
            {
                Color color = Color;
                color.R = value;
                Color = color;
            }
        }
        [JsonProperty]
        public byte G
        {
            get => Color.G;
            set
            {
                Color color = Color;
                color.G = value;
                Color = color;
            }
        }
        [JsonProperty]
        public byte B
        {
            get => Color.B;
            set
            {
                Color color = Color;
                color.B = value;
                Color = color;
            }
        }

        public Spool()
        {

        }
        public Spool(Spool other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Color = other.Color;
            Count = other.Count;
        }
    }
}
