using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using GuardianEye.ViewModels;

namespace GuardianEye.Views
{
    public partial class DeveloperOverlayWindow : Window
    {
        private readonly DeveloperOverlayViewModel? _viewModel;

        public DeveloperOverlayWindow()
        {
            InitializeComponent();
            _viewModel = DataContext as DeveloperOverlayViewModel;
            Loaded += DeveloperOverlayWindow_Loaded;
            KeyDown += DeveloperOverlayWindow_KeyDown;
            MouseMove += (s, e) => _viewModel?.HandleActivity();
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
                _viewModel?.CloseOverlayCommand.Execute(null);
            }
            _viewModel?.HandleActivity();
        }
    }
}