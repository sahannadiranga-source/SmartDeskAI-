namespace SmartDeskAPI.Models
{
    public class KnowledgeBase
    {
        public string Company_Name { get; set; }
        public string Headquarters { get; set; }
        public Contact Contact { get; set; }
        public List<Faq> Faqs { get; set; }
    }

    public class Contact
    {
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
