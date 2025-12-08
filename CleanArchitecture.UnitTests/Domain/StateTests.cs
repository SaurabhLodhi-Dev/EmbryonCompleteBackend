using System;
using CleanArchitecture.Domain.Entities;
using Xunit;

namespace CleanArchitecture.UnitTests.Domain
{
    public class StateTests
    {
        [Fact]
        public void Should_Set_Properties()
        {
            var id = Guid.NewGuid();
            var countryId = Guid.NewGuid();

            var s = new State
            {
                Id = id,
                CountryId = countryId,
                Name = "Madhya Pradesh"
            };

            Assert.Equal(id, s.Id);
            Assert.Equal(countryId, s.CountryId);
            Assert.Equal("Madhya Pradesh", s.Name);
        }
    }
}
