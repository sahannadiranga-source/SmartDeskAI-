namespace SmartDeskAPI.Models
{
    public class ChatResponse
    {
        public string SessionId { get; set; }
        public string Answer { get; set; }
        public double Sentiment { get; set; }
        public bool Priority_Escalation { get; set; }
        public List<SessionMessage> History { get; set; }
    }
}
