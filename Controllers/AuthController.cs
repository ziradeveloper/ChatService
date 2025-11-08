using ChatService.Application.Person.Model;
using ChatService.Application.User.Model;
using ChatService.Settings.Data;
using ChatService.Settings.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🧩 Register new user
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel data)
        {
            // Check if username already exists
            if (await _context.Person.AnyAsync(p => p.Username == data.Username))
                return BadRequest(new Result
                {
                    Success = false,
                    Message = "Username already taken"
                });
            Person person = new Person();
            // Generate permanent connection key
            person.Id = Guid.NewGuid();
            person.ConnectionKey = Guid.NewGuid().ToString();
            person.Name = data.Name;
            person.Username = data.Username;
            person.Password = data.Password;

            _context.Person.Add(person);
            await _context.SaveChangesAsync();

            return Ok(new Result
            {
                Success = true,
                Message = "User registered successfully",
                Data = new { person.Id, person.Username, person.ConnectionKey }
            });
        }

        // 🧠 Login existing user
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel person)
        {
            var existingUser = await _context.Person
                .FirstOrDefaultAsync(p => p.Username == person.Username && p.Password == person.Password);

            if (existingUser == null)
                return Unauthorized(new Result
                {
                    Success = false,
                    Message = "Invalid username or password"
                });

            return Ok(new Result
            {
                Success = true,
                Message = "Login successful",
                Data = new { existingUser.Id, existingUser.Username, existingUser.ConnectionKey }
            });
        }

        // 🔑 Re-authenticate using ConnectionKey (for SignalR)
        [HttpPost("reauth")]
        public async Task<IActionResult> Reauth([FromBody] string connectionKey)
        {
            var existingUser = await _context.Person
                .FirstOrDefaultAsync(p => p.ConnectionKey == connectionKey);

            if (existingUser == null)
                return Unauthorized(new Result
                {
                    Success = false,
                    Message = "Invalid connection key"
                });

            return Ok(new Result
            {
                Success = true,
                Message = "Re-auth successful",
                Data = new { existingUser.Id, existingUser.Username, existingUser.ConnectionKey }
            });
        }
    }
}