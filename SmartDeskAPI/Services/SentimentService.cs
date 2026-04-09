namespace SmartDeskAPI.Services
{
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;

    public class SentimentService
    {
        private readonly HttpClient _httpClient;
        private readonly string _model = "cardiffnlp/twitter-roberta-base-sentiment-latest";

        private readonly Dictionary<string, double> _lexicon = new()
        {
            { "good", 0.5 }, { "great", 0.7 }, { "excellent", 0.9 }, { "nice", 0.5 },
            { "love", 0.8 }, { "happy", 0.7 }, { "awesome", 0.8 }, { "perfect", 0.9 },
            { "thanks", 0.4 }, { "thank you", 0.5 }, { "helpful", 0.6 }, { "fast", 0.4 },
            { "easy", 0.4 }, { "works", 0.5 }, { "working", 0.4 }, { "satisfied", 0.7 },
            { "impressed", 0.7 }, { "wonderful", 0.8 },
            { "bad", -0.5 }, { "terrible", -0.8 }, { "worst", -0.9 }, { "hate", -0.8 },
            { "slow", -0.6 }, { "problem", -0.5 }, { "not working", -0.7 }, { "broken", -0.7 },
            { "useless", -0.8 }, { "awful", -0.8 }, { "frustrated", -0.7 }, { "frustrating", -0.7 },
            { "frustration", -0.7 }, { "annoying", -0.6 }, { "annoyed", -0.6 },
            { "disappointed", -0.7 }, { "disappointing", -0.7 }, { "error", -0.5 },
            { "fail", -0.6 }, { "failed", -0.6 }, { "failing", -0.6 }, { "crash", -0.7 },
            { "crashed", -0.7 }, { "down", -0.5 }, { "issue", -0.4 }, { "issues", -0.4 },
            { "wrong", -0.5 }, { "ridiculous", -0.7 }, { "unacceptable", -0.8 }, { "waste", -0.6 },
            { "horrible", -0.9 }, { "pathetic", -0.8 }, { "angry", -0.7 }, { "upset", -0.6 },
            { "poor", -0.5 }, { "worst ever", -1.0 }, { "not happy", -0.7 }, { "not good", -0.6 }
        };

        private readonly HashSet<string> _negations = new()
        {
            "not", "no", "never", "neither", "nor", "cannot", "can't", "won't", "don't", "doesn't", "isn't", "wasn't"
        };

        public SentimentService(IConfiguration config, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            var apiKey = config["HuggingFace:ApiKey"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task<double> AnalyzeAsync(string text)
        {
            try
            {
                var payload = JsonSerializer.Serialize(new { inputs = text });
                var url = $"https://api-inference.huggingface.co/models/{_model}";
                var response = await _httpClient.PostAsync(url,
                    new StringContent(payload, Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                    return AnalyzeLexicon(text);

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var results = root[0];
                    double best = 0;
                    string bestLabel = "neutral";

                    foreach (var item in results.EnumerateArray())
                    {
                        var score = item.GetProperty("score").GetDouble();
                        if (score > best)
                        {
                            best = score;
                            bestLabel = item.GetProperty("label").GetString()?.ToLower() ?? "neutral";
                        }
                    }

                    return bestLabel switch
                    {
                        "positive" => Math.Round(best, 2),
                        "negative" => Math.Round(-best, 2),
                        _ => 0.0
                    };
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Sentiment AI request timed out. Using lexicon fallback.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sentiment AI request failed: {ex.Message}. Using lexicon fallback.");
            }

            return AnalyzeLexicon(text);
        }

        public double AnalyzeLexicon(string text)
        {
            text = text.ToLower();
            double score = 0;
            int matchCount = 0;

            foreach (var entry in _lexicon.OrderByDescending(e => e.Key.Length))
            {
                if (text.Contains(entry.Key))
                {
                    score += entry.Value;
                    matchCount++;
                    text = text.Replace(entry.Key, " ");
                }
            }

            var tokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            bool negate = false;

            foreach (var token in tokens)
            {
                if (_negations.Contains(token)) { negate = true; continue; }
                if (_lexicon.TryGetValue(token, out double wordScore))
                {
                    score += negate ? -wordScore : wordScore;
                    matchCount++;
                }
                negate = false;
            }

            if (matchCount == 0) return 0.0;
            return Math.Clamp(Math.Round(score / matchCount, 2), -1.0, 1.0);
        }
    }
}
