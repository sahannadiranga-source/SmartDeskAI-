namespace SmartDeskAPI.Interfaces
{
    public interface IAiAdapter
    {
        Task<string?> AskAsync(string userMessage, string knowledgeContext);
    }
}
