namespace SmartDeskAPI.Strategies
{
    using SmartDeskAPI.Interfaces;
    using SmartDeskAPI.Models;
    using SmartDeskAPI.Services;

    public class AiResponseStrategy : IResponseStrategy
    {
        private readonly IAiAdapter _aiAdapter;
        private readonly KnowledgeService _knowledgeService;

        public AiResponseStrategy(IAiAdapter aiAdapter, KnowledgeService knowledgeService)
        {
            _aiAdapter = aiAdapter;
            _knowledgeService = knowledgeService;
        }

        public async Task<string> ResolveAsync(string userMessage, List<SessionMessage> history)
        {
            string knowledgeContext = _knowledgeService.GetAnswer(userMessage);
            string? aiAnswer = await _aiAdapter.AskAsync(userMessage, knowledgeContext);
            return aiAnswer ?? knowledgeContext;
        }
    }
}
