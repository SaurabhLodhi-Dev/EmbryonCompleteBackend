namespace CleanArchitecture.Application.Email
{
    public class QueuedEmail
    {
        public string ToEmail { get; set; } = "";
        public string Subject { get; set; } = "";
        public string HtmlBody { get; set; } = "";
        public string PlainBody { get; set; } = "";
        public string FromEmail { get; set; } = "";
        public string FromName { get; set; } = "";
        public Guid RelatedEntityId { get; set; } // optional - e.g., contact submission id
        public string? Type { get; set; } // "contact_user" | "contact_admin" etc.
        public int Attempt { get; set; } = 0;
    }
}
