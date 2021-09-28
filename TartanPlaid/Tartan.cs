// © 2021 Jong-il Hong

using System;
using System.Collections.ObjectModel;
using System.Windows;
using Newtonsoft.Json;

namespace Haruby.TartanPlaid
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Tartan : PropertyNotifyObject
    {
        public static readonly DependencyProperty SpoolsProperty = DependencyProperty.Register(
            nameof(Spools), typeof(ObservableCollection<Spool>), typeof(Tartan), new PropertyMetadata(PropertyChangedHandler));

        [JsonProperty]
        public ObservableCollection<Spool> Spools { get => (ObservableCollection<Spool>)GetValue(SpoolsProperty); private set => SetValue(SpoolsProperty, value); }

        public Tartan()
        {
            Spools = new();
        }

        public Tartan(Tartan other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }
            Spools = new();
            foreach (Spool item in other.Spools)
            {
                Spools.Add(new(item));
            }
        }

        private void Spools_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is not null)
            {
                foreach (Spool item in e.NewItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }
            }
            if (e.OldItems is not null)
            {
                foreach (Spool item in e.OldItems)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
            }
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(Spools)));
        }

        private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }

        private void OnSpoolsChanged(ObservableCollection<Spool> prev, ObservableCollection<Spool> next)
        {
            if (prev is not null)
            {
                prev.CollectionChanged -= Spools_CollectionChanged;
            }
            if (next is null)
            {
                throw new InvalidOperationException("New spools is null.");
            }
            next.CollectionChanged += Spools_CollectionChanged;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == SpoolsProperty)
            {
                OnSpoolsChanged((ObservableCollection<Spool>)e.OldValue, (ObservableCollection<Spool>)e.NewValue);
            }
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
        public void Deserialize(string json)
        {
            Spools = new();
            JsonConvert.PopulateObject(json, this);
        }
    }
}
