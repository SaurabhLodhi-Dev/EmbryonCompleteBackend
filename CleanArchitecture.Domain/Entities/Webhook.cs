using CleanArchitecture.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Entities
{
    /// <summary>
    /// Configured webhooks for integrations.
    /// </summary>
    public class Webhook : BaseEntity
    {
        public string? Url { get; set; }
        public string? Event { get; set; }
        public string? SecretKey { get; set; }
    }

}
