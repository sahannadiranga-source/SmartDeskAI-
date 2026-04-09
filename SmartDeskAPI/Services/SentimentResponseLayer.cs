namespace SmartDeskAPI.Services
{
    public class SentimentResponseLayer
    {
        private static readonly string FallbackMarker = "I'm not sure about that";

        public string Apply(string answer, double sentimentScore, bool isEscalation)
        {
            bool hasUsefulAnswer = !answer.StartsWith(FallbackMarker);

            if (isEscalation)
            {
                if (hasUsefulAnswer)
                    return $"Priority Support: We're sorry you're having a difficult experience. Our team is here to help you right away.\n\n{answer}\n\nIf this issue persists, please contact us directly at info@ekara.nz or call +64 21 499 224.";

                return "Priority Support: We're sorry you're having a difficult experience. Our team is here to help you right away.\n\nPlease reach out to us directly so we can resolve this for you:\nEmail: info@ekara.nz\nPhone: +64 21 499 224\n\nOur team responds within 2-3 business hours for priority cases.";
            }

            if (sentimentScore >= 0.6)
                return $"Great to hear you're having a positive experience!\n\n{answer}";

            if (sentimentScore >= 0.2)
                return $"Thanks for reaching out!\n\n{answer}";

            if (sentimentScore >= -0.2)
                return answer;

            if (hasUsefulAnswer)
                return $"We're sorry to hear you're facing some trouble. Let us help.\n\n{answer}\n\nFeel free to reach out at info@ekara.nz if you need further assistance.";

            return "We're sorry to hear you're having trouble. Please contact our support team directly:\nEmail: info@ekara.nz\nPhone: +64 21 499 224";
        }
    }
}
