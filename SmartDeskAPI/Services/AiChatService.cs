namespace SmartDeskAPI.Services
{
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using SmartDeskAPI.Interfaces;

    public class AiChatService : IAiAdapter
    {
        private readonly HttpClient _httpClient;
        private readonly string _model;

        public AiChatService(IConfiguration config, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            var apiKey = config["HuggingFace:ApiKey"];
            _model = config["HuggingFace:Model"] ?? "HuggingFaceH4/zephyr-7b-beta";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
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

                // Zephyr/Mistral instruct format uses <|system|> / <|user|> / <|assistant|> tags
                var prompt = $"<|system|>\nYou are a helpful support assistant for Ekara Digital Partners, Wellington, New Zealand.\n" +
                             $"Tone: {tone}\n" +
                             $"Answer ONLY using the knowledge below. If the answer is not in the knowledge, say you are not sure and provide the contact details.\n\n" +
                             $"Knowledge:\n{knowledgeContext}\n</s>\n" +
                             $"<|user|>\n{userMessage}\n</s>\n" +
                             $"<|assistant|>\n";

                var payload = JsonSerializer.Serialize(new
                {
                    inputs = prompt,
                    parameters = new { max_new_tokens = 250, temperature = 0.3, return_full_text = false }
                });

                var url = $"https://api-inference.huggingface.co/models/{_model}";
                var response = await _httpClient.PostAsync(url,
                    new StringContent(payload, Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"AI response failed with status: {response.StatusCode}. Using fallback.");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var generated = root[0].GetProperty("generated_text").GetString()?.Trim();
                    return string.IsNullOrWhiteSpace(generated) ? null : generated;
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("AI request timed out. Using fallback.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI request failed: {ex.Message}. Using fallback.");
            }

            return null;
        }
    }
}
