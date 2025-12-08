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
    public class ContactSubmissionRepositoryTests
    {
        private ApplicationDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldPersistEntity()
        {
            // Arrange
            using var db = CreateDb();
            var repo = new ContactSubmissionRepository(db);

            var entity = new ContactSubmission
            {
                FirstName = "John",
                Email = "john@example.com"
            };

            // Act
            var saved = await repo.AddAsync(entity);

            // Assert
            Assert.NotEqual(Guid.Empty, saved.Id);
            Assert.Equal("John", saved.FirstName);
            Assert.Single(db.ContactSubmissions);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnDescendingByCreatedAt()
        {
            using var db = CreateDb();
            var repo = new ContactSubmissionRepository(db);

            db.ContactSubmissions.Add(new ContactSubmission { FirstName = "Old", CreatedAt = DateTime.UtcNow.AddHours(-1) });
            db.ContactSubmissions.Add(new ContactSubmission { FirstName = "New", CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();

            // Act
            var list = await repo.GetAllAsync();

            // Assert
            Assert.Equal("New", list.First().FirstName);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnEntity()
        {
            using var db = CreateDb();
            var repo = new ContactSubmissionRepository(db);

            var entity = new ContactSubmission { FirstName = "Test" };
            db.ContactSubmissions.Add(entity);
            await db.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdAsync(entity.Id);

            // Assert
            Assert.Equal(entity.Id, found.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrow_WhenNotFound()
        {
            using var db = CreateDb();
            var repo = new ContactSubmissionRepository(db);

            var invalidId = Guid.NewGuid();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.GetByIdAsync(invalidId));
        }
    }
}
