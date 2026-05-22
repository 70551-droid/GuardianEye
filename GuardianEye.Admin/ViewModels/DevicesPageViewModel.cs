using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace GuardianEye.Admin.ViewModels
{
    public partial class DevicesPageViewModel : ObservableObject
    {
        private readonly DispatcherTimer _refreshTimer;

        [ObservableProperty]
        private ObservableCollection<DeviceStatus> devices = new();

        [ObservableProperty]
        private string lastRefresh = "Last refresh: --";

        [ObservableProperty]
        private bool isLoading = false;

        public DevicesPageViewModel()
        {
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _refreshTimer.Tick += async (s, e) => await LoadDevicesAsync();
        }

        public async Task LoadDevicesAsync()
        {
            IsLoading = true;
            try
            {
                // TODO: Call API to get active sessions
                // For now, populate with placeholder data
                Devices.Clear();
                
                // Sample data - will be replaced with real API call
                Devices.Add(new DeviceStatus
                {
                    DeviceName = "PC-001",
                    UserName = "student001",
                    StudentId = "STU001",
                    State = "In Session",
                    StateColor = "#FF0078D4",
                    SessionRemaining = "12:34",
                    LastHeartbeat = DateTime.Now.AddSeconds(-30).ToString("HH:mm:ss")
                });

                LastRefresh = $"Last refresh: {DateTime.Now:HH:mm:ss}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void StartAutoRefresh()
        {
            _refreshTimer.Start();
        }

        [RelayCommand]
        public void StopAutoRefresh()
        {
            _refreshTimer.Stop();
        }
    }

    public class DeviceStatus
    {
        public string DeviceName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string StateColor { get; set; } = "#FF6B6B";
        public string SessionRemaining { get; set; } = string.Empty;
        public string LastHeartbeat { get; set; } = string.Empty;
    }
}