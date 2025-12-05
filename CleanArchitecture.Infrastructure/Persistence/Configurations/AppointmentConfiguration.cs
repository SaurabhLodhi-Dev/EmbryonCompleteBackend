using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace CleanArchitecture.Infrastructure.Persistence.Configurations
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FirstName).HasMaxLength(150);
            builder.Property(x => x.LastName).HasMaxLength(150);
            builder.Property(x => x.Email).HasMaxLength(200);

            builder.Property(x => x.Description).HasColumnType("NVARCHAR(MAX)");
            builder.Property(x => x.IpAddress).HasMaxLength(50);
            builder.Property(x => x.UserAgent).HasMaxLength(300);

            builder.HasOne(x => x.AppointmentType)
                   .WithMany(a => a.Appointments)
                   .HasForeignKey(x => x.AppointmentTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Slot)
                   .WithOne(s => s.Appointment)
                   .HasForeignKey<Appointment>(x => x.SlotId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
