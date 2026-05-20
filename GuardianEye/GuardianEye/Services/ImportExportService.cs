using CsvHelper;
using CsvHelper.Configuration;
using GuardianEye.Models;
using System.Globalization;
using System.IO;
using System.Text;

namespace GuardianEye.Services
{
    public interface IImportExportService
    {
        Task<ImportResult> ImportStudentsFromCsvAsync(string filePath);
        Task<string> ExportStudentsToCsvAsync(List<User> students);
        Task<byte[]> GenerateCsvTemplateAsync();
    }

    public class ImportResult
    {
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<User> ImportedUsers { get; set; } = new();
    }

    public class ImportExportService : IImportExportService
    {
        private readonly IUserService _userService;

        public ImportExportService(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<ImportResult> ImportStudentsFromCsvAsync(string filePath)
        {
            var result = new ImportResult();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            };

            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, config);
                var records = csv.GetRecords<CsvStudentRecord>().ToList();

                foreach (var record in records)
                {
                    var user = new User
                    {
                        FullName = record.FullName,
                        Username = record.Username,
                        StudentId = record.StudentID,
                        Class = record.Class,
                        DeviceId = record.DeviceID,
                        MaxDailySessions = record.DailyLimit > 0 ? record.DailyLimit : 2,
                        Role = "Student",
                        IsActive = true
                    };

                    if (await _userService.CreateUserAsync(user, record.Password))
                    {
                        result.SuccessCount++;
                        result.ImportedUsers.Add(user);
                    }
                    else
                    {
                        result.FailCount++;
                        result.Errors.Add($"Failed to import {record.Username}");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Import error: {ex.Message}");
            }

            return result;
        }

        public async Task<string> ExportStudentsToCsvAsync(List<User> students)
        {
            using var writer = new StringWriter();
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            await csv.WriteRecordsAsync(students.Select(s => new
            {
                s.FullName,
                s.Username,
                s.StudentId,
                s.Class,
                s.IsActive,
                s.SessionsUsedToday,
                s.MaxDailySessions
            }));
            return writer.ToString();
        }

        public async Task<byte[]> GenerateCsvTemplateAsync()
        {
            var template = "FullName,StudentID,Class,Username,Password,DeviceID,DailyLimit\n" +
                          "John Doe,STU001,Grade 10,john.doe,password,DEVICE01,2\n";
            return System.Text.Encoding.UTF8.GetBytes(template);
        }
    }

    public class CsvStudentRecord
    {
        public string FullName { get; set; } = string.Empty;
        public string StudentID { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DeviceID { get; set; } = string.Empty;
        public int DailyLimit { get; set; } = 2;
    }
}