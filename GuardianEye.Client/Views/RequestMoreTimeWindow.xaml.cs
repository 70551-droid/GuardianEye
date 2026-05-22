using System.Windows;
using System.Windows.Input;

namespace GuardianEye.Views
{
    public partial class RequestMoreTimeWindow : Window
    {
        public RequestMoreTimeWindow()
        {
            InitializeComponent();
        }

        private void MinutesTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    return;
                }
            }
        }
    }
}