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

        public static readonly DependencyProperty SourceColorStateProperty = DependencyProperty.Register(
            nameof(SourceColorState), typeof(ColorState), typeof(ColorWindow));
        public static readonly DependencyProperty SelectedColorStateProperty = DependencyProperty.Register(
            nameof(SelectedColorState), typeof(ColorState), typeof(ColorWindow));

        public Color SourceColor { get => Util.ColorStateToRgb(SourceColorState); set => SourceColorState = Util.RgbToColorState(value); }
        public Color SelectedColor { get => Util.ColorStateToRgb(SelectedColorState); set => SelectedColorState = Util.RgbToColorState(value); }

        public IReadOnlyList<ColorItem> OtherColors { get => (IReadOnlyList<ColorItem>)GetValue(OtherColorsProperty); set => SetValue(OtherColorsProperty, value); }

        private ColorState SourceColorState { get => (ColorState)GetValue(SourceColorStateProperty); set => SetValue(SourceColorStateProperty, value); }
        private ColorState SelectedColorState { get => (ColorState)GetValue(SelectedColorStateProperty); set => SetValue(SelectedColorStateProperty, value); }

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
