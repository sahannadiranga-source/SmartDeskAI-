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
            var apiKey = config["HuggingFace:ApiKey"];
            _model = config["HuggingFace:Model"] ?? "facebook/blenderbot-400M-distill";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task<string?> AskAsync(string userMessage, string knowledgeContext)
        {
            var prompt = $"You are a helpful support assistant for Ekara Digital. Use the following knowledge to answer the user's question.\n\nKnowledge:\n{knowledgeContext}\n\nUser: {userMessage}\nAssistant:";

            var payload = JsonSerializer.Serialize(new
            {
                inputs = prompt,
                parameters = new { max_new_tokens = 200, temperature = 0.7 }
            });

            var url = $"https://api-inference.huggingface.co/models/{_model}";
            var response = await _httpClient.PostAsync(url,
                new StringContent(payload, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
            {
                var generated = root[0].GetProperty("generated_text").GetString();

                if (generated != null && generated.Contains("Assistant:"))
                    generated = generated[(generated.LastIndexOf("Assistant:") + "Assistant:".Length)..].Trim();

                return string.IsNullOrWhiteSpace(generated) ? null : generated;
            }

            return null;
        }
    }
}
