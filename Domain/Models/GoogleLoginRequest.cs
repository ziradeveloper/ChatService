namespace ChatService.Domain.Models
{
    public class GoogleLoginRequest
    {
        public string Email { get; set; }
        public string Fullname { get; set; }
        public int ProfileImageIndex { get; set; }
    }
}
