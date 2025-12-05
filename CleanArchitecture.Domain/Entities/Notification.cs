using CleanArchitecture.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Entities
{
    /// <summary>
    /// Email/SMS/WhatsApp notifications history.
    /// </summary>
    public class Notification : BaseEntity
    {
        public string? Type { get; set; }           // email/sms/whatsapp
        public string? ToAddress { get; set; }      // recipient
        public string? Subject { get; set; }
        public string? Message { get; set; }
        public string? Status { get; set; }         // pending/sent/failed
        public string? ErrorMessage { get; set; }
        public DateTime? SentAt { get; set; }
        public int AttemptCount { get; set; } = 0;
    }

}
