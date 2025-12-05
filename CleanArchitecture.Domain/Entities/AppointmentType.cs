using CleanArchitecture.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Entities
{
    /// <summary>
    /// Type of appointment (Zoom/WebEx/Google Meet). 
    /// </summary>
    public class AppointmentType : BaseEntity
    {
        public string? Name { get; set; }        // "Zoom", "WebEx", "GoogleMeet"
        public string? Description { get; set; }

        // Navigation
        public ICollection<Appointment>? Appointments { get; set; }
    }

}
