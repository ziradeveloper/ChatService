using ChatService.Settings.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("reset_connection_key/{personId}")]
        public async Task<IActionResult> ResetConnectionKey(Guid personId)
        {
            var person = await _context.Person.FirstOrDefaultAsync(p => p.Id == personId);
            if (person == null)
                return NotFound();

            person.ConnectionKey = Guid.NewGuid().ToString();

            // Optionally clear all existing connections
            var oldConnections = _context.Connections.Where(c => c.PersonId == personId);
            _context.Connections.RemoveRange(oldConnections);

            await _context.SaveChangesAsync();
            return Ok(new { newKey = person.ConnectionKey });
        }

    }
}
