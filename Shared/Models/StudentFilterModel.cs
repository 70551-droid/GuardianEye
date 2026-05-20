using System.ComponentModel.DataAnnotations;

namespace GuardianEye.Shared.Models
{
    public class StudentFilterModel
    {
        public string? SearchTerm { get; set; }
        public string? ClassFilter { get; set; }
        public bool? IsActiveFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}