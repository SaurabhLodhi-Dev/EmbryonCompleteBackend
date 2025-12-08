
using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //public DbSet<Product> Products { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Product>().HasKey(p => p.Id);

        //    // Seed data
        //    modelBuilder.Entity<Product>().HasData(
        //        new Product("Test Product 1", "Description 1", 19.99m) { Id = 1 },
        //        new Product("Test Product 2", "Description 2", 29.99m) { Id = 2 }
        //    );
        //}
        // DbSets (Domain entities)
        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<Country> Countries { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }

        public DbSet<ContactSubmission> ContactSubmissions { get; set; }



   
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PageVisit> PageVisits { get; set; }
        public DbSet<Webhook> Webhooks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Register IEntityTypeConfiguration<> classes here (Persistence layer)
            // e.g. modelBuilder.ApplyConfiguration(new CountryConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
