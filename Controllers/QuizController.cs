using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;

namespace ChatService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        [Authorize]
        [HttpPost("generate")]
        public async Task<ActionResult<List<QuizResponse>>> GenerateQuiz([FromBody] QuizRequest request)
        {
            var quiz = await _quizService.GenerateQuizAsync(request);
            return Ok(quiz);
        }
    }
}
