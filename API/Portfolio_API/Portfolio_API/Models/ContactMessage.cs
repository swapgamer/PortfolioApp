using System.ComponentModel.DataAnnotations;

namespace Portfolio_API.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public required string Name { get; set; }

        [Required, MaxLength(320), EmailAddress]
        public required string Email { get; set; }

        [Required, MaxLength(2000)]
        public required string Message { get; set; }

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        public bool Handled { get; set; }
    }
}
