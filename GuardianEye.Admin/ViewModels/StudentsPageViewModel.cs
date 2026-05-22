using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuardianEye.Shared.Dtos;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;

namespace GuardianEye.Admin.ViewModels
{
    public partial class StudentsPageViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;

        [ObservableProperty]
        private ObservableCollection<StudentDto> students = new();

        [ObservableProperty]
        private string searchQuery = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        public StudentsPageViewModel()
        {
            _httpClient = new HttpClient();
        }

        [RelayCommand]
        public async Task LoadStudentsAsync()
        {
            IsLoading = true;
            try
            {
                var response = await _httpClient.GetAsync("api/students");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<StudentDto>>();
                    Students.Clear();
                    if (result != null)
                    {
                        foreach (var student in result)
                        {
                            Students.Add(student);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task AddStudentAsync()
        {
            // TODO: Open add student dialog
        }

        [RelayCommand]
        public async Task EditStudentAsync(StudentDto student)
        {
            // TODO: Open edit student dialog
        }

        [RelayCommand]
        public async Task ResetSessionsAsync(StudentDto student)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/students/{student.Id}/reset-sessions", null);
                if (response.IsSuccessStatusCode)
                {
                    await LoadStudentsAsync();
                    MessageBox.Show("Sessions reset successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error resetting sessions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task ForceLogoutAsync(StudentDto student)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/students/{student.Id}/force-logout", null);
                if (response.IsSuccessStatusCode)
                {
                    await LoadStudentsAsync();
                    MessageBox.Show($"{student.FullName} has been logged out", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error logging out student: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}