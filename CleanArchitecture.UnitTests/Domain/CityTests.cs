using System;
using CleanArchitecture.Domain.Entities;
using Xunit;

namespace CleanArchitecture.UnitTests.Domain
{
    public class CityTests
    {
        [Fact]
        public void Should_Set_Properties()
        {
            var id = Guid.NewGuid();
            var stateId = Guid.NewGuid();

            var city = new City
            {
                Id = id,
                Name = "Indore",
                StateId = stateId
            };

            Assert.Equal(id, city.Id);
            Assert.Equal("Indore", city.Name);
            Assert.Equal(stateId, city.StateId);
        }
    }
}
