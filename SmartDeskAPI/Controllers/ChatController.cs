using Microsoft.AspNetCore.Mvc;
using SmartDeskAPI.Interfaces;
using SmartDeskAPI.Models;
using SmartDeskAPI.Services;
using SmartDeskAPI.Utils;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IResponseStrategy _responseStrategy;
    private readonly SentimentService _sentimentService;
    private readonly SessionService _sessionService;
    private readonly SentimentResponseLayer _sentimentResponseLayer;

    public ChatController(IResponseStrategy responseStrategy, SentimentService sentimentService,
        SessionService sessionService, SentimentResponseLayer sentimentResponseLayer)
    {
        _responseStrategy = responseStrategy;
        _sentimentService = sentimentService;
        _sessionService = sessionService;
        _sentimentResponseLayer = sentimentResponseLayer;
    }

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { error = "Message cannot be empty." });

        var sessionId = string.IsNullOrWhiteSpace(request.SessionId)
            ? _sessionService.CreateSession()
            : request.SessionId;

        _sessionService.AddMessage(sessionId, "user", request.Message);

        double sentiment = _sentimentService.Analyze(request.Message);
        bool escalation = sentiment < -0.6;

        var history = _sessionService.GetHistory(sessionId);
        string rawAnswer = await _responseStrategy.ResolveAsync(request.Message, history);
        string answer = _sentimentResponseLayer.Apply(rawAnswer, sentiment, escalation);

        _sessionService.AddMessage(sessionId, "assistant", answer);

        var response = new ChatResponse
        {
            SessionId = sessionId,
            Answer = answer,
            Sentiment = sentiment,
            Priority_Escalation = escalation,
            History = _sessionService.GetHistory(sessionId)
        };

        var (isValid, validationError) = ResponseValidator.Validate(response);
        if (!isValid)
            return StatusCode(500, new { error = validationError });

        return Ok(response);
    }

    [HttpDelete("session/{sessionId}")]
    public IActionResult ResetSession(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return BadRequest(new { error = "SessionId is required." });

        _sessionService.ResetSession(sessionId);
        return Ok(new { message = $"Session '{sessionId}' has been reset." });
    }
}
