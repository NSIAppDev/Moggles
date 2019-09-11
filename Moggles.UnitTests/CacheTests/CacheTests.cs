using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Controllers;
using Moggles.Models;
using Moq;
using System;
using System.Threading.Tasks;
using Moggles.Domain;

namespace Moggles.UnitTests.CacheTests
{
    [TestClass]
    public class CacheTests
    {
        private Mock<IRepository<Application>> _appRepositoryMock;

        [TestInitialize]
        public void BeforeTest()
        {
            _appRepositoryMock = new Mock<IRepository<Application>>();
        }

        [TestCleanup]
        public void AfterTest()
        {
        }

        [TestMethod]
        public async Task RefreshCache_ReturnBadRequestResult_WhenModelStateIsInvalid()
        {
            //arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(x => x.GetService(typeof(IBus))).Returns(new Mock<IBus>().Object);
            var controller = new CacheRefreshController(_appRepositoryMock.Object, new Mock<IConfiguration>().Object, mockServiceProvider.Object);

            controller.ModelState.AddModelError("error", "some error");
            
            //act
            var result = await controller.RefreshCache(new RefreshCacheModel());
            
            //assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBe(null);
        }

        //TODO: add 2 or 3 more tests for the entire functionality

    }
}
