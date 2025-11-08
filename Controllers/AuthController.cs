using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChatService.Application.Interfaces;
using ChatService.Domain.Models;

namespace ChatService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var jwt = await _userService.RegisterAsync(request);
                return Ok(jwt);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred while registering." });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var jwt = await _userService.LoginAsync(request.Email, request.Password);
                return Ok(jwt);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred while logging in." });
            }
        }

        [AllowAnonymous]
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            var jwt = await _userService.GoogleLoginAsync(request.Email, request.Fullname, request.ProfileImageIndex);
            return Ok(jwt);
        }

        [HttpPost("set-openai-token")]
        public async Task<IActionResult> SetOpenAiToken([FromBody] OpenAiTokenRequest request)
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null) return Unauthorized();

            if (!int.TryParse(userIdString, out int userId))
                return BadRequest(new { message = "Invalid user ID" });

            await _userService.UpdateOpenAiTokenAsync(userId, request.Token);
            return Ok(new { message = "Token saved successfully." });
        }
    }
}
