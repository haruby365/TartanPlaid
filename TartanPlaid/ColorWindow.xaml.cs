// © 2021 Jong-il Hong

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ColorPicker.Models;

namespace Haruby.TartanPlaid
{
    /// <summary>
    /// ColorWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ColorWindow : Window
    {
        public static readonly DependencyProperty OtherColorsProperty = DependencyProperty.Register(
            nameof(OtherColors), typeof(IReadOnlyList<ColorItem>), typeof(ColorWindow), new PropertyMetadata(Array.Empty<ColorItem>()));

        public static readonly DependencyProperty ColorStateProperty = DependencyProperty.Register(
            nameof(ColorState), typeof(ColorState), typeof(ColorWindow));

        public Color SelectedColor
        {
            get
            {
                ColorState colorState = ColorState;
                return Color.FromArgb(byte.MaxValue, (byte)(colorState.RGB_R * byte.MaxValue), (byte)(colorState.RGB_G * byte.MaxValue), (byte)(colorState.RGB_B * byte.MaxValue));
            }
            set
            {
                ColorState colorState = new();
                colorState.SetARGB(1d, value.R / (double)byte.MaxValue, value.G / (double)byte.MaxValue, value.B / (double)byte.MaxValue);
                ColorState = colorState;
            }
        }

        public IReadOnlyList<ColorItem> OtherColors { get => (IReadOnlyList<ColorItem>)GetValue(OtherColorsProperty); set => SetValue(OtherColorsProperty, value); }

        private ColorState ColorState { get => (ColorState)GetValue(ColorStateProperty); set => SetValue(ColorStateProperty, value); }

        public ColorWindow()
        {
            InitializeComponent();

            SelectedColor = Colors.White;
        }

        private void OtherColorsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ColorItem? colorItem = (ColorItem?)OtherColorsListBox.SelectedItem;
            if (colorItem is null)
            {
                return;
            }
            SelectedColor = colorItem.Color;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
