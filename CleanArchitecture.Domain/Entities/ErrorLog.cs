using CleanArchitecture.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Entities
{
    /// <summary>
    /// Error log entry for server-side exceptions and errors.
    /// </summary>
    public class ErrorLog : BaseEntity
    {
        public string? ErrorMessage { get; set; }
        public string? StackTrace { get; set; }
        public string? Endpoint { get; set; }
        public string? HttpMethod { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
    }

}
