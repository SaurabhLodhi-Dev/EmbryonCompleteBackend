using System;
using CleanArchitecture.Domain.Entities;
using Xunit;

namespace CleanArchitecture.UnitTests.Domain
{
    public class PageVisitTests
    {
        [Fact]
        public void Should_Set_Properties()
        {
            var id = Guid.NewGuid();

            var pv = new PageVisit
            {
                Id = id,
                PageUrl = "/home",
                IpAddress = "1.2.3.4",
                UserAgent = "Mozilla",
                Referrer = "google.com"
            };

            Assert.Equal(id, pv.Id);
            Assert.Equal("/home", pv.PageUrl);
            Assert.Equal("1.2.3.4", pv.IpAddress);
            Assert.Equal("Mozilla", pv.UserAgent);
            Assert.Equal("google.com", pv.Referrer);
        }
    }
}
