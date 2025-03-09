namespace WebhookReceiver.Models
{
    public class Alert
    {
        public int Id { get; set; }

        public string AlertName { get; set; }

        public string Severity { get; set; }

        public string Resource { get; set; }

        public string Description { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
