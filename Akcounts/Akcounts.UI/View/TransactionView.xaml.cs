using System.Windows.Controls;
using System.Windows.Input;

namespace Akcounts.UI.View
{
    public partial class TransactionView
    {
        public TransactionView()
        {
            InitializeComponent();
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var textbox = sender as TextBox;

            if (textbox != null)
            {
                textbox.SelectAll();
            }
        }
    }
}
