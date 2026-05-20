using System.Windows;
using System.Windows.Controls;

namespace GuardianEye.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            MouseLeftButtonDown += (s, e) => DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}