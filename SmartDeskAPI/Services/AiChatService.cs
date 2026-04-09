namespace SmartDeskAPI.Services
{
    using System.Text;
    using System.Text.Json;
    using SmartDeskAPI.Interfaces;

    public class AiChatService : IAiAdapter
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string GeminiUrl =
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

        public AiChatService(IConfiguration config, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _apiKey = config["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key is missing.");
        }

        public async Task<string?> AskAsync(string userMessage, string knowledgeContext, double sentimentScore)
        {
            try
            {
                var tone = sentimentScore switch
                {
                    < -0.6 => "The user is very frustrated. Start with a sincere apology and offer immediate help.",
                    < -0.2 => "The user seems unhappy. Be understanding and helpful.",
                    > 0.6  => "The user is very positive. Be enthusiastic and friendly.",
                    > 0.2  => "The user is in a good mood. Be friendly and helpful.",
                    _      => "Respond in a professional and helpful tone."
                };

                var systemInstruction =
                    $"You are a helpful support assistant for Ekara Digital Partners, Wellington, New Zealand.\n" +
                    $"Tone: {tone}\n" +
                    $"Answer ONLY using the knowledge provided below. " +
                    $"If the answer is not in the knowledge, say you are not sure and provide: Email info@ekara.nz or Phone +64 21 499 224.\n" +
                    $"Keep answers concise and clear.\n\n" +
                    $"Knowledge:\n{knowledgeContext}";

                var payload = new
                {
                    system_instruction = new
                    {
                        parts = new[] { new { text = systemInstruction } }
                    },
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[] { new { text = userMessage } }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.3,
                        maxOutputTokens = 300
                    }
                };

                var url = $"{GeminiUrl}?key={_apiKey}";
                var response = await _httpClient.PostAsync(url,
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Gemini API failed: {response.StatusCode} - {errorBody}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString()
                    ?.Trim();

                return string.IsNullOrWhiteSpace(text) ? null : text;
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Gemini request timed out. Using fallback.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Gemini request failed: {ex.Message}. Using fallback.");
            }

            return null;
        }
    }
}
