using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Consumers;
using Moggles.Domain;
using Moggles.UnitTests.Helpers;
using MassTransit.TestFramework;
using MogglesContracts;

namespace Moggles.UnitTests.ShowDeployedTogglesTests
{
    [TestClass]
    public class ShowDeployedTogglesTests
    {
        private IRepository<Application> _appRepository;
        private FeatureToggleDeployStatusConsumer _sut;

        [TestInitialize]
        public void BeforeTest()
        {
            _appRepository = new InMemoryApplicationRepository();
        }

        [TestMethod]
        public async Task FeatureToggleShowDeployedEnvironmentIsUpdatedCorrectly()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            app.AddFeatureToggle("TestToggle-1", "TestNotes-1");
            app.AddFeatureToggle("TestToggle-2", "TestNotes-2");

            await _appRepository.AddAsync(app);

            FeatureToggleTestHelper.UpdateFeatureToggleDeployedStatus(app, "TestToggle-1", "DEV", true);

            var context = new TestConsumeContext<RegisteredTogglesUpdate>(new RegisteredTogglesUpdate
            {
                Environment = "dev",
                AppName = "testapp",
                FeatureToggles = new []{ "TestToggle-2" }
            });

            //act
            _sut = new FeatureToggleDeployStatusConsumer(_appRepository);
            await _sut.Consume(context);

            //assert
            var updatedApp = await _appRepository.FindByIdAsync(app.Id);
            FeatureToggleTestHelper.GetFeatureToggleDeployedStatus(updatedApp, "TestToggle-1", "DEV").Should().BeFalse();
            FeatureToggleTestHelper.GetFeatureToggleDeployedStatus(updatedApp, "TestToggle-2", "DEV").Should().BeTrue();
        }
    }
}
