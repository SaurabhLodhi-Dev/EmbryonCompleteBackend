using CleanArchitecture.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Entities
{
    /// <summary>
    /// City reference. Belongs to a State.
    /// </summary>
    public class City : BaseEntity
    {
        public Guid? StateId { get; set; }
        public string? Name { get; set; }

        // Navigation
        public State? State { get; set; }
    }

}
