using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Persistence.Configurations
{
    public class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
    {
        public void Configure(EntityTypeBuilder<AppSetting> builder)
        {
            builder.ToTable("AppSettings");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.KeyName)
                   .HasMaxLength(150);

            builder.Property(x => x.Value)
                   .HasColumnType("NVARCHAR(MAX)");

            builder.Property(x => x.Description)
                   .HasMaxLength(500);

            builder.Property(x => x.AdditionalData)
                   .HasColumnType("NVARCHAR(MAX)");
        }
    }

}
