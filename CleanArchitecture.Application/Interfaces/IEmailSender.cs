using System.Threading.Tasks;

namespace CleanArchitecture.Application.Interfaces
{
    public interface IEmailSender
    {
        /// <summary>
        /// Send an email (HTML or plain) synchronously from calling code.
        /// This method is used by the background worker. Use QueueEmailAsync for enqueueing from services.
        /// </summary>
        Task SendAsync(string fromName, string fromEmail, string toEmail, string subject, string htmlMessage, string plainTextMessage = "");
    }
}
