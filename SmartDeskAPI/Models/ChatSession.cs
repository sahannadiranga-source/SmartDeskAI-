namespace SmartDeskAPI.Models
{
    public class ChatSession
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();

        public List<ChatMessage> Messages { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
