// © 2021 Jong-il Hong

using System.Windows.Input;

namespace Haruby.TartanPlaid
{
    public class ListBox : System.Windows.Controls.ListBox
    {
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            switch (SelectionMode)
            {
                case System.Windows.Controls.SelectionMode.Multiple:
                case System.Windows.Controls.SelectionMode.Extended:
                    SelectedItems.Clear();
                    break;

                default:
                    SelectedItem = null;
                    break;
            }
        }
    }
}
