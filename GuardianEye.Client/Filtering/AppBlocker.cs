using GuardianEye.Data;
using GuardianEye.Models;

namespace GuardianEye.Filtering
{
    public interface IAppBlocker
    {
        Task<bool> IsAppBlockedAsync(string processName);
        Task<bool> KillBlockedProcessAsync(string processName);
        Task<List<AppBlock>> GetAllBlockedAppsAsync();
        Task<bool> AddBlockedAppAsync(AppBlock appBlock);
    }

    public class AppBlocker : IAppBlocker
    {
        private readonly IDatabaseService _db;

        public AppBlocker(IDatabaseService db)
        {
            _db = db;
        }

        public async Task<bool> IsAppBlockedAsync(string processName)
        {
            try
            {
                var result = await _db.QueryFirstOrDefaultAsync<int>(
                    "SELECT COUNT(*) FROM AppBlocks WHERE IsEnabled = 1 AND ProcessName = @ProcessName",
                    new { ProcessName = processName.ToLower() });
                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> KillBlockedProcessAsync(string processName)
        {
            try
            {
                var processes = System.Diagnostics.Process.GetProcessesByName(processName);
                foreach (var process in processes)
                {
                    process.Kill();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<AppBlock>> GetAllBlockedAppsAsync()
        {
            var apps = await _db.QueryAsync<AppBlock>(
                "SELECT * FROM AppBlocks ORDER BY Category");
            return apps.ToList();
        }

        public async Task<bool> AddBlockedAppAsync(AppBlock appBlock)
        {
            try
            {
                var result = await _db.ExecuteAsync(
                    @"INSERT INTO AppBlocks (ProcessName, DisplayName, Category, Description) 
                      VALUES (@ProcessName, @DisplayName, @Category, @Description)", appBlock);
                return result > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}