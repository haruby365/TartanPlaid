// © 2021 Jong-il Hong

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using Newtonsoft.Json;

namespace Haruby.TartanPlaid
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Tartan : PropertyNotifyObject
    {
        public static readonly DependencyProperty SpoolsProperty = DependencyProperty.Register(
            nameof(Spools), typeof(IReadOnlyList<Spool>), typeof(Tartan), new PropertyMetadata(Array.Empty<Spool>(), PropertyChangedHandler));

        public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register(
            nameof(Settings), typeof(TartanSettings), typeof(Tartan), new PropertyMetadata(TartanSettings.Default, PropertyChangedHandler));

        [JsonProperty]
        public IReadOnlyList<Spool> Spools { get => (IReadOnlyList<Spool>)GetValue(SpoolsProperty); set => SetValue(SpoolsProperty, value); }

        [JsonProperty]
        public TartanSettings Settings { get => (TartanSettings)GetValue(SettingsProperty); set => SetValue(SettingsProperty, value); }

        private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }

        private void OnSpoolsChanged(IReadOnlyList<Spool> prev, IReadOnlyList<Spool> next)
        {
            if (prev is not null)
            {
                foreach (Spool item in prev)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
            }
            if (next is null)
            {
                throw new InvalidOperationException("New spools is null.");
            }
            foreach (Spool item in next)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == SpoolsProperty)
            {
                OnSpoolsChanged((IReadOnlyList<Spool>)e.OldValue, (IReadOnlyList<Spool>)e.NewValue);
            }
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        public void Deserialize(string json)
        {
            Tartan tartan = (Tartan?)JsonConvert.DeserializeObject(json, typeof(Tartan)) ?? throw new InvalidOperationException("Deserialized null.");
            foreach (PropertyInfo info in JsonProperties)
            {
                object? v = info.GetValue(tartan);
                info.SetValue(this, v);
            }
        }

        private static readonly IReadOnlyList<PropertyInfo> JsonProperties;
        static Tartan()
        {
            JsonProperties = typeof(Tartan).GetProperties().Where(p => p.GetCustomAttribute<JsonPropertyAttribute>() is not null).ToList();
        }
    }
}
