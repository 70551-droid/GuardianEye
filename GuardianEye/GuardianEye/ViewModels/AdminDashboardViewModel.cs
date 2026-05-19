using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuardianEye.Models;
using GuardianEye.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace GuardianEye.ViewModels
{
    public partial class AdminDashboardViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly ISessionService _sessionService;
        private readonly IAuthService _authService;

        [ObservableProperty]
        private ObservableCollection<User> _students = new();

        [ObservableProperty]
        private User? _selectedStudent;

        [ObservableProperty]
        private string _searchText = "";

        [ObservableProperty]
        private int _totalStudents = 0;

        [ObservableProperty]
        private int _activeSessions = 0;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome, Admin";

        public AdminDashboardViewModel(IUserService userService, ISessionService sessionService, IAuthService authService)
        {
            _userService = userService;
            _sessionService = sessionService;
            _authService = authService;
            LoadData();
        }

        [RelayCommand]
        private async Task LoadData()
        {
            try
            {
                var students = await _userService.GetAllStudentsAsync();
                Students.Clear();
                foreach (var student in students)
                    Students.Add(student);

                TotalStudents = Students.Count;
                ActiveSessions = Students.Count(s => s.IsLoggedIn);
            }
            catch (Exception ex)
            {
                Helpers.Logging.Error("Error loading admin dashboard data", ex);
            }
        }

        [RelayCommand]
        private async Task AddStudentAsync()
        {
            // TODO: Open add student dialog
        }

        [RelayCommand]
        private async Task EditStudentAsync()
        {
            if (SelectedStudent == null) return;
            // TODO: Open edit student dialog
        }

        [RelayCommand]
        private async Task DeleteStudentAsync()
        {
            if (SelectedStudent == null) return;

            var result = MessageBox.Show($"Delete student {SelectedStudent.FullName}?", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                await _userService.DeleteUserAsync(SelectedStudent.Id);
                await LoadData();
            }
        }

        [RelayCommand]
        private async Task ResetSessionsAsync()
        {
            if (SelectedStudent == null) return;

            await _userService.ResetUserSessionsAsync(SelectedStudent.Id);
            await LoadData();
        }

        [RelayCommand]
        private async Task LockStudentAsync()
        {
            if (SelectedStudent == null) return;

            await _userService.LockUserAsync(SelectedStudent.Id);
            await LoadData();
        }
    }
}