using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Common
{
    /// <summary>
    /// Base entity with common audit fields. All models inherit this.
    /// Id is required (Guid) to keep identity consistent across services.
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>Primary key</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>UTC creation timestamp (non-nullable)</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>UTC last update timestamp (nullable)</summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>UTC deletion timestamp (nullable). Soft-delete when set.</summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>Arbitrary JSON for future extension (nullable).</summary>
        public string? AdditionalData { get; set; }
    }

}
