using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Persistence.Configurations
{
    public class AppointmentSlotConfiguration : IEntityTypeConfiguration<AppointmentSlot>
    {
        public void Configure(EntityTypeBuilder<AppointmentSlot> builder)
        {
            builder.ToTable("AppointmentSlots");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SlotDate);
            builder.Property(x => x.StartTime);
            builder.Property(x => x.EndTime);

            builder.Property(x => x.IsBooked);
        }
    }

}
