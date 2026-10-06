using System.ComponentModel.DataAnnotations;

namespace Portfolio_API.Models
{
    public class ExperienceEntry
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public required string Company { get; set; }

        [Required, MaxLength(200)]
        public required string Role { get; set; }

        public DateOnly StartDate { get; set; }

        // null = current position
        public DateOnly? EndDate { get; set; }

        [Required]
        public required string Description { get; set; }

        public int DisplayOrder { get; set; }
    }
}
