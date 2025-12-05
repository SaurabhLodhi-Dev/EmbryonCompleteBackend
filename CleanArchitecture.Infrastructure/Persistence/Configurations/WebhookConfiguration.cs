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
    public class WebhookConfiguration : IEntityTypeConfiguration<Webhook>
    {
        public void Configure(EntityTypeBuilder<Webhook> builder)
        {
            builder.ToTable("Webhooks");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Url).HasMaxLength(500);
            builder.Property(x => x.Event).HasMaxLength(100);
            builder.Property(x => x.SecretKey).HasMaxLength(255);
        }
    }


}
