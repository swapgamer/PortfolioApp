using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio_API.Data;
using Portfolio_API.Dtos;
using Portfolio_API.Models;
using Portfolio_API.Services;

namespace Portfolio_API.Controllers
{
    [ApiController]
    [Route("api/contact")]
    [EnableRateLimiting("contact")]
    public class ContactController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;

        public ContactController(ApplicationDbContext db, IEmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] ContactRequest request, CancellationToken ct)
        {
            var entity = new ContactMessage
            {
                Name = request.Name,
                Email = request.Email,
                Message = request.Message
            };

            _db.ContactMessages.Add(entity);
            await _db.SaveChangesAsync(ct);

            await _emailService.NotifyOwnerAsync(entity, ct);

            return Ok();
        }
    }
}
