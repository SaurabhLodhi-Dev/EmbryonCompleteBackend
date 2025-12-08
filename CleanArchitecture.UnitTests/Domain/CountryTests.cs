using System;
using CleanArchitecture.Domain.Entities;
using Xunit;

namespace CleanArchitecture.UnitTests.Domain
{
    public class CountryTests
    {
        [Fact]
        public void Should_Set_Properties()
        {
            var id = Guid.NewGuid();

            var c = new Country
            {
                Id = id,
                Name = "India",
                IsoCode = "IN",
                PhoneCode = "+91"
            };

            Assert.Equal(id, c.Id);
            Assert.Equal("India", c.Name);
            Assert.Equal("IN", c.IsoCode);
            Assert.Equal("+91", c.PhoneCode);
        }
    }
}
