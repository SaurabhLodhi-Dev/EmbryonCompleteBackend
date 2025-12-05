using CleanArchitecture.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Entities
{
    /// <summary>
    /// Contact form submission.
    /// </summary>
    public class ContactSubmission : BaseEntity
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? PhoneCountryCode { get; set; }
        public Guid? CountryId { get; set; }
        public string? State { get; set; }   // free-text (client asked)
        public string? City { get; set; }    // free-text
        public string? Subject { get; set; }
        public string? Message { get; set; }

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        // Navigation
        public Country? Country { get; set; }
    }

}
