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
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type).HasMaxLength(50);
            builder.Property(x => x.ToAddress).HasMaxLength(255);
            builder.Property(x => x.Subject).HasMaxLength(255);
            builder.Property(x => x.Status).HasMaxLength(50);
            builder.Property(x => x.ErrorMessage).HasColumnType("NVARCHAR(MAX)");
            builder.Property(x => x.Message).HasColumnType("NVARCHAR(MAX)");
        }
    }

}
