using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using GuardianEye.ViewModels;
using GuardianEye.Services;

namespace GuardianEye.Views
{
    public partial class DeveloperOverlayWindow : Window
    {
        public DeveloperOverlayWindow(DeveloperOverlayViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += DeveloperOverlayWindow_Loaded;
            KeyDown += DeveloperOverlayWindow_KeyDown;
            MouseMove += (s, e) => viewModel?.HandleActivity();
        }

        private void DeveloperOverlayWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            this.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        private void DeveloperOverlayWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (DataContext is DeveloperOverlayViewModel vm)
                    vm.CloseOverlayCommand.Execute(null);
            }
            if (DataContext is DeveloperOverlayViewModel vm2)
                vm2.HandleActivity();
        }
    }
}