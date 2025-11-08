using ChatService.Application.DTOs;

namespace ChatService.Application.Interfaces
{
    public interface IQuizService
    {
        Task<List<QuizResponse>> GenerateQuizAsync(QuizRequest request);
    }
}
