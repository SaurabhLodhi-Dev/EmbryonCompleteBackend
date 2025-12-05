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
    public class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.ToTable("Countries");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).HasMaxLength(150);
            builder.Property(x => x.IsoCode).HasMaxLength(2);
            builder.Property(x => x.PhoneCode).HasMaxLength(10);

            builder.HasMany(x => x.States)
                   .WithOne(x => x.Country)
                   .HasForeignKey(x => x.CountryId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
