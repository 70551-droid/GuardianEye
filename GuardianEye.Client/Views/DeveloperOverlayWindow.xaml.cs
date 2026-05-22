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
        private readonly DeveloperOverlayViewModel? _viewModel;

        public DeveloperOverlayWindow(DeveloperOverlayViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            _viewModel = viewModel;
            Loaded += DeveloperOverlayWindow_Loaded;
            KeyDown += DeveloperOverlayWindow_KeyDown;
            MouseMove += (s, e) => _viewModel?.HandleActivity();
            PreviewMouseDown += DeveloperOverlayWindow_PreviewMouseDown;
        }

        private void DeveloperOverlayWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Opacity = 0;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150));
            this.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        private void DeveloperOverlayWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _viewModel?.HandleActivity();
        }

        private void DeveloperOverlayWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _viewModel?.CloseOverlayCommand.Execute(null);
            }
            _viewModel?.HandleActivity();
        }
    }
}