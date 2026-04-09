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

        public async Task<string> ResolveAsync(string userMessage, List<SessionMessage> history, double sentimentScore)
        {
            // Pass the full knowledge base so the AI has all context to reason from
            string fullContext = _knowledgeService.GetFullContext();
            string? aiAnswer = await _aiAdapter.AskAsync(userMessage, fullContext, sentimentScore);

            // If AI fails, fall back to direct knowledge base lookup
            if (aiAnswer == null)
                return _knowledgeService.GetAnswer(userMessage);

            return aiAnswer;
        }
    }
}
