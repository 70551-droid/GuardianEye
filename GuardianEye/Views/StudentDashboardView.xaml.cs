using System.Windows;

namespace GuardianEye.Views
{
    public partial class StudentDashboardView : Window
    {
        public StudentDashboardView()
        {
            InitializeComponent();
            Closing += (s, e) =>
            {
                if (DataContext is CommunityToolkit.Mvvm.ComponentModel.ObservableObject)
                {
                }
            };
        }
    }
}