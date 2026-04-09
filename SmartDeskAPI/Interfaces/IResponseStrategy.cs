namespace SmartDeskAPI.Interfaces
{
    using SmartDeskAPI.Models;

    public interface IResponseStrategy
    {
        Task<string> ResolveAsync(string userMessage, List<SessionMessage> history, double sentimentScore);
    }
}
