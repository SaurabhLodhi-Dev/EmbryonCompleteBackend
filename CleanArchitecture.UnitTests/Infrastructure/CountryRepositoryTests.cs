using System;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CleanArchitecture.UnitTests.Infrastructure
{
    public class CountryRepositoryTests
    {
        private ApplicationDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAlphabetical()
        {
            using var db = CreateDb();
            var repo = new CountryRepository(db);

            db.Countries.Add(new Country { Name = "USA" });
            db.Countries.Add(new Country { Name = "India" });
            db.Countries.Add(new Country { Name = "Brazil" });
            await db.SaveChangesAsync();

            // Act
            var list = await repo.GetAllAsync();

            // Assert
            var names = list.Select(x => x.Name).ToList();
            Assert.Equal(new[] { "Brazil", "India", "USA" }, names);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectCountry()
        {
            using var db = CreateDb();
            var repo = new CountryRepository(db);

            var id = Guid.NewGuid();
            db.Countries.Add(new Country { Id = id, Name = "India" });
            await db.SaveChangesAsync();

            var result = await repo.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal("India", result!.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            using var db = CreateDb();
            var repo = new CountryRepository(db);

            var result = await repo.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }
    }
}
