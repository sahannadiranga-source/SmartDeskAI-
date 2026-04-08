namespace SmartDeskAPI.Services
{
    public class SentimentService
    {
        // Weighted word lexicon
        private readonly Dictionary<string, double> _lexicon = new()
        {
            // Positive
            { "good", 0.5 },
            { "great", 0.7 },
            { "excellent", 0.9 },
            { "nice", 0.5 },
            { "love", 0.8 },
            { "happy", 0.7 },
            { "awesome", 0.8 },
            { "perfect", 0.9 },
            { "thanks", 0.4 },
            { "thank you", 0.5 },
            { "helpful", 0.6 },
            { "fast", 0.4 },
            { "easy", 0.4 },
            { "works", 0.5 },
            { "working", 0.4 },
            { "satisfied", 0.7 },
            { "impressed", 0.7 },
            { "wonderful", 0.8 },

            // Negative
            { "bad", -0.5 },
            { "terrible", -0.8 },
            { "worst", -0.9 },
            { "hate", -0.8 },
            { "slow", -0.6 },
            { "problem", -0.5 },
            { "not working", -0.7 },
            { "broken", -0.7 },
            { "useless", -0.8 },
            { "awful", -0.8 },
            { "frustrated", -0.7 },
            { "annoying", -0.6 },
            { "disappointed", -0.7 },
            { "error", -0.5 },
            { "fail", -0.6 },
            { "failed", -0.6 },
            { "crash", -0.7 },
            { "down", -0.5 },
            { "issue", -0.4 },
            { "wrong", -0.5 },
            { "ridiculous", -0.7 },
            { "unacceptable", -0.8 },
            { "waste", -0.6 },
            { "horrible", -0.9 },
            { "pathetic", -0.8 }
        };

        // Negation words that flip the sentiment of the next word
        private readonly HashSet<string> _negations = new()
        {
            "not", "no", "never", "neither", "nor", "cannot", "can't", "won't", "don't", "doesn't", "isn't", "wasn't"
        };

        public double Analyze(string text)
        {
            text = text.ToLower();

            // Multi-word phrase check first (e.g. "not working", "thank you")
            double score = 0;
            int matchCount = 0;

            foreach (var entry in _lexicon.OrderByDescending(e => e.Key.Length))
            {
                if (text.Contains(entry.Key))
                {
                    score += entry.Value;
                    matchCount++;
                    // Remove matched phrase to avoid double-counting sub-words
                    text = text.Replace(entry.Key, " ");
                }
            }

            // Negation pass on remaining tokens
            var tokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            bool negate = false;

            foreach (var token in tokens)
            {
                if (_negations.Contains(token))
                {
                    negate = true;
                    continue;
                }

                if (_lexicon.TryGetValue(token, out double wordScore))
                {
                    score += negate ? -wordScore : wordScore;
                    matchCount++;
                }

                // Negation only applies to the immediately following sentiment word
                negate = false;
            }

            if (matchCount == 0) return 0.0;

            // Weighted average then clamp
            double normalized = score / matchCount;
            return Math.Clamp(Math.Round(normalized, 2), -1.0, 1.0);
        }
    }
}
