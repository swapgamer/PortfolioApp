using System.ComponentModel.DataAnnotations;

namespace Portfolio_API.Models
{
    public class Testimonial
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public required string AuthorName { get; set; }

        // optional - not every testimonial has a title/company attached
        [MaxLength(200)]
        public string? AuthorTitle { get; set; }

        [Required, MaxLength(1000)]
        public required string Quote { get; set; }

        public int DisplayOrder { get; set; }
    }
}
