namespace SmartDeskAPI.Services
{
    using SmartDeskAPI.Models;
    using System.Text.Json;

    public class KnowledgeService
    {
        private readonly KnowledgeBase _knowledgeBase;

        public KnowledgeService()
        {
            var json = File.ReadAllText("Data/knowledge-base.json");
            _knowledgeBase = JsonSerializer.Deserialize<KnowledgeBase>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public string GetAnswer(string userQuestion)
        {
            if (_knowledgeBase == null || _knowledgeBase.Faqs == null)
                return "Knowledge base not loaded properly.";

            userQuestion = userQuestion.ToLower();
            var userWords = userQuestion.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            string bestAnswer = "Sorry, I couldn't find an answer. Please contact us at info@ekara.nz for further assistance.";
            int bestScore = 0;

            foreach (var faq in _knowledgeBase.Faqs)
            {
                int score = 0;
                var faqQuestion = faq.Question.ToLower();

                // Full question contained in user message
                if (userQuestion.Contains(faqQuestion))
                    score += 10;

                // Individual words from the FAQ question found in user message
                var faqWords = faqQuestion.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 3); // skip short stop words
                foreach (var word in faqWords)
                {
                    if (userQuestion.Contains(word))
                        score += 2;
                }

                // User words found in FAQ question
                foreach (var word in userWords.Where(w => w.Length > 3))
                {
                    if (faqQuestion.Contains(word))
                        score += 2;
                }

                // Tag matching
                if (faq.Metadata?.Tags != null)
                {
                    foreach (var tag in faq.Metadata.Tags)
                    {
                        if (userQuestion.Contains(tag.ToLower()))
                            score += 3;
                    }
                }

                // Category matching
                if (!string.IsNullOrEmpty(faq.Category) && userQuestion.Contains(faq.Category.ToLower()))
                    score += 2;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestAnswer = faq.Answer;
                }
            }

            return bestAnswer;
        }
    }
}
