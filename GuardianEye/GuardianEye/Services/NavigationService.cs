using System.Windows;
using System.Windows.Controls;

namespace GuardianEye.Services
{
    public interface INavigationService
    {
        void NavigateTo<T>() where T : class;
        void NavigateTo<T>(object parameter) where T : class;
        void GoBack();
        Frame? CurrentFrame { get; set; }
    }

    public class NavigationService : INavigationService
    {
        private readonly Dictionary<Type, Type> _viewMapping = new();

        public Frame? CurrentFrame { get; set; }

        public NavigationService()
        {
            _viewMapping[typeof(LoginViewModel)] = typeof(Views.LoginWindow);
            _viewMapping[typeof(AdminDashboardViewModel)] = typeof(Views.AdminDashboardView);
            _viewMapping[typeof(StudentDashboardViewModel)] = typeof(Views.StudentDashboardView);
        }

        public void NavigateTo<T>() where T : class
        {
            NavigateTo<T>(null);
        }

        public void NavigateTo<T>(object parameter) where T : class
        {
            var viewType = _viewMapping.GetValueOrDefault(typeof(T));
            if (viewType == null || CurrentFrame == null) return;

            var view = Activator.CreateInstance(viewType);
            if (view is Window window)
            {
                if (CurrentFrame.NavigationService?.Content is Window currentWindow)
                    currentWindow.Close();
                window.Show();
            }
            else if (view is UserControl control)
            {
                CurrentFrame.Navigate(control);
            }
        }

        public void GoBack()
        {
            if (CurrentFrame?.CanGoBack == true)
                CurrentFrame.GoBack();
        }
    }
}