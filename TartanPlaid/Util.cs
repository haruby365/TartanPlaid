// © 2021 Jong-il Hong

using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using ColorPicker.Models;

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
        public static ColorState RgbToColorState(Color color)
        {
            ColorState colorState = default;
            colorState.SetARGB(1d, color.R / (double)byte.MaxValue, color.G / (double)byte.MaxValue, color.B / (double)byte.MaxValue);
            return colorState;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color ColorStateToRgb(ColorState colorState)
        {
            return Color.FromRgb((byte)Math.Round(colorState.RGB_R * byte.MaxValue), (byte)Math.Round(colorState.RGB_G * byte.MaxValue), (byte)Math.Round(colorState.RGB_B * byte.MaxValue));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OpenSystemWebBrowser(string uri)
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true, });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateDirectories(string directory)
        {
            CreateDirectories(new DirectoryInfo(directory));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateDirectories(DirectoryInfo directory)
        {
            if (directory is null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            Stack<DirectoryInfo> dirs = new();
            DirectoryInfo? dir = directory;
            while (dir is not null && !dir.Exists)
            {
                dirs.Push(dir);
                dir = dir.Parent;
            }
            while (dirs.Count > 0)
            {
                dirs.Pop().Create();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateFileDirectories(string file)
        {
            CreateFileDirectories(new FileInfo(file));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateFileDirectories(FileInfo file)
        {
            CreateDirectories(file.Directory ?? throw new ArgumentException("File has not directory."));
        }
    }
}
