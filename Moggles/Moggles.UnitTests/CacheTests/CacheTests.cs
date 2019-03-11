using System;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Controllers;
using Moggles.Data;
using Moggles.Models;
using Moq;

namespace Moggles.UnitTests.CacheTests
{
    [TestClass]
    public class CacheTests
    {
        public TestContext TestContext { get; set; }
        private static TogglesContext _context;

        [TestInitialize]
        public void BeforeTest()
        {
            _context = Fixture.GetTogglesContext(TestContext.TestName);
        }

        [TestCleanup]
        public void AfterTest()
        {
            //Different scopes sharing the same Incrementing Key

            //EnsureDeleted does not reset "identity" columns for InMemory database provider
            //Do not provide id's unless required,failing to do so will result in "failed to track .." error
            //see https://github.com/aspnet/EntityFrameworkCore/issues/4096 and https://github.com/aspnet/EntityFrameworkCore/issues/6872

            _context.Database.EnsureDeleted();
        }

        [TestMethod]
        public void RefreshCache_ReturnBadRequestResult_WhenModelStateIsInvalid()
        {
            //arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(x => x.GetService(typeof(IBus))).Returns(new Mock<IBus>().Object);
            var controller = new CacheRefreshController(_context, new Mock<IConfiguration>().Object, mockServiceProvider.Object);

            controller.ModelState.AddModelError("error", "some error");
            
            //act
            var result = controller.RefreshCache(new RefreshCacheModel());
            
            //assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBe(null);
        }

    }
}
