using System.Collections.Concurrent;
using System.Data.SQLite;
using System.Reflection;
using Dapper;
using GuardianEye.Helpers;
using GuardianEye.Models;

namespace GuardianEye.Data
{
    public interface IDatabaseService
    {
        Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null);
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null);
        Task<int> ExecuteAsync(string sql, object? param = null);
        Task<int> ExecuteScalarAsync(string sql, object? param = null);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        SQLiteConnection GetConnection();
    }

    public class DatabaseService : IDatabaseService, IDisposable
    {
        private readonly string _connectionString;
        private SQLiteTransaction? _transaction;
        private SQLiteConnection? _currentConnection;
        private bool _disposed = false;

        public DatabaseService()
        {
            AppPaths.EnsureDirectories();
            _connectionString = $"Data Source={AppPaths.Database};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            
            var tables = new[]
            {
                @"CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FullName TEXT NOT NULL,
                    Username TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    Role TEXT NOT NULL DEFAULT 'Student',
                    StudentId TEXT,
                    Class TEXT,
                    DeviceId TEXT,
                    SessionsUsedToday INTEGER DEFAULT 0,
                    MaxDailySessions INTEGER DEFAULT 2,
                    SessionDurationMinutes INTEGER DEFAULT 15,
                    LastLoginDate DATETIME,
                    SessionEndTime DATETIME,
                    IsLoggedIn INTEGER DEFAULT 0,
                    IsLocked INTEGER DEFAULT 0,
                    IsActive INTEGER DEFAULT 1,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME,
                    LastActivityTime DATETIME
                )",
                @"CREATE TABLE IF NOT EXISTS Sessions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL,
                    StartTime DATETIME DEFAULT CURRENT_TIMESTAMP,
                    EndTime DATETIME,
                    Duration INTEGER,
                    Status TEXT,
                    DeviceName TEXT,
                    SessionNumber INTEGER,
                    Notes TEXT,
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                )",
                @"CREATE TABLE IF NOT EXISTS ActivityLogs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER,
                    ActivityType TEXT NOT NULL,
                    Description TEXT,
                    ApplicationName TEXT,
                    WindowTitle TEXT,
                    Url TEXT,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                    Duration INTEGER,
                    DeviceName TEXT,
                    Severity TEXT,
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                )",
                @"CREATE TABLE IF NOT EXISTS Devices (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    DeviceName TEXT NOT NULL,
                    CpuId TEXT,
                    MotherboardId TEXT,
                    DiskId TEXT,
                    HardwareHash TEXT,
                    IsAuthorized INTEGER DEFAULT 0,
                    AssignedUserId INTEGER,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    LastSeen DATETIME,
                    Status TEXT DEFAULT 'Active',
                    FOREIGN KEY (AssignedUserId) REFERENCES Users(Id)
                )",
                @"CREATE TABLE IF NOT EXISTS WebsiteRules (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UrlPattern TEXT NOT NULL,
                    Category TEXT,
                    RuleType TEXT NOT NULL,
                    IsEnabled INTEGER DEFAULT 1,
                    Description TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    CreatedByUserId INTEGER,
                    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
                )",
                @"CREATE TABLE IF NOT EXISTS AppBlocks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProcessName TEXT NOT NULL,
                    DisplayName TEXT,
                    Category TEXT,
                    IsEnabled INTEGER DEFAULT 1,
                    AutoKill INTEGER DEFAULT 1,
                    Description TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                )",
                @"CREATE TABLE IF NOT EXISTS Screenshots (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER,
                    FilePath TEXT NOT NULL,
                    CapturedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    Reason TEXT,
                    FileSizeBytes INTEGER,
                    Width INTEGER,
                    Height INTEGER,
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                )",
                @"CREATE TABLE IF NOT EXISTS Notifications (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER,
                    Title TEXT NOT NULL,
                    Message TEXT,
                    Type TEXT,
                    IsRead INTEGER DEFAULT 0,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    ExpiresAt DATETIME,
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                )",
                @"CREATE TABLE IF NOT EXISTS Reports (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    ReportType TEXT,
                    GeneratedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    PeriodStart DATETIME,
                    PeriodEnd DATETIME,
                    Data TEXT,
                    TotalStudents INTEGER,
                    TotalSessions INTEGER,
                    Violations INTEGER,
                    AverageUsageMinutes REAL,
                    FilePath TEXT,
                    GeneratedByUserId INTEGER,
                    FOREIGN KEY (GeneratedByUserId) REFERENCES Users(Id)
                )",
                @"CREATE TABLE IF NOT EXISTS Settings (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Key TEXT NOT NULL UNIQUE,
                    Value TEXT,
                    Description TEXT,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                )"
            };

            foreach (var table in tables)
                connection.Execute(table);

            SeedAdminUser(connection);
            SeedDefaultAppBlocks(connection);
            SeedDefaultWebsiteRules(connection);
        }

        private void SeedAdminUser(SQLiteConnection connection)
        {
            var existing = connection.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM Users WHERE Role = 'Admin'");
            if (existing > 0) return;

            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
            connection.Execute(
                @"INSERT INTO Users (FullName, Username, PasswordHash, Role, IsActive, SessionsUsedToday, MaxDailySessions, SessionDurationMinutes)
                  VALUES (@FullName, @Username, @PasswordHash, @Role, 1, 0, 999, 1440)",
                new { FullName = "Administrator", Username = "admin", PasswordHash = adminPasswordHash, Role = "Admin" });
        }

        private void SeedDefaultAppBlocks(SQLiteConnection connection)
        {
            var existing = connection.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM AppBlocks");
            if (existing > 0) return;

            var blocks = new[]
            {
                new { ProcessName = "taskmgr", DisplayName = "Task Manager", Category = "System", Description = "Block Task Manager" },
                new { ProcessName = "regedit", DisplayName = "Registry Editor", Category = "System", Description = "Block Registry Editor" },
                new { ProcessName = "cmd", DisplayName = "Command Prompt", Category = "System", Description = "Block Command Prompt" },
                new { ProcessName = "powershell", DisplayName = "PowerShell", Category = "System", Description = "Block PowerShell" }
            };
            foreach (var block in blocks)
                connection.Execute(
                    @"INSERT INTO AppBlocks (ProcessName, DisplayName, Category, Description) VALUES (@ProcessName, @DisplayName, @Category, @Description)",
                    block);
        }

        private void SeedDefaultWebsiteRules(SQLiteConnection connection)
        {
            var existing = connection.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM WebsiteRules");
            if (existing > 0) return;

            var rules = new[]
            {
                new { UrlPattern = "*facebook.com*", Category = "Social Media", RuleType = "Blacklist" },
                new { UrlPattern = "*instagram.com*", Category = "Social Media", RuleType = "Blacklist" },
                new { UrlPattern = "*twitter.com*", Category = "Social Media", RuleType = "Blacklist" },
                new { UrlPattern = "*tiktok.com*", Category = "Social Media", RuleType = "Blacklist" },
                new { UrlPattern = "*youtube.com*", Category = "Streaming", RuleType = "Blacklist" },
                new { UrlPattern = "*twitch.tv*", Category = "Streaming", RuleType = "Blacklist" }
            };
            foreach (var rule in rules)
                connection.Execute(
                    @"INSERT INTO WebsiteRules (UrlPattern, Category, RuleType) VALUES (@UrlPattern, @Category, @RuleType)",
                    rule);
        }

        public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<T>(sql, param);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.QueryAsync<T>(sql, param);
        }

        public async Task<int> ExecuteAsync(string sql, object? param = null)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.ExecuteAsync(sql, param);
        }

        public async Task<int> ExecuteScalarAsync(string sql, object? param = null)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.ExecuteScalarAsync<int>(sql, param);
        }

        public async Task BeginTransactionAsync()
        {
            if (_currentConnection == null)
            {
                _currentConnection = new SQLiteConnection(_connectionString);
                await _currentConnection.OpenAsync();
            }
            _transaction = _currentConnection.BeginTransaction();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction.Dispose();
                _transaction = null;
            }
            if (_currentConnection != null)
            {
                await _currentConnection.CloseAsync();
                _currentConnection.Dispose();
                _currentConnection = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                _transaction.Dispose();
                _transaction = null;
            }
            if (_currentConnection != null)
            {
                await _currentConnection.CloseAsync();
                _currentConnection.Dispose();
                _currentConnection = null;
            }
        }

        public SQLiteConnection GetConnection() => new SQLiteConnection(_connectionString);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                _currentConnection?.Dispose();
                _disposed = true;
            }
        }
    }
}