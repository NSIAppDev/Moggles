using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Controllers;
using Moggles.Domain;
using Moggles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moggles.UnitTests.FeatureTogglesTests
{
    [TestClass]
    public class FeatureTogglesTests
    {
        private InMemoryRepository _appRepository;

        [TestInitialize]
        public void BeforeTest()
        {
            _appRepository = new InMemoryRepository();
        }

        [TestMethod]
        public async Task GetToggles_ReturnsAList_WithAllTheToggles_ForTheGivenApplication()
        {
            //arrange
            var app = Application.Create("BCC", "dev", false);
            app.AddFeatureToggle("TestToggle", "TestNotes", true);
            app.AddFeatureToggle("TestToggle2", "TestNotes2");

            await _appRepository.AddAsync(app);

            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.GetToggles(app.Id) as OkObjectResult;
            var list = result.Value as IEnumerable<FeatureToggleViewModel>;

            //assert
            list.Count().Should().Be(2);
            var toggle = list.FirstOrDefault(t => t.ToggleName == "TestToggle");
            toggle.Notes.Should().Be("TestNotes");
            toggle.CreatedDate.Should().BeCloseTo(DateTime.Now, 200);
            toggle.UserAccepted.Should().Be(false);
            toggle.IsPermanent.Should().Be(true);
        }

        //[TestMethod]
        //public async Task GetToggles_ReturnsAList_WithAllTheToggles_AndTheStatusesOfThoseToggles_ForTheGivenApplication()
        //{
        //    //arrange
        //    var app = new Application { AppName = "BCC", Id = 1 };

        //    var featureToggle = new FeatureToggle { Application = app, ApplicationId = app.Id };

        //    var env = new DeployEnvironment { Application = app, EnvName = "DEV" };

        //    var devFeatureStatus = new FeatureToggleStatus { Id = 1, Environment = env, FeatureToggle = featureToggle, FeatureToggleId = app.Id, Enabled = true, IsDeployed = true };
        //    var qaFeatureStatus = new FeatureToggleStatus { Id = 2, Environment = env, FeatureToggle = featureToggle, FeatureToggleId = app.Id, Enabled = false, IsDeployed = false };

        //    _context.FeatureToggles.Add(featureToggle);
        //    _context.Applications.Add(app);
        //    _context.DeployEnvironments.Add(env);
        //    _context.FeatureToggleStatuses.AddRange(devFeatureStatus, qaFeatureStatus);

        //    _context.SaveChanges();

        //    var controller = new FeatureTogglesController(_context);

        //    //act
        //    var result = controller.GetToggles(app.Id) as OkObjectResult;
        //    result.Should().NotBeNull();
        //    var list = result.Value as IEnumerable<FeatureToggleViewModel>;

        //    //assert
        //    list.First().Statuses.Count.Should().Be(2);
        //}

        [TestMethod]
        public async Task GetEnvironments_ReturnsAList_WithAllTheEnvironments_ForTheGivenApplication()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);

            var expectedEnvNames = new List<string>
            {
                "DEV",
                "QA",
                "SBX",
                "TRN",
                "LIVE"
            };

            app.AddDeployEnvironment("QA", false);
            app.AddDeployEnvironment("SBX", false);
            app.AddDeployEnvironment("TRN", false);
            app.AddDeployEnvironment("LIVE", false);
            await _appRepository.AddAsync(app);

            //act
            var controller = new FeatureTogglesController(_appRepository);
            var results = await controller.GetEnvironments(app.Id) as OkObjectResult;

            //assert
            results.Value.Should().BeEquivalentTo(expectedEnvNames);
        }

        [TestMethod]
        public async Task GetEnvironments_ReturnsAList_WithAllTheDistinctEnvironments_ForTheGivenApplication()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);

            app.AddDeployEnvironment("QA", false);
            app.AddDeployEnvironment("QA", false);

            var expectedEnvNames = new List<string> { "DEV", "QA" };

            await _appRepository.AddAsync(app);
            var controller = new FeatureTogglesController(_appRepository);

            //act
            var results = await controller.GetEnvironments(app.Id) as OkObjectResult;

            //assert
            results.Value.Should().BeEquivalentTo(expectedEnvNames);
        }

        [TestMethod]
        public async Task Updates_CanBeMade_ToExistingFeatureToggle()
        {
            //arrange
            var app = Application.Create("test", "DEV", false);
            app.AddFeatureToggle("TestToggle", "FirstNote", false);
            await _appRepository.AddAsync(app);

            var toggle = app.FeatureToggles.Single();
            var updatedValue = new FeatureToggleUpdateModel { ApplicationId = app.Id, Id = toggle.Id, FeatureToggleName = "UpdatedFeatureToggleName", Notes = "Update", UserAccepted = true, Statuses = new List<FeatureToggleStatusUpdateModel>(), IsPermanent = true };

            var controller = new FeatureTogglesController(_appRepository);

            //act
            await controller.Update(updatedValue);

            //assert
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            savedApp.FeatureToggles.FirstOrDefault().ToggleName.Should().Be("UpdatedFeatureToggleName");
            savedApp.FeatureToggles.FirstOrDefault().Notes.Should().Be("Update");
            savedApp.FeatureToggles.FirstOrDefault().UserAccepted.Should().BeTrue();
            savedApp.FeatureToggles.FirstOrDefault().IsPermanent.Should().BeTrue();
        }

        [TestMethod]
        public async Task AddFeatureToggle_ReturnBadRequestResult_WhenModelStateIsInvalid()
        {
            //arrange
            var controller = new FeatureTogglesController(_appRepository);
            controller.ModelState.AddModelError("error", "some error");

            //act
            var result = await controller.AddFeatureToggle(new AddFeatureToggleModel());

            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        }

        [TestMethod]
        public async Task AddFeatureToggle_ReturnBadRequestResult_WhenFeatureAlreadyExists()
        {
            //arrange
            var app = Application.Create("bcc", "dev", false);
            app.AddFeatureToggle("TestToggle", string.Empty);

            var newFeatureToggle = new AddFeatureToggleModel { ApplicationId = app.Id, FeatureToggleName = "TestToggle" };

            await _appRepository.AddAsync(app);

            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.AddFeatureToggle(newFeatureToggle);

            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        }

        [TestMethod]
        public async Task AddFeatureToggle_ReturnBadRequestResult_WhenApplicationNotSpecified()
        {
            //arrange
            var newFeatureToggle = new AddFeatureToggleModel { FeatureToggleName = "TestToggle" };

            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.AddFeatureToggle(newFeatureToggle);

            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        }

        [TestMethod]
        public async Task AddFeatureToggle_FeatureToggleIsCreated()
        {
            //arrange
            var app = Application.Create("tst", "dev", false);
            await _appRepository.AddAsync(app);
            var newFeatureToggle = new AddFeatureToggleModel { ApplicationId = app.Id, FeatureToggleName = "TestToggle" };

            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.AddFeatureToggle(newFeatureToggle);

            //assert
            result.Should().BeOfType<OkResult>();
            var toggle = _appRepository.Applications.FirstOrDefault(a => a.Id == app.Id).FeatureToggles.FirstOrDefault();
            toggle.Should().NotBeNull();
            toggle.ToggleName.Should().Be("TestToggle");
        }

        [TestMethod]
        public async Task AddFeatureToggle_FeatureToggleStatus_IsCreated_ForEveryEnvironment()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            app.AddDeployEnvironment("QA", false);

            var newFeatureToggle = new AddFeatureToggleModel { ApplicationId = app.Id, FeatureToggleName = "TestToggle" };

            await _appRepository.AddAsync(app);

            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.AddFeatureToggle(newFeatureToggle);

            //assert
            result.Should().BeOfType<OkResult>();
            (await _appRepository.FindByIdAsync(app.Id)).FeatureToggles.FirstOrDefault().FeatureToggleStatuses.Count.Should().Be(2);
        }

        [TestMethod]
        public async Task RemoveFeatureToggle_FeatureToggleIsDeleted()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            app.AddFeatureToggle("t1", "");
            var theToggle = app.FeatureToggles.Single();
            await _appRepository.AddAsync(app);

            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.RemoveFeatureToggle(theToggle.Id, app.Id);

            //assert
            result.Should().BeOfType<OkResult>();
            (await _appRepository.FindByIdAsync(app.Id)).FeatureToggles.Count.Should().Be(0);
        }

        [TestMethod]
        public async Task AddEnvironment_EnvironmentIsBeingCreated()
        {
            //arrange
            var app = Application.Create("tst", "DEV", false);
            await _appRepository.AddAsync(app);
            var newEnvironment = new AddEnvironmentModel { ApplicationId = app.Id, EnvName = "QA" };

            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.AddEnvironment(newEnvironment);

            //assert
            result.Should().BeOfType<OkResult>();
            (await _appRepository.FindByIdAsync(app.Id)).DeploymentEnvironments.Count().Should().Be(2);
        }

        [TestMethod]
        public async Task AddEnvironment_ReturnBadRequestResult_WhenModelStateIsInvalid()
        {
            //arrange
            var controller = new FeatureTogglesController(_appRepository);
            controller.ModelState.AddModelError("error", "some error");

            //act
            var result = await controller.AddEnvironment(new AddEnvironmentModel());

            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        }

        [TestMethod]
        public async Task AddEnvironment_ReturnBadRequestResult_WhenEnvironmentAlreadyExists()
        {
            //arrange
            var app = Application.Create("tst", "dev", false);
            await _appRepository.AddAsync(app);

            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.AddEnvironment(new AddEnvironmentModel { ApplicationId = app.Id, EnvName = "dev" });

            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        }

        [TestMethod]
        public async Task AddEnvironment_EveryExistingFeatureToggle_IsMarkedAs_Off_ForTheNewEnvironment()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            app.AddFeatureToggle("t1", string.Empty);
            app.AddFeatureToggle("t2", string.Empty);
            await _appRepository.AddAsync(app);

            var newEnvironment = new AddEnvironmentModel { ApplicationId = app.Id, EnvName = "QA" };

            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.AddEnvironment(newEnvironment);

            //assert
            result.Should().BeOfType<OkResult>();
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            savedApp.GetFeatureToggleStatus("t1", "QA").Enabled.Should().BeFalse();
            savedApp.GetFeatureToggleStatus("t2", "QA").Enabled.Should().BeFalse();
        }

        [TestMethod]
        public async Task AddEnvironment_EveryExistingFeatureToggle_IsMarkedAs_On_ForTheNewEnvironment_WhenTheDefaultValueForTheToggleIsTrue()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            var newEnvironment = new AddEnvironmentModel { ApplicationId = app.Id, EnvName = "QA", DefaultToggleValue = true };
            app.AddFeatureToggle("t1", string.Empty);
            app.AddFeatureToggle("t2", string.Empty);

            await _appRepository.AddAsync(app);
            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.AddEnvironment(newEnvironment);

            //assert
            result.Should().BeOfType<OkResult>();
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            savedApp.GetFeatureToggleStatus("t1", "QA").Enabled.Should().BeTrue();
            savedApp.GetFeatureToggleStatus("t2", "QA").Enabled.Should().BeTrue();
        }

        //[TestMethod]
        //public async Task GetApplicationFeatureToggles_ReturnsExistingFeaturesStatuses_ForTheGivenApplicationNameAndEnvironmentName()
        //{
        //    //arrange
        //    var app = new Application { Id = 1, AppName = "TestApp" };
        //    var environment = new DeployEnvironment
        //    { Application = app, ApplicationId = app.Id, EnvName = "TestEnv" };
        //    var firstFeatureStatus = new FeatureToggleStatus { Enabled = false, Id = 1, IsDeployed = false, Environment = environment, EnvironmentId = environment.Id };
        //    var secondFeatureStatus = new FeatureToggleStatus { Enabled = false, Id = 2, IsDeployed = false, Environment = environment, EnvironmentId = environment.Id };
        //    var thirdFeatureStatus = new FeatureToggleStatus { Enabled = false, Id = 3, IsDeployed = false, Environment = environment, EnvironmentId = environment.Id };
        //    var discardedFeature = new FeatureToggleStatus { Enabled = false, Id = 5, IsDeployed = false, Environment = new DeployEnvironment { EnvName = "AnotherEnv" }, EnvironmentId = 67 };
        //    var feature = new FeatureToggle { Id = 1, Application = app, ApplicationId = app.Id, FeatureToggleStatuses = new List<FeatureToggleStatus> { firstFeatureStatus, secondFeatureStatus, thirdFeatureStatus }, ToggleName = "Test" };

        //    _context.FeatureToggleStatuses.AddRange(firstFeatureStatus, secondFeatureStatus, thirdFeatureStatus, discardedFeature);
        //    _context.Applications.Add(app);
        //    _context.DeployEnvironments.Add(environment);
        //    _context.FeatureToggles.Add(feature);
        //    _context.SaveChanges();

        //    var controller = new FeatureTogglesController(_context);

        //    //act
        //    var result = controller.GetApplicationFeatureToggles(app.AppName, environment.EnvName) as OkObjectResult;
        //    var okObjectResult = result.Value as IEnumerable<ApplicationFeatureToggleViewModel>;

        //    //assert
        //    okObjectResult.ToList().Count.Should().Be(3);
        //    okObjectResult.First().FeatureToggleName.Should().Be(firstFeatureStatus.FeatureToggle.ToggleName);
        //    okObjectResult.First().IsEnabled.Should().Be(firstFeatureStatus.Enabled);
        //}

        //[TestMethod]
        //public async Task GetApplicationFeatureToggleValue_ReturnsTheStatus_OfTheGivenFeatureToggle()
        //{
        //    //arrange
        //    var app = new Application { Id = 1, AppName = "TestApp" };
        //    var environment = new DeployEnvironment
        //    { Id = 2, Application = app, ApplicationId = app.Id, EnvName = "TestEnv" };
        //    var featureStatus = new FeatureToggleStatus { Enabled = true, Id = 1, IsDeployed = false, Environment = environment, EnvironmentId = environment.Id };

        //    var feature = new FeatureToggle { Id = 1, Application = app, ApplicationId = app.Id, FeatureToggleStatuses = new List<FeatureToggleStatus> { featureStatus }, ToggleName = "Test" };

        //    _context.FeatureToggleStatuses.Add(featureStatus);
        //    _context.Applications.Add(app);
        //    _context.DeployEnvironments.Add(environment);
        //    _context.FeatureToggles.Add(feature);
        //    _context.SaveChanges();

        //    var controller = new FeatureTogglesController(_context);

        //    //act
        //    var result = controller.GetApplicationFeatureToggleValue(app.AppName, environment.EnvName, feature.ToggleName) as OkObjectResult;
        //    var okObjectResult = result.Value as ApplicationFeatureToggleViewModel;

        //    //assert
        //    okObjectResult.FeatureToggleName.Should().Be(feature.ToggleName);
        //    okObjectResult.IsEnabled.Should().BeTrue();
        //}

        [TestMethod]
        public async Task CreateEnvironment_A_NewEnvironmentIsBeingStoredInDB()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            await _appRepository.AddAsync(app);
            var createdEnvironment = new AddEnvironmentModel { ApplicationId = app.Id, EnvName = "QA" };
            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.AddEnvironment(createdEnvironment);

            //assert
            result.Should().BeOfType<OkResult>();
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            savedApp.DeploymentEnvironments.FirstOrDefault(x => x.EnvName.Equals(createdEnvironment.EnvName)).Should().NotBeNull();
            savedApp.DeploymentEnvironments.FirstOrDefault(x => x.DefaultToggleValue == false).Should().NotBeNull();
        }

        [TestMethod]
        public async Task EditEnvironment_EnvironmentIsBeingModified()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            await _appRepository.AddAsync(app);

            var controller = new FeatureTogglesController(_appRepository);

            var updatedEnvironmentName = "QA";

            var updatedEnvironment = new UpdateEnvironmentModel
            {
                ApplicationId = app.Id,
                InitialEnvName = "DEV",
                NewEnvName = updatedEnvironmentName
            };

            //act
            var result = await controller.UpdateEnvironment(updatedEnvironment);

            //assert
            result.Should().BeOfType<OkResult>();
            (await _appRepository.FindByIdAsync(app.Id)).DeploymentEnvironments.First().EnvName.Should().Be(updatedEnvironmentName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Environment does not exist!")]
        public async Task EditEnvironment_EnvironmentIsModifiedWithInvalidID_ThrowsInvalidOperationException()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            await _appRepository.AddAsync(app);

            var controller = new FeatureTogglesController(_appRepository);

            var updatedEnvironmentName = "QA";

            var updatedEnvironment = new UpdateEnvironmentModel
            {
                ApplicationId = app.Id,
                InitialEnvName = "BLA",
                NewEnvName = updatedEnvironmentName
            };

            //act
            await controller.UpdateEnvironment(updatedEnvironment);

            //assert
            //throws InvalidOperationException
        }

        [TestMethod]
        public async Task DeleteEnvironment_EnvironmentIsDeleted_FeatureToggleStatusForThatEnvironmentIsDeletedForAllToggles()
        {
            //arrange
            var app = Application.Create("TestApp", "TestEnv", false);
            app.AddFeatureToggle("t1", "");
            app.AddFeatureToggle("t2", "");
            app.AddFeatureToggle("t3", "");
            await _appRepository.AddAsync(app);

            var controller = new FeatureTogglesController(_appRepository);

            var environmentToRemove = new DeleteEnvironmentModel
            {
                ApplicationId = app.Id,
                EnvName = "TestEnv"
            };

            //act
            var result = await controller.RemoveEnvironment(environmentToRemove);

            //assert
            result.Should().BeOfType<OkResult>();
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            savedApp.DeploymentEnvironments.Count().Should().Be(0);
            savedApp.FeatureToggles.Count(ft => ft.FeatureToggleStatuses.Count > 0).Should().Be(0);
        }

        [TestMethod]
        public async Task DeleteEnvironment_EnvironmentIsDeleted_FeatureTogglesAreNotDeleted()
        {
            //arrange
            var app = Application.Create("TestApp", "TestEnv", false);
            app.AddFeatureToggle("t1", "");
            app.AddFeatureToggle("t2", "");
            app.AddFeatureToggle("t3", "");
            await _appRepository.AddAsync(app);

            //  var featureToggle = new FeatureToggle { Id = 1 };

            var controller = new FeatureTogglesController(_appRepository);

            //var firstFeatureStatus = new FeatureToggleStatus { Enabled = false, Id = 1, FeatureToggle = featureToggle, IsDeployed = false, Environment = environment, EnvironmentId = environment.Id };
            //var secondFeatureStatus = new FeatureToggleStatus { Enabled = false, Id = 2, FeatureToggle = featureToggle, IsDeployed = false, Environment = environment, EnvironmentId = environment.Id };
            //var thirdFeatureStatus = new FeatureToggleStatus { Enabled = false, Id = 3, FeatureToggle = featureToggle, IsDeployed = false, Environment = environment, EnvironmentId = environment.Id };

            //featureToggle.FeatureToggleStatuses.AddRange(new List<FeatureToggleStatus>()
            //{
            //    firstFeatureStatus, secondFeatureStatus, thirdFeatureStatus
            //});

            //_context.FeatureToggles.Add(featureToggle);
            //_context.FeatureToggleStatuses.AddRange(firstFeatureStatus, secondFeatureStatus, thirdFeatureStatus);
            //_context.Applications.Add(app);
            //_context.DeployEnvironments.Add(environment);
            //_context.SaveChanges();

            var environmentToRemove = new DeleteEnvironmentModel
            {
                ApplicationId = app.Id,
                EnvName = "TestEnv"
            };
            //act
            var result = await controller.RemoveEnvironment(environmentToRemove);

            //assert
            result.Should().BeOfType<OkResult>();
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            savedApp.DeploymentEnvironments.Count().Should().Be(0);
            savedApp.FeatureToggles.Count().Should().Be(3);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Environment does not exist!")]
        public async Task DeleteEnvironment_EnvironmentIsDeletedWithInvalidID_ThrowsInvalidOperationException()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            await _appRepository.AddAsync(app);

            var controller = new FeatureTogglesController(_appRepository);

            var environmentToRemove = new DeleteEnvironmentModel
            {
                ApplicationId = app.Id,
                EnvName = "BLA"
            };

            //act
            await controller.RemoveEnvironment(environmentToRemove);

            //assert
            //throws InvalidOperationException
        }
    }
}
