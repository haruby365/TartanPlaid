// © 2021 Jong-il Hong

using System.Windows;
using System.Windows.Media;

namespace Haruby.TartanPlaid
{
    /// <summary>
    /// ColorWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ColorWindow : Window
    {
        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
            nameof(SelectedColor), typeof(Color), typeof(ColorWindow), new PropertyMetadata(Colors.Black));

        public static readonly DependencyProperty InputHexTextProperty = DependencyProperty.Register(
            nameof(InputHexText), typeof(string), typeof(ColorWindow));

        public Color SelectedColor { get => (Color)GetValue(SelectedColorProperty); set => SetValue(SelectedColorProperty, value); }

        private string InputHexText { get => (string)GetValue(InputHexTextProperty); set => SetValue(InputHexTextProperty, value); }

        public ColorWindow()
        {
            InitializeComponent();

            SelectedColor = Colors.White;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnSelectedColorChanged(Color prev, Color next)
        {
            HexTextBox.Text = $"#{next.R:x2}{next.G:x2}{next.B:x2}";
        }
        private void OnInputHexTextChanged(string prev, string next)
        {
            if (string.IsNullOrEmpty(next))
            {
                return;
            }
            if (next[0] != '#')
            {
                next = '#' + next;
            }
            try
            {
                SelectedColor = (Color)ColorConverter.ConvertFromString(next);
            }
            catch
            {

            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == SelectedColorProperty)
            {
                OnSelectedColorChanged((Color)e.OldValue, (Color)e.NewValue);
            }
            else if (e.Property == InputHexTextProperty)
            {
                OnInputHexTextChanged((string)e.OldValue, (string)e.NewValue);
            }
        }
    }
}
