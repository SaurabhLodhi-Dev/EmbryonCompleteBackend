using System;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CleanArchitecture.UnitTests.Infrastructure
{
    public class NotificationRepositoryTests
    {
        private ApplicationDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldPersistNotification()
        {
            using var db = CreateDb();
            var repo = new NotificationRepository(db);

            var n = new Notification
            {
                Type = "email",
                ToAddress = "test@test.com",
                Status = "pending"
            };

            var saved = await repo.AddAsync(n);

            Assert.NotEqual(Guid.Empty, saved.Id);
            Assert.Single(db.Notifications);
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyExistingNotification()
        {
            using var db = CreateDb();
            var repo = new NotificationRepository(db);

            var n = new Notification { Status = "pending" };
            db.Notifications.Add(n);
            await db.SaveChangesAsync();

            // modify
            n.Status = "sent";

            await repo.UpdateAsync(n);

            var updated = await db.Notifications.FirstAsync();
            Assert.Equal("sent", updated.Status);
        }
    }
}
