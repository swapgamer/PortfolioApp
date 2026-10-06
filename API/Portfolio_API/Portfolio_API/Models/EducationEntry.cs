using System.ComponentModel.DataAnnotations;

namespace Portfolio_API.Models
{
    public class EducationEntry
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public required string Institution { get; set; }

        [Required, MaxLength(200)]
        public required string Degree { get; set; }

        public DateOnly StartDate { get; set; }

        // null = ongoing
        public DateOnly? EndDate { get; set; }

        public int DisplayOrder { get; set; }
    }
}
