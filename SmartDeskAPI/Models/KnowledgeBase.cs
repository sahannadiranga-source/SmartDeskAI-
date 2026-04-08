namespace SmartDeskAPI.Models
{
    public class KnowledgeBase
    {
        public string Company_Name { get; set; }
        public string Headquarters { get; set; }
        public List<Faq> Faqs { get; set; }
    }
}
