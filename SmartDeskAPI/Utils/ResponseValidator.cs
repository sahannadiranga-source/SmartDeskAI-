namespace SmartDeskAPI.Utils
{
    using SmartDeskAPI.Models;

    public static class ResponseValidator
    {
        public static (bool isValid, string error) Validate(ChatResponse response)
        {
            if (response == null)
                return (false, "Response object is null.");

            if (string.IsNullOrWhiteSpace(response.SessionId))
                return (false, "SessionId is missing.");

            if (string.IsNullOrWhiteSpace(response.Answer))
                return (false, "Answer is missing.");

            if (response.Sentiment < -1.0 || response.Sentiment > 1.0)
                return (false, $"Sentiment score {response.Sentiment} is out of range [-1.0, 1.0].");

            return (true, string.Empty);
        }
    }
}
