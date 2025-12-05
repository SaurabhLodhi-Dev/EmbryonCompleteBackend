using CleanArchitecture.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Entities
{
    /// <summary>
    /// Booked appointment record.
    /// </summary>
    public class Appointment : BaseEntity
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Description { get; set; }

        public Guid? AppointmentTypeId { get; set; }
        public Guid? SlotId { get; set; }

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        // Navigation
        public AppointmentType? AppointmentType { get; set; }
        public AppointmentSlot? Slot { get; set; }
    }

}
