using ChatService.Settings.Data;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Database = _context.Database.CanConnect() ? "Connected" : "Disconnected"
            });
        }

        [HttpGet]
        public IActionResult Get() => Ok("ChatServer is running 🧩");
    }
}
