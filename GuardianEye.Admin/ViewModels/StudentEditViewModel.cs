using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuardianEye.Shared.Dtos;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;

namespace GuardianEye.Admin.ViewModels
{
    public partial class StudentEditViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;

        [ObservableProperty]
        private string fullName = string.Empty;

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string studentId = string.Empty;

        [ObservableProperty]
        private string className = string.Empty;

        [ObservableProperty]
        private int maxSessions = 2;

        [ObservableProperty]
        private int sessionDuration = 15;

        [ObservableProperty]
        private bool isActive = true;

        [ObservableProperty]
        private bool isEditMode = false;

        [ObservableProperty]
        private int studentIdValue = 0;

        public StudentEditViewModel()
        {
            _httpClient = new HttpClient();
        }

        [RelayCommand]
        public async Task SaveAsync(Window window)
        {
            try
            {
                if (IsEditMode)
                {
                    var updateDto = new UpdateStudentDto
                    {
                        Id = StudentIdValue,
                        FullName = FullName,
                        StudentId = StudentId,
                        Class = ClassName,
                        MaxDailySessions = MaxSessions,
                        SessionDurationMinutes = SessionDuration,
                        IsActive = IsActive
                    };
                    var response = await _httpClient.PutAsJsonAsync($"api/students/{StudentIdValue}", updateDto);
                    if (response.IsSuccessStatusCode)
                    {
                        window.DialogResult = true;
                        window.Close();
                    }
                }
                else
                {
                    var createDto = new CreateStudentDto
                    {
                        FullName = FullName,
                        Username = Username,
                        Password = Password,
                        StudentId = StudentId,
                        Class = ClassName,
                        MaxDailySessions = MaxSessions,
                        SessionDurationMinutes = SessionDuration
                    };
                    var response = await _httpClient.PostAsJsonAsync("api/students", createDto);
                    if (response.IsSuccessStatusCode)
                    {
                        window.DialogResult = true;
                        window.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving student: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public void Cancel(Window window)
        {
            window.DialogResult = false;
            window.Close();
        }
    }
}