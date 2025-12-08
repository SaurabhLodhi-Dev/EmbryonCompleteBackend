using System;
using CleanArchitecture.Domain.Common;
using Xunit;

namespace CleanArchitecture.UnitTests.Domain
{
    public class BaseEntityTests
    {
        private class TestEntity : BaseEntity { }

        [Fact]
        public void NewEntity_ShouldHaveNonEmptyId_And_CreatedAtSet()
        {
            // Arrange & Act
            var entity = new TestEntity();

            // Assert
            Assert.NotEqual(Guid.Empty, entity.Id);
            Assert.True(entity.CreatedAt <= DateTime.UtcNow);
        }

        [Fact]
        public void CanSetUpdatedAt_And_DeletedAt()
        {
            // Arrange
            var entity = new TestEntity();
            var now = DateTime.UtcNow;

            // Act
            entity.UpdatedAt = now;
            entity.DeletedAt = now;

            // Assert
            Assert.Equal(now, entity.UpdatedAt);
            Assert.Equal(now, entity.DeletedAt);
        }
    }
}
