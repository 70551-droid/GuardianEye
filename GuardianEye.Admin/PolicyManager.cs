using System.Text.Json;

namespace GuardianEye.Admin;

public class PolicyExport
{
    public int Version { get; set; } = 1;
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
    public List<GroupExport> Groups { get; set; } = new();
    public List<StudentExport> Students { get; set; } = new();
    public FilterSettingsExport FilterSettings { get; set; } = new();
}

public class GroupExport
{
    public string Name { get; set; }
    public int DailyLimitSeconds { get; set; }
}

public class StudentExport
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string DisplayName { get; set; }
    public int DailyLimitSeconds { get; set; }
    public string GroupName { get; set; }
}

public class FilterSettingsExport
{
    public List<string> BlockedDomains { get; set; } = new();
    public List<string> BlockedProcesses { get; set; } = new();
}

public static class PolicyManager
{
    public static void ExportPolicy(string filePath, DatabaseService db)
    {
        var policy = new PolicyExport();

        foreach (var g in db.GetAllGroups())
            policy.Groups.Add(new GroupExport { Name = g.Name, DailyLimitSeconds = g.DailyLimit });

        foreach (var s in db.GetAllStudents())
        {
            string hash = GetPasswordHash(s.Username, db);
            policy.Students.Add(new StudentExport
            {
                Username = s.Username,
                PasswordHash = hash,
                DisplayName = s.DisplayName,
                DailyLimitSeconds = s.DailyLimit,
                GroupName = s.GroupName
            });
        }

        policy.FilterSettings.BlockedDomains = GetFilterList(db, "domains");
        policy.FilterSettings.BlockedProcesses = GetFilterList(db, "processes");

        string json = JsonSerializer.Serialize(policy, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    public static void ImportPolicy(string filePath, DatabaseService db)
    {
        string json = File.ReadAllText(filePath);
        var policy = JsonSerializer.Deserialize<PolicyExport>(json);
        if (policy == null) return;

        foreach (var g in policy.Groups)
        {
            var existing = db.GetAllGroups().FirstOrDefault(x => x.Name == g.Name);
            if (existing.Id > 0)
                db.UpdateGroup(existing.Id, g.Name, g.DailyLimitSeconds);
            else
                db.CreateGroup(g.Name, g.DailyLimitSeconds);
        }

        foreach (var s in policy.Students)
        {
            int? groupId = null;
            var match = db.GetAllGroups().FirstOrDefault(x => x.Name == s.GroupName);
            if (match.Id > 0) groupId = match.Id;

            var existing = db.GetAllStudents().FirstOrDefault(x => x.Username == s.Username);
            if (existing.Id > 0)
                db.UpdateStudent(existing.Id, s.DisplayName, s.DailyLimitSeconds, groupId);
        }

        SaveFilterList(db, "domains", policy.FilterSettings.BlockedDomains);
        SaveFilterList(db, "processes", policy.FilterSettings.BlockedProcesses);
    }

    private static string GetPasswordHash(string username, DatabaseService db)
    {
        using var conn = new Microsoft.Data.Sqlite.SqliteConnection(db.ConnectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT password_hash FROM students WHERE username = @u";
        cmd.Parameters.AddWithValue("@u", username);
        return cmd.ExecuteScalar() as string ?? "";
    }

    private static List<string> GetFilterList(DatabaseService db, string type)
    {
        using var conn = new Microsoft.Data.Sqlite.SqliteConnection(db.ConnectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT value FROM filter_settings WHERE type = @t";
        cmd.Parameters.AddWithValue("@t", type);
        var result = new List<string>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            result.Add(reader.GetString(0));
        return result;
    }

    private static void SaveFilterList(DatabaseService db, string type, List<string> values)
    {
        using var conn = new Microsoft.Data.Sqlite.SqliteConnection(db.ConnectionString);
        conn.Open();
        using var del = conn.CreateCommand();
        del.CommandText = "DELETE FROM filter_settings WHERE type = @t";
        del.Parameters.AddWithValue("@t", type);
        del.ExecuteNonQuery();

        foreach (var v in values)
        {
            using var ins = conn.CreateCommand();
            ins.CommandText = "INSERT INTO filter_settings (type, value) VALUES (@t, @v)";
            ins.Parameters.AddWithValue("@t", type);
            ins.Parameters.AddWithValue("@v", v);
            ins.ExecuteNonQuery();
        }
    }
}
