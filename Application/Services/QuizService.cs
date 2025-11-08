using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ChatService.Application.Services
{
    public class QuizService : IQuizService
    {
        private static readonly Random _random = new Random();
        private static readonly HashSet<string> StopWords = new HashSet<string>
        {
            "the","and","with","that","from","this","for","all","are","our","has","was","in","on","of","to","by","as","it","is","be","at","or"
        };

        public async Task<List<QuizResponse>> GenerateQuizAsync(QuizRequest request)
        {
            string text = request.Text?.Trim() ?? "Knowledge is power.";

            // Step 1: Clean the text
            string cleanText = Regex.Replace(text, @"\[[^\]]*\]", "");
            cleanText = Regex.Replace(cleanText, @"\s+", " ");

            // Step 2: Split sentences
            var sentences = cleanText.Split(new[] { '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(s => s.Trim())
                                     .Where(s => s.Length > 3)
                                     .ToList();

            if (!sentences.Any())
                sentences.Add("This is a default sentence to generate a quiz.");

            // Step 3: Extract keywords using simple frequency + TextRank approximation
            var words = Regex.Matches(cleanText.ToLower(), @"\b[a-z]{4,}\b")
                             .Select(m => m.Value)
                             .Where(w => !StopWords.Contains(w))
                             .GroupBy(w => w)
                             .OrderByDescending(g => g.Count())
                             .Select(g => g.Key)
                             .Take(50)
                             .ToList();

            if (!words.Any())
                words.AddRange(new[] { "knowledge", "science", "learning", "example" });

            var quiz = new List<QuizResponse>();

            // Step 4: Generate 5 questions with smarter wrong options
            for (int i = 0; i < 5; i++)
            {
                string sentence = sentences[_random.Next(sentences.Count)];

                // Pick the “most relevant” keyword in the sentence if exists
                var possibleAnswers = words.Where(w => sentence.ToLower().Contains(w)).ToList();
                string answer = possibleAnswers.Any() ? possibleAnswers[_random.Next(possibleAnswers.Count)]
                                                      : words[_random.Next(words.Count)];

                string question = GenerateQuestion(sentence, answer, request.Subject, request.Difficulty);

                // Generate wrong options using semantic similarity (approximation: random other keywords)
                var wrongOptions = words.Where(w => w != answer)
                                        .OrderBy(x => _random.Next())
                                        .Take(3)
                                        .ToList();

                var options = wrongOptions.Append(answer)
                                          .OrderBy(x => _random.Next())
                                          .ToList();

                while (options.Count < 4)
                    options.Add("Option" + (char)('A' + options.Count));

                quiz.Add(new QuizResponse
                {
                    Question = question,
                    Options = options,
                    Answer = answer
                });
            }

            return quiz;
        }

        private string GenerateQuestion(string sentence, string answer, string subject, string difficulty)
        {
            // More sophisticated templates with varied phrasing
            var templatesEasy = new[]
            {
                $"Which of the following terms is related to {subject}?",
                $"Identify the correct term mentioned in the text.",
                $"What word in the following sentence refers to {subject}? \"{sentence}\"",
                $"Choose the correct answer from the options below related to {subject}."
            };

            var templatesMedium = new[]
            {
                $"In the context of this sentence: \"{sentence}\", which word is significant?",
                $"What concept is highlighted in: \"{sentence}\"?",
                $"Which option best describes an element of {subject}?"
            };

            var templatesHard = new[]
            {
                $"Fill in the blank: {sentence.Replace(answer, "____")}",
                $"From the text, which concept accurately fits here: \"{sentence}\"?",
                $"Analyze the sentence: \"{sentence}\". Which option best explains the meaning of \"{answer}\"?"
            };

            return difficulty?.ToLower() switch
            {
                "easy" => templatesEasy[_random.Next(templatesEasy.Length)],
                "hard" => templatesHard[_random.Next(templatesHard.Length)],
                _ => templatesMedium[_random.Next(templatesMedium.Length)]
            };
        }
    }
}
