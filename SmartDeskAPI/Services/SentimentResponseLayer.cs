namespace SmartDeskAPI.Services
{
    /// <summary>
    /// Wraps a raw answer with tone-appropriate messaging based on sentiment score.
    /// Score ranges:
    ///   >= 0.6  : Very positive
    ///   0.2 to 0.6 : Mildly positive
    ///  -0.2 to 0.2 : Neutral
    ///  -0.6 to -0.2 : Mildly negative
    ///   < -0.6  : Frustrated / escalation
    /// </summary>
    public class SentimentResponseLayer
    {
        public string Apply(string answer, double sentimentScore, bool isEscalation)
        {
            if (isEscalation)
            {
                return $"🚨 Priority Support: We're sorry you're having a difficult experience. " +
                       $"Our team is here to help you right away.\n\n{answer}\n\n" +
                       $"If this issue persists, please contact us directly at info@ekara.nz or call +64 21 499 224.";
            }

            if (sentimentScore >= 0.6)
            {
                return $"😊 Great to hear you're having a positive experience!\n\n{answer}";
            }

            if (sentimentScore >= 0.2)
            {
                return $"Thanks for reaching out!\n\n{answer}";
            }

            if (sentimentScore >= -0.2)
            {
                // Neutral — return answer as-is
                return answer;
            }

            // Mildly negative (-0.6 to -0.2)
            return $"We're sorry to hear you're facing some trouble. Let us help.\n\n{answer}\n\n" +
                   $"Feel free to reach out at info@ekara.nz if you need further assistance.";
        }
    }
}
