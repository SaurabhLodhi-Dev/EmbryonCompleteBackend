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
    public class PageVisitConfiguration : IEntityTypeConfiguration<PageVisit>
    {
        public void Configure(EntityTypeBuilder<PageVisit> builder)
        {
            builder.ToTable("PageVisits");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PageUrl).HasMaxLength(500);
            builder.Property(x => x.IpAddress).HasMaxLength(50);
            builder.Property(x => x.UserAgent).HasMaxLength(300);
            builder.Property(x => x.Referrer).HasMaxLength(300);
        }
    }

}
