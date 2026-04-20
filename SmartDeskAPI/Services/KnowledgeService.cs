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

        private const string DefaultAnswer = "I'm not sure about that. Please contact our support team at info@fuso.nz or call +64 21 499 224.";

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
                return $"fuso Digital is headquartered in {_knowledgeBase.Headquarters}. " +
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
                var faqAnswer = faq.Answer.ToLower();

                // Exact full question match
                if (original.Contains(faqQuestion))
                    score += 10;

                var faqWords = faqQuestion
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 3 && !StopWords.Contains(w))
                    .ToList();

                // FAQ question words found in user message
                foreach (var word in faqWords)
                    if (original.Contains(word)) score += 2;

                // User words found in FAQ question
                foreach (var word in userWords)
                {
                    if (faqQuestion.Contains(word)) score += 2;
                    // Also match against answer text (lower weight)
                    if (faqAnswer.Contains(word)) score += 1;
                    // Partial/stem match: user word starts with or contains a faq word stem
                    foreach (var fw in faqWords)
                        if (fw.Length >= 4 && word.Length >= 4 && (word.StartsWith(fw[..4]) || fw.StartsWith(word[..4])))
                            score += 1;
                }

                // Tag matches
                if (faq.Metadata?.Tags != null)
                    foreach (var tag in faq.Metadata.Tags)
                        if (original.Contains(tag.ToLower())) score += 4;

                // Category match
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
