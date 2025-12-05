using CleanArchitecture.Domain.Entities;
using System.Text.Encodings.Web;

namespace CleanArchitecture.Application.Email
{
    public static class EmailTemplates
    {
        public static string ContactUserConfirmation(ContactSubmission s)
        {
            // Use null-coalescing to handle possible null values
            var name = HtmlEncoder.Default.Encode($"{s.FirstName ?? string.Empty} {s.LastName ?? string.Empty}");
            var message = HtmlEncoder.Default.Encode(s.Message ?? string.Empty);
            return $@"
                <html>
                  <body>
                    <p>Hi {name},</p>
                    <p>Thank you for contacting us. We received your message and will get back to you shortly.</p>
                    <h4>Your message</h4>
                    <p>{message}</p>
                    <p>Best regards,<br/>Team</p>
                  </body>
                </html>";
        }

        public static string ContactUserConfirmationPlain(ContactSubmission s)
        {
            // Handle null values for properties like Message
            return $"Hi {s.FirstName ?? string.Empty} {s.LastName ?? string.Empty},\n\nThank you for contacting us. We received your message:\n\n{s.Message ?? string.Empty}\n\nBest regards,\nTeam";
        }

        public static string ContactAdminNotification(ContactSubmission s)
        {
            // Handle possible null values for each property
            var name = HtmlEncoder.Default.Encode($"{s.FirstName ?? string.Empty} {s.LastName ?? string.Empty}");
            var message = HtmlEncoder.Default.Encode(s.Message ?? string.Empty);
            var email = HtmlEncoder.Default.Encode(s.Email ?? string.Empty);
            var phone = HtmlEncoder.Default.Encode(s.Phone ?? string.Empty);
            var city = HtmlEncoder.Default.Encode(s.City ?? string.Empty);
            var state = HtmlEncoder.Default.Encode(s.State ?? string.Empty);
            var ip = HtmlEncoder.Default.Encode(s.IpAddress ?? string.Empty);

            return $@"
                <html>
                  <body>
                    <p>New contact submission received.</p>
                    <p><strong>From:</strong> {name} &lt;{email}&gt;</p>
                    <p><strong>Phone:</strong> {phone}</p>
                    <p><strong>Country/State/City:</strong> {city}/{state}</p>
                    <h4>Message</h4>
                    <p>{message}</p>
                    <p>IP: {ip}</p>
                    <p>CreatedAt (UTC): {s.CreatedAt:O}</p>
                  </body>
                </html>";
        }

        public static string ContactAdminNotificationPlain(ContactSubmission s)
        {
            // Handle possible null values for properties like Message
            return $"New contact submission.\nFrom: {s.FirstName ?? string.Empty} {s.LastName ?? string.Empty} <{s.Email ?? string.Empty}>\nPhone: {s.Phone ?? string.Empty}\nLocation: {s.City ?? string.Empty}/{s.State ?? string.Empty}\n\nMessage:\n{s.Message ?? string.Empty}\n\nIP: {s.IpAddress ?? string.Empty}\nCreatedAt: {s.CreatedAt:O}";
        }
    }
}
