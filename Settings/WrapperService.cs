using ChatService.Settings.JWT;
using System.Security.Claims;

namespace ChatService.Settings
{
    public class WrapperService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JwtUtils _jwtUtils; // Inject JwtUtils to use its methods

        public WrapperService(IHttpContextAccessor httpContextAccessor, JwtUtils jwtUtils)
        {
            _httpContextAccessor = httpContextAccessor;
            _jwtUtils = jwtUtils; // Initialize JwtUtils
        }

        // Method to get the UserId from the JWT token
        public string GetUserId()
        {
            var token = GetTokenFromRequest();
            if (string.IsNullOrEmpty(token)) return null;

            var claimsPrincipal = _jwtUtils.ValidateToken(token);
            var userIdClaim = claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim?.Value; // returns the UserId from the token if available
        }

        // Method to get the Username from the JWT token
        public string GetUsername()
        {
            var token = GetTokenFromRequest();
            if (string.IsNullOrEmpty(token)) return null;

            var claimsPrincipal = _jwtUtils.ValidateToken(token);
            var usernameClaim = claimsPrincipal?.FindFirst(ClaimTypes.Name);
            return usernameClaim?.Value; // returns the Username from the token if available
        }

        // Method to get the User Role from the JWT token
        public string GetUserRole()
        {
            var token = GetTokenFromRequest();
            if (string.IsNullOrEmpty(token)) return null;

            var claimsPrincipal = _jwtUtils.ValidateToken(token);
            var roleClaim = claimsPrincipal?.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value; // returns the Role from the token if available
        }

        // Helper method to extract any claim from the JWT
        public string GetClaim(string claimType)
        {
            var token = GetTokenFromRequest();
            if (string.IsNullOrEmpty(token)) return null;

            var claimsPrincipal = _jwtUtils.ValidateToken(token);
            var claim = claimsPrincipal?.FindFirst(claimType);
            return claim?.Value; // returns the specified claim from the token if available
        }

        // Helper method to extract the JWT token from the request's Authorization header
        private string GetTokenFromRequest()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (token?.StartsWith("Bearer ") == true)
            {
                return token.Substring("Bearer ".Length).Trim();
            }
            else if (token?.StartsWith("Bearer ") == false)
            {
                return token;
            }
            return null;
        }
    }
}
