using System.Windows;

namespace GuardianEye.Views
{
    public partial class StudentDashboardView : Window
    {
        public StudentDashboardView()
        {
            InitializeComponent();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}