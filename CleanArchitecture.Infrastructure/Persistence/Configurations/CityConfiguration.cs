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
    public class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.ToTable("Cities");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).HasMaxLength(150);

            builder.HasOne(x => x.State)
                   .WithMany(s => s.Cities)
                   .HasForeignKey(x => x.StateId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
