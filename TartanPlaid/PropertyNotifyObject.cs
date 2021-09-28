// © 2021 Jong-il Hong

using System.ComponentModel;
using System.Windows;

namespace Haruby.TartanPlaid
{
    public class PropertyNotifyObject : DependencyObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected static void PropertyChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PropertyNotifyObject notifyObject = (PropertyNotifyObject)d;
            notifyObject.OnPropertyChanged(new PropertyChangedEventArgs(e.Property.Name));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
