using CleanArchitecture.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Entities
{
    /// <summary>
    /// Audit log for tracking DB-level changes (INSERT/UPDATE/DELETE).
    /// </summary>
    public class AuditLog : BaseEntity
    {
        public string? TableName { get; set; }
        public Guid? RecordId { get; set; }
        public string? Action { get; set; }      // INSERT, UPDATE, DELETE
        public string? OldData { get; set; }     // JSON
        public string? NewData { get; set; }     // JSON
        public string? CreatedBy { get; set; }
    }

}
