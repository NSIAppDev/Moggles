using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Controllers;
using Moggles.Domain;
using Moq;

namespace Moggles.UnitTests.FeatureTogglesTests
{
    [TestClass]
    public class RemoveFeatureToggleTests
    {
        private IRepository<Application> _appRepository;
        private IHttpContextAccessor _httpContextAccessor;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        [TestInitialize]
        public void BeforeTest()
        {
            _appRepository = new InMemoryApplicationRepository();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContextAccessor.Setup(x => x.HttpContext.User.Identity.Name).Returns("bla");
            _httpContextAccessor = _mockHttpContextAccessor.Object;
        }

        [TestMethod]
        public async Task RemoveFeatureToggle_FeatureToggleIsDeleted()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false, "username");
            app.AddFeatureToggle("t1", "", "username");
            var theToggle = app.FeatureToggles.Single();
            await _appRepository.AddAsync(app);

            var controller = new FeatureTogglesController(_appRepository, _httpContextAccessor);

            //act
            var result = await controller.RemoveFeatureToggle(theToggle.Id, app.Id);

            //assert
            result.Should().BeOfType<OkResult>();
            (await _appRepository.FindByIdAsync(app.Id)).FeatureToggles.Count.Should().Be(0);
        }
    }
}