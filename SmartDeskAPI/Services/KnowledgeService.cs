namespace SmartDeskAPI.Services
{
    using SmartDeskAPI.Models;
    using System.Text.Json;

    public class KnowledgeService
    {
        private readonly KnowledgeBase _knowledgeBase;

        private static readonly HashSet<string> StopWords = new()
        {
            "what", "when", "where", "which", "who", "whom", "why", "how",
            "does", "do", "did", "is", "are", "was", "were", "will", "would",
            "can", "could", "should", "have", "has", "your", "you", "their",
            "this", "that", "these", "those", "with", "from", "about", "into",
            "give", "tell", "show", "know", "need", "want", "like", "just",
            "also", "more", "some", "than", "then", "them", "they", "there"
        };

        private const string DefaultAnswer = "I'm not sure about that. Please contact our support team at info@ekara.nz or call +64 21 499 224.";

        public KnowledgeService()
        {
            var json = File.ReadAllText("Data/knowledge-base.json");
            _knowledgeBase = JsonSerializer.Deserialize<KnowledgeBase>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public string GetFullContext()
        {
            if (_knowledgeBase == null) return string.Empty;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Company: {_knowledgeBase.Company_Name}, headquartered in {_knowledgeBase.Headquarters}.");
            sb.AppendLine($"Contact: Email {_knowledgeBase.Contact?.Email}, Phone {_knowledgeBase.Contact?.Phone}.");
            sb.AppendLine();

            if (_knowledgeBase.Faqs != null)
            {
                foreach (var faq in _knowledgeBase.Faqs)
                {
                    sb.AppendLine($"Q: {faq.Question}");
                    sb.AppendLine($"A: {faq.Answer}");
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        public string GetAnswer(string userQuestion)
        {
            if (_knowledgeBase == null || _knowledgeBase.Faqs == null)
                return DefaultAnswer;

            var original = userQuestion.ToLower();

            if (original.Contains("country") || original.Contains("location") ||
                original.Contains("headquarter") || original.Contains("based") || original.Contains("where"))
            {
                return $"Ekara Digital is headquartered in {_knowledgeBase.Headquarters}. " +
                       $"Contact us at {_knowledgeBase.Contact?.Email} or {_knowledgeBase.Contact?.Phone}.";
            }

            var userWords = original
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3 && !StopWords.Contains(w))
                .ToList();

            string bestAnswer = DefaultAnswer;
            int bestScore = 0;

            foreach (var faq in _knowledgeBase.Faqs)
            {
                int score = 0;
                var faqQuestion = faq.Question.ToLower();

                if (original.Contains(faqQuestion))
                    score += 10;

                var faqWords = faqQuestion
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 3 && !StopWords.Contains(w))
                    .ToList();

                foreach (var word in faqWords)
                    if (original.Contains(word)) score += 2;

                foreach (var word in userWords)
                    if (faqQuestion.Contains(word)) score += 2;

                if (faq.Metadata?.Tags != null)
                    foreach (var tag in faq.Metadata.Tags)
                        if (original.Contains(tag.ToLower())) score += 4;

                if (!string.IsNullOrEmpty(faq.Category) && original.Contains(faq.Category.ToLower()))
                    score += 2;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestAnswer = faq.Answer;
                }
            }

            return bestScore < 2 ? DefaultAnswer : bestAnswer;
        }
    }
}
