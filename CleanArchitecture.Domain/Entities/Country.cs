using CleanArchitecture.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Entities
{
    /// <summary>
    /// Country reference (name, ISO code, phone code).
    /// </summary>
    public class Country : BaseEntity
    {
        public string? Name { get; set; }
        public string? IsoCode { get; set; }      // e.g. "IN", "US"
        public string? PhoneCode { get; set; }    // e.g. "+91"

        // Navigation
        public ICollection<State>? States { get; set; }
    }

}
