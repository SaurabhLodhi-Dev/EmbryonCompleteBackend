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
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TableName).HasMaxLength(100);
            builder.Property(x => x.Action).HasMaxLength(20);
            builder.Property(x => x.CreatedBy).HasMaxLength(200);

            builder.Property(x => x.OldData).HasColumnType("NVARCHAR(MAX)");
            builder.Property(x => x.NewData).HasColumnType("NVARCHAR(MAX)");
        }
    }

}
