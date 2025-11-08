namespace ChatService.Application.DTOs
{
    public class QuizResponse
    {
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public string Answer { get; set; } = string.Empty;
    }
}
