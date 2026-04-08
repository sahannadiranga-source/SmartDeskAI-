namespace SmartDeskAPI.Models
{
    public class ChatMessage
    {
        public string UserMessage { get; set; }
        public string BotResponse { get; set; }
        public string Sentiment { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
