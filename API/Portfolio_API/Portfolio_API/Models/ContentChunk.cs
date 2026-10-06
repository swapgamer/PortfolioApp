using System.ComponentModel.DataAnnotations;

namespace Portfolio_API.Models
{
    public class ContentChunk
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public required string Section { get; set; }

        [Required]
        public required string Text { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
    }
}
