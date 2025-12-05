using CleanArchitecture.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Entities
{
    /// <summary>
    /// State/Region reference. Belongs to a Country.
    /// </summary>
    public class State : BaseEntity
    {
        public Guid? CountryId { get; set; }
        public string? Name { get; set; }

        // Navigation
        public Country? Country { get; set; }
        public ICollection<City>? Cities { get; set; }
    }

}
