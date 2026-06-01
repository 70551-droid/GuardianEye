using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

namespace GuardianEye.Admin;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService()
    {
        string dbDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GuardianEye");
        Directory.CreateDirectory(dbDir);
        string dbPath = Path.Combine(dbDir, "GuardianEye.db");
        _connectionString = $"Data Source={dbPath}";
        Initialize();
    }

    public string ConnectionString => _connectionString;

    private void Initialize()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS admin_users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                username TEXT NOT NULL UNIQUE,
                password_hash TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS groups (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL UNIQUE,
                daily_limit_seconds INTEGER NOT NULL DEFAULT 3600
            );
            CREATE TABLE IF NOT EXISTS students (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                username TEXT NOT NULL UNIQUE,
                password_hash TEXT NOT NULL,
                display_name TEXT NOT NULL,
                daily_limit_seconds INTEGER NOT NULL DEFAULT 3600,
                group_id INTEGER REFERENCES groups(id)
            );
            CREATE TABLE IF NOT EXISTS sessions (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                student_username TEXT NOT NULL,
                session_id TEXT NOT NULL,
                start_time TEXT NOT NULL,
                duration_seconds INTEGER NOT NULL,
                status TEXT NOT NULL DEFAULT 'active'
            );
            CREATE TABLE IF NOT EXISTS blocked_attempts (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                student_username TEXT NOT NULL,
                attempt_type TEXT NOT NULL,
                target_name TEXT NOT NULL,
                timestamp TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS filter_settings (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                type TEXT NOT NULL,
                value TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS daily_remaining (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                student_username TEXT NOT NULL,
                date TEXT NOT NULL,
                remaining_seconds INTEGER NOT NULL,
                UNIQUE(student_username, date)
            );";
        cmd.ExecuteNonQuery();

        SeedDefaultAdmin(conn);
        SeedDefaultGroups(conn);
        SeedDefaultStudents(conn);
    }

    private static void SeedDefaultAdmin(SqliteConnection conn)
    {
        using var check = conn.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM admin_users";
        long count = (long)check.ExecuteScalar();
        if (count > 0) return;

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO admin_users (username, password_hash) VALUES (@u, @p)";
        cmd.Parameters.AddWithValue("@u", "admin");
        cmd.Parameters.AddWithValue("@p", HashPassword("admin123"));
        cmd.ExecuteNonQuery();
    }

    private static void SeedDefaultGroups(SqliteConnection conn)
    {
        using var check = conn.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM groups";
        long count = (long)check.ExecuteScalar();
        if (count > 0) return;

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO groups (name, daily_limit_seconds) VALUES (@n, @l)";
        cmd.Parameters.AddWithValue("@n", "Default");
        cmd.Parameters.AddWithValue("@l", 3600);
        cmd.ExecuteNonQuery();
    }

    private static void SeedDefaultStudents(SqliteConnection conn)
    {
        using var check = conn.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM students";
        long count = (long)check.ExecuteScalar();
        if (count > 0) return;

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO students (username, password_hash, display_name, daily_limit_seconds, group_id) VALUES (@u, @p, @n, @l, @g)";
        cmd.Parameters.AddWithValue("@u", "student");
        cmd.Parameters.AddWithValue("@p", HashPassword("1234"));
        cmd.Parameters.AddWithValue("@n", "Default Student");
        cmd.Parameters.AddWithValue("@l", 3600);
        cmd.Parameters.AddWithValue("@g", 1);
        cmd.ExecuteNonQuery();
    }

    public bool ValidateAdmin(string username, string password)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM admin_users WHERE username = @u AND password_hash = @p";
        cmd.Parameters.AddWithValue("@u", username);
        cmd.Parameters.AddWithValue("@p", HashPassword(password));
        return (long)cmd.ExecuteScalar() > 0;
    }

    public (bool success, string displayName, int dailyLimit, int groupId, string groupName) ValidateStudent(string username, string password)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT s.display_name, s.daily_limit_seconds, 
                   COALESCE(s.group_id, 0), COALESCE(g.name, '')
            FROM students s
            LEFT JOIN groups g ON g.id = s.group_id
            WHERE s.username = @u AND s.password_hash = @p";
        cmd.Parameters.AddWithValue("@u", username);
        cmd.Parameters.AddWithValue("@p", HashPassword(password));
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return (true, reader.GetString(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetString(3));
        return (false, "", 0, 0, "");
    }

    public int GetGroupDailyLimit(int groupId)
    {
        if (groupId <= 0) return 0;
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT daily_limit_seconds FROM groups WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", groupId);
        var result = cmd.ExecuteScalar();
        return result != null ? Convert.ToInt32(result) : 0;
    }

    public void LogSession(string studentUsername, Guid sessionId, int durationSeconds)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO sessions (student_username, session_id, start_time, duration_seconds, status) VALUES (@u, @s, @t, @d, 'active')";
        cmd.Parameters.AddWithValue("@u", studentUsername);
        cmd.Parameters.AddWithValue("@s", sessionId.ToString());
        cmd.Parameters.AddWithValue("@t", DateTime.UtcNow.ToString("o"));
        cmd.Parameters.AddWithValue("@d", durationSeconds);
        cmd.ExecuteNonQuery();
    }

    public void EndSession(Guid sessionId)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE sessions SET status = 'ended' WHERE session_id = @s";
        cmd.Parameters.AddWithValue("@s", sessionId.ToString());
        cmd.ExecuteNonQuery();
    }

    public int GetDailyUsedSeconds(string studentUsername)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COALESCE(SUM(duration_seconds), 0) FROM sessions WHERE student_username = @u AND status = 'active' AND date(start_time) = date('now')";
        cmd.Parameters.AddWithValue("@u", studentUsername);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void SaveRemainingTime(string username, int seconds)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO daily_remaining (student_username, date, remaining_seconds)
            VALUES (@u, date('now'), @s)
            ON CONFLICT(student_username, date) DO UPDATE SET remaining_seconds = @s";
        cmd.Parameters.AddWithValue("@u", username);
        cmd.Parameters.AddWithValue("@s", seconds);
        cmd.ExecuteNonQuery();
    }

    public int GetSavedRemainingTime(string username)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT remaining_seconds FROM daily_remaining WHERE student_username = @u AND date = date('now')";
        cmd.Parameters.AddWithValue("@u", username);
        var result = cmd.ExecuteScalar();
        if (result == null) return -1;
        int saved = Convert.ToInt32(result);
        using var del = conn.CreateCommand();
        del.CommandText = "DELETE FROM daily_remaining WHERE student_username = @u AND date = date('now')";
        del.Parameters.AddWithValue("@u", username);
        del.ExecuteNonQuery();
        return saved;
    }

    public void LogBlockedAttempt(string studentUsername, string attemptType, string targetName)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO blocked_attempts (student_username, attempt_type, target_name, timestamp) VALUES (@u, @t, @n, @ts)";
        cmd.Parameters.AddWithValue("@u", studentUsername);
        cmd.Parameters.AddWithValue("@t", attemptType);
        cmd.Parameters.AddWithValue("@n", targetName);
        cmd.Parameters.AddWithValue("@ts", DateTime.UtcNow.ToString("o"));
        cmd.ExecuteNonQuery();
    }

    // ===== Group CRUD =====

    public List<(int Id, string Name, int DailyLimit)> GetAllGroups()
    {
        var result = new List<(int, string, int)>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, name, daily_limit_seconds FROM groups ORDER BY name";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            result.Add((reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2)));
        return result;
    }

    public int CreateGroup(string name, int dailyLimit)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO groups (name, daily_limit_seconds) VALUES (@n, @l); SELECT last_insert_rowid()";
        cmd.Parameters.AddWithValue("@n", name);
        cmd.Parameters.AddWithValue("@l", dailyLimit);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void UpdateGroup(int id, string name, int dailyLimit)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE groups SET name = @n, daily_limit_seconds = @l WHERE id = @id";
        cmd.Parameters.AddWithValue("@n", name);
        cmd.Parameters.AddWithValue("@l", dailyLimit);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public void DeleteGroup(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE students SET group_id = NULL WHERE group_id = @id; DELETE FROM groups WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    // ===== Student Management =====

    public List<(int Id, string Username, string DisplayName, int DailyLimit, int? GroupId, string GroupName)> GetAllStudents()
    {
        var result = new List<(int, string, string, int, int?, string)>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT s.id, s.username, s.display_name, s.daily_limit_seconds, s.group_id, COALESCE(g.name, '')
            FROM students s LEFT JOIN groups g ON g.id = s.group_id ORDER BY s.username";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            int? gid = reader.IsDBNull(4) ? null : reader.GetInt32(4);
            result.Add((reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetInt32(3), gid, reader.GetString(5)));
        }
        return result;
    }

    public void UpdateStudent(int id, string displayName, int dailyLimit, int? groupId)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE students SET display_name = @n, daily_limit_seconds = @l, group_id = @g WHERE id = @id";
        cmd.Parameters.AddWithValue("@n", displayName);
        cmd.Parameters.AddWithValue("@l", dailyLimit);
        cmd.Parameters.AddWithValue("@g", (object)groupId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    // ===== Report Queries =====

    public List<(string Username, string DisplayName, string SessionId, DateTime StartTime, int DurationSeconds, string Status)> GetAllSessions()
    {
        var result = new List<(string, string, string, DateTime, int, string)>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT s.student_username, COALESCE(st.display_name, s.student_username),
                   s.session_id, s.start_time, s.duration_seconds, s.status
            FROM sessions s LEFT JOIN students st ON st.username = s.student_username
            ORDER BY s.start_time DESC";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            result.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2),
                DateTime.Parse(reader.GetString(3)), reader.GetInt32(4), reader.GetString(5)));
        return result;
    }

    public List<(string Username, string DisplayName, string AttemptType, string TargetName, DateTime Timestamp)> GetBlockedAttempts()
    {
        var result = new List<(string, string, string, string, DateTime)>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT b.student_username, COALESCE(st.display_name, b.student_username),
                   b.attempt_type, b.target_name, b.timestamp
            FROM blocked_attempts b LEFT JOIN students st ON st.username = b.student_username
            ORDER BY b.timestamp DESC LIMIT 500";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            result.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2),
                reader.GetString(3), DateTime.Parse(reader.GetString(4))));
        return result;
    }

    public (int TotalStudents, int TotalMinutes, int ActiveSessions) GetTodaySummary()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT COUNT(DISTINCT student_username),
                   COALESCE(SUM(duration_seconds) / 60, 0),
                   COUNT(CASE WHEN status = 'active' THEN 1 END)
            FROM sessions WHERE date(start_time) = date('now')";
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return (Convert.ToInt32(reader[0]), Convert.ToInt32(reader[1]), Convert.ToInt32(reader[2]));
        return (0, 0, 0);
    }

    public List<(string Username, string DisplayName, int TotalMinutes, int SessionCount)> GetMostActiveStudents(int topN = 10)
    {
        var result = new List<(string, string, int, int)>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT s.student_username, COALESCE(st.display_name, s.student_username),
                   SUM(s.duration_seconds) / 60, COUNT(*)
            FROM sessions s LEFT JOIN students st ON st.username = s.student_username
            GROUP BY s.student_username ORDER BY SUM(s.duration_seconds) DESC LIMIT @n";
        cmd.Parameters.AddWithValue("@n", topN);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            result.Add((reader.GetString(0), reader.GetString(1), Convert.ToInt32(reader[2]), Convert.ToInt32(reader[3])));
        return result;
    }

    public void ExportSessionsToCsv(string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Student,Display Name,Session ID,Start Time,Duration (min),Status");
        foreach (var s in GetAllSessions())
            sb.AppendLine($"{EscapeCsv(s.Username)},{EscapeCsv(s.DisplayName)},{EscapeCsv(s.SessionId)},{s.StartTime:yyyy-MM-dd HH:mm:ss},{s.DurationSeconds / 60},{s.Status}");
        File.WriteAllText(filePath, sb.ToString());
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    private static string HashPassword(string password)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}
