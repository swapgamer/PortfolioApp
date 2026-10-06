using System.ComponentModel.DataAnnotations;

namespace Portfolio_API.Dtos
{
    public class ContactRequest
    {
        [Required, MaxLength(200)]
        public required string Name { get; set; }

        [Required, MaxLength(320), EmailAddress]
        public required string Email { get; set; }

        [Required, MinLength(10), MaxLength(2000)]
        public required string Message { get; set; }
    }
}
