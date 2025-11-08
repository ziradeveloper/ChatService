using Microsoft.EntityFrameworkCore;
using ChatService.Application.Interfaces;
using ChatService.Domain.Models;
using ChatService.Models;
using ChatService.Settings;
using ChatService.Settings.Helper;
using ChatService.Settings.JWT;

namespace ChatService.Application.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        private readonly JwtUtils _jwt;

        public UserService(AppDbContext db, JwtUtils jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        public async Task<JwtResponseModel> RegisterAsync(RegisterRequest request)
        {
            var existing = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existing != null)
                throw new InvalidOperationException("User already exists");

            var user = new User
            {
                Fullname = request.Fullname,
                Email = request.Email,
                PasswordHash = Helper.HashPassword(request.Password),
                ProfileImageIndex = request.ProfileImageIndex
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var jwtRequest = new JwtRequestModel
            {
                ID = user.Id.ToString(),
                Email = user.Email,
                Name = user.Fullname,
                Role = "User"
            };

            return _jwt.GenerateToken(jwtRequest);
        }

        public async Task<JwtResponseModel> LoginAsync(string email, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !Helper.VerifyPassword(password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

            var jwtRequest = new JwtRequestModel
            {
                ID = user.Id.ToString(),
                Email = user.Email,
                Name = user.Fullname,
                Role = "User"
            };

            return _jwt.GenerateToken(jwtRequest);
        }

        public async Task<JwtResponseModel> GoogleLoginAsync(string email, string fullname, int profileIndex)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    user = new User
                    {
                        Email = email,
                        Fullname = fullname,
                        ProfileImageIndex = profileIndex,
                        IsGoogleAccount = true
                    };
                    _db.Users.Add(user);
                    await _db.SaveChangesAsync();
                }

                var jwtRequest = new JwtRequestModel
                {
                    ID = user.Id.ToString(),
                    Email = user.Email,
                    Name = user.Fullname,
                    Role = "User"
                };

                return _jwt.GenerateToken(jwtRequest);
            }
            catch (Exception ex)
            {
                throw new Exception("Save failed: " + ex.InnerException?.Message, ex);
            }
        }

        public async Task UpdateOpenAiTokenAsync(int userId, string token)
        {
            try
            {
                var user = await _db.Users.FindAsync(userId);
                if (user == null) throw new Exception("User not found");

                user.EncryptedOpenAiToken = EncryptionHelper.Encrypt(token);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Save failed: " + ex.InnerException?.Message, ex);
            }
        }

        public async Task<User> GetByIdAsync(int userId)
        {
            try
            {
                return await _db.Users.FindAsync(userId);
            }
            catch (Exception ex)
            {
                throw new Exception("Save failed: " + ex.InnerException?.Message, ex);
            }
        }
    }
}
