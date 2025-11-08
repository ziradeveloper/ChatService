using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatService.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int ProfileImageIndex { get; set; }
        public bool? IsGoogleAccount { get; set; }
        public string? EncryptedOpenAiToken { get; set; }
        public string Role { get; set; } = "User";
    }
}
