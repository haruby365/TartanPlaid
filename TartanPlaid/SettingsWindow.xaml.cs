// © 2021 Jong-il Hong

using System;
using System.Windows;

namespace Haruby.TartanPlaid
{
    /// <summary>
    /// SettingsWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public static readonly DependencyProperty InputUnitWidthProperty = DependencyProperty.Register(
            nameof(InputUnitWidth), typeof(int), typeof(SettingsWindow));
        public static readonly DependencyProperty InputRepeatProperty = DependencyProperty.Register(
            nameof(InputRepeat), typeof(int), typeof(SettingsWindow));

        public TartanSettings SelectedSettings
        {
            get => new(InputUnitWidth, InputRepeat);
            set
            {
                value ??= TartanSettings.Default;
                InputUnitWidth = value.UnitWidth;
                InputRepeat = value.Repeat;
            }
        }

        private int InputUnitWidth { get => (int)GetValue(InputUnitWidthProperty); set => SetValue(InputUnitWidthProperty, value); }
        private int InputRepeat { get => (int)GetValue(InputRepeatProperty); set => SetValue(InputRepeatProperty, value); }

        public SettingsWindow()
        {
            SelectedSettings = TartanSettings.Default;

            InitializeComponent();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnInputUnitWidthChanged(int prev, int next)
        {
            InputUnitWidth = Math.Max(1, next);
        }
        private void OnInputRepeatChanged(int prev, int next)
        {
            InputRepeat = Math.Max(1, next);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == InputUnitWidthProperty)
            {
                OnInputUnitWidthChanged((int)e.OldValue, (int)e.NewValue);
            }
            else if (e.Property == InputRepeatProperty)
            {
                OnInputRepeatChanged((int)e.OldValue, (int)e.NewValue);
            }
        }
    }
}
