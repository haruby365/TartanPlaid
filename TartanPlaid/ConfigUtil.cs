// © 2021 Jong-il Hong

using Newtonsoft.Json;
using System.IO;
using System.Windows;

namespace Haruby.TartanPlaid
{
    public static class ConfigUtil
    {
        public static TConfig Load<TConfig>(Type hostType, ConfigType configType) where TConfig : new()
        {
            string filePath = GetConfigFilePath(hostType, configType);
            if (!File.Exists(filePath))
            {
                return new TConfig();
            }

            try
            {
                string text = File.ReadAllText(filePath);
                return (TConfig?)JsonConvert.DeserializeObject(text, typeof(TConfig)) ?? new TConfig();
            }
            catch
            {
                return new TConfig();
            }
        }
        public static bool Save<TConfig>(Type hostType, ConfigType configType, TConfig config)
        {
            string filePath = GetConfigFilePath(hostType, configType);

            try
            {
                Util.CreateFileDirectories(filePath);
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetConfigFilePath(Type hostType, ConfigType configType)
        {
            string personalPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            return Path.Combine(personalPath, hostType.Namespace ?? "_", configType.ToString("G") + ".json");
        }

        public static void InitWindowState(Window window, WindowStateConfig? config, WindowStartupLocation defaultStartupLocation)
        {
            void SetDefault()
            {
                window.WindowStartupLocation = defaultStartupLocation;
            }

            if (config is null)
            {
                SetDefault();
                return;
            }

            window.WindowStartupLocation = WindowStartupLocation.Manual;
            if (!SetWindowState(window, config))
            {
                SetDefault();
            }
        }
        public static bool SetWindowState(Window window, WindowStateConfig config)
        {
            if (config is null)
            {
                return false;
            }
            if (config.Width <= 1 || config.Height <= 1)
            {
                return false;
            }
            window.WindowState = WindowState.Normal;
            window.Left = config.Left;
            window.Top = config.Top;
            window.Width = config.Width;
            window.Height = config.Height;
            window.WindowState = config.WindowState;
            return true;
        }
        public static WindowStateConfig GetWindowState(Window window)
        {
            WindowStateConfig config = new();
            if (window.WindowState == WindowState.Normal)
            {
                config.Left = window.Left;
                config.Top = window.Top;
                config.Width = window.ActualWidth;
                config.Height = window.ActualHeight;
            }
            else
            {
                config.Left = window.RestoreBounds.X;
                config.Top = window.RestoreBounds.Y;
                config.Width = window.RestoreBounds.Width;
                config.Height = window.RestoreBounds.Height;
            }
            config.WindowState = window.WindowState;
            return config;
        }
    }
}
