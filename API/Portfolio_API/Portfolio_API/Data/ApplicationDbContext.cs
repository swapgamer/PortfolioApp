using Microsoft.EntityFrameworkCore;
using Portfolio_API.Models;

namespace Portfolio_API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ContentChunk> ContentChunks { get; set; } = null!;
        public DbSet<ExperienceEntry> ExperienceEntries { get; set; } = null!;
        public DbSet<EducationEntry> EducationEntries { get; set; } = null!;
        public DbSet<Testimonial> Testimonials { get; set; } = null!;
        public DbSet<ContactMessage> ContactMessages { get; set; } = null!;
    }
}
