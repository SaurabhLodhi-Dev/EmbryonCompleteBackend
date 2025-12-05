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
    public class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
    {
        public void Configure(EntityTypeBuilder<ErrorLog> builder)
        {
            builder.ToTable("ErrorLogs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Endpoint).HasMaxLength(300);
            builder.Property(x => x.HttpMethod).HasMaxLength(20);
            builder.Property(x => x.UserAgent).HasMaxLength(300);
            builder.Property(x => x.IpAddress).HasMaxLength(50);

            builder.Property(x => x.ErrorMessage).HasColumnType("NVARCHAR(MAX)");
            builder.Property(x => x.StackTrace).HasColumnType("NVARCHAR(MAX)");
        }
    }

}
