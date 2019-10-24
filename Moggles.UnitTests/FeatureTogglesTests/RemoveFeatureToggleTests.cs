using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Controllers;
using Moggles.Domain;

namespace Moggles.UnitTests.FeatureTogglesTests
{
    [TestClass]
    public class RemoveFeatureToggleTests
    {
        private IRepository<Application> _appRepository;

        [TestInitialize]
        public void BeforeTest()
        {
            _appRepository = new InMemoryApplicationRepository();
        }

        [TestMethod]
        public async Task RemoveFeatureToggle_FeatureToggleIsDeleted()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false, "username");
            app.AddFeatureToggle("t1", "", "username");
            var theToggle = app.FeatureToggles.Single();
            await _appRepository.AddAsync(app);

            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.RemoveFeatureToggle(theToggle.Id, app.Id);

            //assert
            result.Should().BeOfType<OkResult>();
            (await _appRepository.FindByIdAsync(app.Id)).FeatureToggles.Count.Should().Be(0);
        }
    }
}