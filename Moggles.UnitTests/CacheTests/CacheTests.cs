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
        private IRepository<Application> _appRepository;
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<IBus> _busMock;

        [TestInitialize]
        public void BeforeTest()
        {
            _appRepository = new InMemoryApplicationRepository();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _busMock = new Mock<IBus>();
            _mockServiceProvider.Setup(x => x.GetService(typeof(IBus))).Returns(_busMock.Object);
        }

        [TestCleanup]
        public void AfterTest()
        {
        }

        [TestMethod]
        public async Task RefreshCache_ReturnBadRequestResult_WhenModelStateIsInvalid()
        {
            //arrange
            var controller = new CacheRefreshController(_appRepository, new Mock<IConfiguration>().Object, _mockServiceProvider.Object);
            controller.ModelState.AddModelError("error", "some error");

            //act
            var result = await controller.RefreshCache(new RefreshCacheModel());

            //assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBe(null);
        }

        [TestMethod]
        public async Task RefreshCache_ThrowsException_WhenProvidedInvalidAppId()
        {
            //arrange
            var app = Application.Create("tst", "dev", false);
            await _appRepository.AddAsync(app);
            var controller = new CacheRefreshController(_appRepository, new Mock<IConfiguration>().Object, _mockServiceProvider.Object);

            //act
            Func<Task> result = async () => await controller.RefreshCache(new RefreshCacheModel { ApplicationId = Guid.NewGuid() });

            //assert
            result.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public async Task RefreshCacheEventIsPublished_WithTheCorrectAppNameAndEnvironmentInformation()
        {
            //arrange
            var app = Application.Create("tst", "dev", false);
            await _appRepository.AddAsync(app);
           
            var controller = new CacheRefreshController(_appRepository, new Mock<IConfiguration>().Object, _mockServiceProvider.Object);

            //act
            await controller.RefreshCache(new RefreshCacheModel {ApplicationId = app.Id, EnvName = "DEV"});

            //assert
            _busMock.Verify(x => x.Publish(It.Is<NSTogglesContracts.RefreshTogglesCache>(e => e.ApplicationName == "tst" && e.Environment == "DEV"), default),
                Times.Once);
        }
    }
}
