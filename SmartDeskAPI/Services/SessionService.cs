namespace SmartDeskAPI.Services
{
    using SmartDeskAPI.Models;
    using System.Collections.Concurrent;

    public class SessionService
    {
        private const int MaxHistorySize = 3;

        // sessionId -> ordered list of messages
        private readonly ConcurrentDictionary<string, List<SessionMessage>> _sessions = new();

        public List<SessionMessage> GetHistory(string sessionId)
        {
            return _sessions.TryGetValue(sessionId, out var history)
                ? new List<SessionMessage>(history)
                : new List<SessionMessage>();
        }

        public void AddMessage(string sessionId, string role, string content)
        {
            _sessions.AddOrUpdate(
                sessionId,
                _ => new List<SessionMessage> { new SessionMessage { Role = role, Content = content } },
                (_, history) =>
                {
                    lock (history)
                    {
                        history.Add(new SessionMessage { Role = role, Content = content });

                        // Keep only the last MaxHistorySize messages
                        if (history.Count > MaxHistorySize)
                            history.RemoveRange(0, history.Count - MaxHistorySize);
                    }
                    return history;
                });
        }

        public bool ResetSession(string sessionId)
        {
            return _sessions.TryRemove(sessionId, out _);
        }

        public string CreateSession()
        {
            var sessionId = Guid.NewGuid().ToString();
            _sessions[sessionId] = new List<SessionMessage>();
            return sessionId;
        }
    }
}
