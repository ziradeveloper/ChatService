namespace ChatService.Settings.JWT
{
    public class JwtRequestModel
    {
        public string ID { get; set; } = null;
        public string Email { get; set; } = null;
        public string Name { get; set; } = null;
        public string Role { get; set; } = "User";
        public string ProfilePicture { get; set; } = null;
    }
}
