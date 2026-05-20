using GuardianEye.Data;
using GuardianEye.Models;

namespace GuardianEye.Services
{
    public interface IDeviceService
    {
        Task<Device?> GetDeviceByHardwareHashAsync(string hardwareHash);
        Task<Device?> GetDeviceByNameAsync(string deviceName);
        Task<bool> RegisterDeviceAsync(Device device);
        Task<bool> AuthorizeDeviceAsync(int deviceId);
        Task<bool> UnauthorizeDeviceAsync(int deviceId);
        Task<List<Device>> GetAllDevicesAsync();
        Task UpdateDeviceLastSeenAsync(int deviceId);
    }

    public class DeviceService : IDeviceService
    {
        private readonly IDatabaseService _db;

        public DeviceService(IDatabaseService db)
        {
            _db = db;
        }

        public async Task<Device?> GetDeviceByHardwareHashAsync(string hardwareHash)
        {
            return await _db.QueryFirstOrDefaultAsync<Device>(
                "SELECT * FROM Devices WHERE HardwareHash = @HardwareHash", new { HardwareHash = hardwareHash });
        }

        public async Task<Device?> GetDeviceByNameAsync(string deviceName)
        {
            return await _db.QueryFirstOrDefaultAsync<Device>(
                "SELECT * FROM Devices WHERE DeviceName = @DeviceName", new { DeviceName = deviceName });
        }

        public async Task<bool> RegisterDeviceAsync(Device device)
        {
            try
            {
                device.CreatedAt = DateTime.UtcNow;
                device.LastSeen = DateTime.UtcNow;

                var result = await _db.ExecuteAsync(
                    @"INSERT INTO Devices (DeviceName, CpuId, MotherboardId, DiskId, HardwareHash, IsAuthorized)
                      VALUES (@DeviceName, @CpuId, @MotherboardId, @DiskId, @HardwareHash, @IsAuthorized)", device);

                return result > 0;
            }
            catch (Exception ex)
            {
                Helpers.Logging.Error($"Error registering device {device.DeviceName}", ex);
                return false;
            }
        }

        public async Task<bool> AuthorizeDeviceAsync(int deviceId)
        {
            try
            {
                var result = await _db.ExecuteAsync(
                    "UPDATE Devices SET IsAuthorized = 1 WHERE Id = @Id", new { Id = deviceId });
                return result > 0;
            }
            catch (Exception ex)
            {
                Helpers.Logging.Error($"Error authorizing device ID {deviceId}", ex);
                return false;
            }
        }

        public async Task<bool> UnauthorizeDeviceAsync(int deviceId)
        {
            try
            {
                var result = await _db.ExecuteAsync(
                    "UPDATE Devices SET IsAuthorized = 0 WHERE Id = @Id", new { Id = deviceId });
                return result > 0;
            }
            catch (Exception ex)
            {
                Helpers.Logging.Error($"Error unauthorizing device ID {deviceId}", ex);
                return false;
            }
        }

        public async Task<List<Device>> GetAllDevicesAsync()
        {
            var devices = await _db.QueryAsync<Device>("SELECT * FROM Devices ORDER BY CreatedAt DESC");
            return devices.ToList();
        }

        public async Task UpdateDeviceLastSeenAsync(int deviceId)
        {
            try
            {
                await _db.ExecuteAsync(
                    "UPDATE Devices SET LastSeen = @LastSeen WHERE Id = @Id",
                    new { LastSeen = DateTime.UtcNow, Id = deviceId });
            }
            catch (Exception ex)
            {
                Helpers.Logging.Error($"Error updating last seen for device ID {deviceId}", ex);
            }
        }
    }
}