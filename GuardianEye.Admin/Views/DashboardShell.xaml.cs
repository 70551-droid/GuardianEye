using System.Windows;
using System.Windows.Controls;
using GuardianEye.Admin.Views.Pages;

namespace GuardianEye.Admin.Views
{
    public partial class DashboardShell : Window
    {
        public DashboardShell()
        {
            InitializeComponent();
            // Set default page to Dashboard
            MainContentControl.Content = new DashboardPage();
            DashboardItem.IsSelected = true;
        }

        private void NavigationListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavigationListView.SelectedItem is ListViewItem selectedItem)
            {
                switch (selectedItem.Name)
                {
                    case "DashboardItem":
                        MainContentControl.Content = new DashboardPage();
                        break;
                    case "StudentsItem":
                        MainContentControl.Content = new StudentsPage();
                        break;
                    case "MonitoringItem":
                        MainContentControl.Content = new MonitoringPage();
                        break;
                    case "RestrictionsItem":
                        MainContentControl.Content = new RestrictionsPage();
                        break;
                    case "ReportsItem":
                        MainContentControl.Content = new ReportsPage();
                        break;
                    case "DevicesItem":
                        MainContentControl.Content = new DevicesPage();
                        break;
                    case "SettingsItem":
                        MainContentControl.Content = new SettingsPage();
                        break;
                }
            }
        }
    }
}