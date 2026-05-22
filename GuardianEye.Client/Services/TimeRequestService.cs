using GuardianEye.Data;
using GuardianEye.Helpers;
using GuardianEye.Models;

namespace GuardianEye.Services
{
    public interface ITimeRequestService
    {
        Task<bool> SubmitRequestAsync(int studentId, int minutes, string reason);
    }

    public class TimeRequestService : ITimeRequestService
    {
        private readonly IDatabaseService _db;

        public TimeRequestService(IDatabaseService db)
        {
            _db = db;
        }

        public async Task<bool> SubmitRequestAsync(int studentId, int minutes, string reason)
        {
            try
            {
                var request = new TimeRequest
                {
                    StudentId = studentId,
                    RequestedMinutes = minutes,
                    Reason = reason,
                    Timestamp = DateTime.UtcNow,
                    Status = "Pending"
                };

                await _db.ExecuteAsync(
                    @"INSERT INTO TimeRequests (StudentId, RequestedMinutes, Reason, Timestamp, Status)
                      VALUES (@StudentId, @RequestedMinutes, @Reason, @Timestamp, @Status)",
                    request);

                Logging.Info($"Time request submitted: student {studentId}, {minutes} min");
                return true;
            }
            catch (Exception ex)
            {
                Logging.Error("Error submitting time request", ex);
                return false;
            }
        }
    }
}