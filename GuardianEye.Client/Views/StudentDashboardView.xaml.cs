using System.Windows;
using System.Windows.Input;

namespace GuardianEye.Views
{
    public partial class StudentDashboardView : Window
    {
        public StudentDashboardView()
        {
            InitializeComponent();
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                    Close();
            };
        }
    }
}