namespace ChatService.Application.DTOs
{
    public class QuizRequest
    {
        public string Text { get; set; } = string.Empty;
        public string Subject { get; set; } = "General";
        public string Difficulty { get; set; } = "medium"; // default if not provided
    }
}
