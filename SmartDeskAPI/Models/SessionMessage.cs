namespace SmartDeskAPI.Models
{
    public class SessionMessage
    {
        public string Role { get; set; }   // "user" or "assistant"
        public string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
