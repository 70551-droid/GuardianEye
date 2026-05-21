using System.Windows;
using System.Windows.Controls;
using GuardianEye.ViewModels;
using GuardianEye.Views;

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
            _viewMapping[typeof(GuardianEye.ViewModels.LoginViewModel)] = typeof(GuardianEye.Views.LoginWindow);
            _viewMapping[typeof(GuardianEye.ViewModels.AdminDashboardViewModel)] = typeof(GuardianEye.Views.AdminDashboardView);
            _viewMapping[typeof(GuardianEye.ViewModels.StudentDashboardViewModel)] = typeof(GuardianEye.Views.StudentDashboardView);
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