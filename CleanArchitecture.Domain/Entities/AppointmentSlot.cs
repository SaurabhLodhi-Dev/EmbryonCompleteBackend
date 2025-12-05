using CleanArchitecture.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Entities
{
    /// <summary>
    /// A slot (date + start/end time) that can be booked.
    /// </summary>
    public class AppointmentSlot : BaseEntity
    {
        public DateTime? SlotDate { get; set; }   // store date-only or normalize to UTC
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public bool? IsBooked { get; set; } = false;

        // Navigation
        public Appointment? Appointment { get; set; }
    }

}
