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
    public class ContactSubmissionConfiguration : IEntityTypeConfiguration<ContactSubmission>
    {
        public void Configure(EntityTypeBuilder<ContactSubmission> builder)
        {
            builder.ToTable("ContactSubmissions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FirstName).HasMaxLength(150);
            builder.Property(x => x.LastName).HasMaxLength(150);
            builder.Property(x => x.Email).HasMaxLength(200);
            builder.Property(x => x.Phone).HasMaxLength(50);
            builder.Property(x => x.PhoneCountryCode).HasMaxLength(10);

            builder.Property(x => x.State).HasMaxLength(150);
            builder.Property(x => x.City).HasMaxLength(150);

            builder.Property(x => x.Subject).HasMaxLength(255);
            builder.Property(x => x.Message).HasColumnType("NVARCHAR(MAX)");

            builder.HasOne(x => x.Country)
                   .WithMany()
                   .HasForeignKey(x => x.CountryId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
