namespace SmartDeskAPI.Strategies
{
    using SmartDeskAPI.Interfaces;
    using SmartDeskAPI.Models;
    using SmartDeskAPI.Services;

    public class FallbackResponseStrategy : IResponseStrategy
    {
        private readonly KnowledgeService _knowledgeService;

        public FallbackResponseStrategy(KnowledgeService knowledgeService)
        {
            _knowledgeService = knowledgeService;
        }

        public Task<string> ResolveAsync(string userMessage, List<SessionMessage> history)
        {
            return Task.FromResult(_knowledgeService.GetAnswer(userMessage));
        }
    }
}
