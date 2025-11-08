using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ChatService.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatService.Settings.JWT
{
    public class JwtUtils
    {
        private readonly IConfiguration _configuration;

        public JwtUtils(IConfiguration config)
        {
            _configuration = config;
        }

        public JwtResponseModel GenerateToken(JwtRequestModel request)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, request.ID),
                new Claim(ClaimTypes.Email, request.Email),
                new Claim(ClaimTypes.Role, request.Role ?? "User"),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expirationMinutes = double.Parse(_configuration["Jwt:ExpirationMinutes"]);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: creds
            );

            return new JwtResponseModel
            {
                Id = request.ID,
                Email = request.Email,
                Name = request.Name,
                Role = request.Role,
                ProfilePicture = request.ProfilePicture ?? "",
                token = new JwtSecurityTokenHandler().WriteToken(token)
            };
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]);

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            return tokenHandler.ValidateToken(token, parameters, out _);
        }
    }
}
