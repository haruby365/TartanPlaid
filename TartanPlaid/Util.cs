// © 2021 Jong-il Hong

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Haruby.TartanPlaid
{
    public static class Util
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string RgbColorToHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryHexToRgbColor(string hex, out Color color)
        {
            if (string.IsNullOrEmpty(hex))
            {
                color = default;
                return false;
            }
            if (hex[0] != '#')
            {
                hex = '#' + hex;
            }
            try
            {
                color = (Color)ColorConverter.ConvertFromString(hex);
                color.A = byte.MaxValue;
                return true;
            }
            catch
            {

            }
            color = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OpenSystemWebBrowser(string uri)
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true, });
        }
    }
}
