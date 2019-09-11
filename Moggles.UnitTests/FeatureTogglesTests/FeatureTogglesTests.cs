using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Controllers;
using Moggles.Domain;
using Moggles.Models;
using Moq;

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
            var app = Application.Create("BCC","dev",false);
           // app.AddDeployEnvironment("dev", false);
           app.AddFeatureToggle("TestToggle","TestNotes",true);
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
            toggle.CreatedDate.Should().BeCloseTo(DateTime.Now,200);
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

        //[TestMethod]
        //public async Task GetEnvironments_ReturnsAList_WithAllTheDistinctEnvironments_ForTheGivenApplication()
        //{
        //    //arrange
        //    var app = new Application { Id = 1, AppName = "TestApp" };
        //    var dev = new DeployEnvironment { Application = app, ApplicationId = app.Id, Id = 10, EnvName = "DEV" };
        //    var qa = new DeployEnvironment { Application = app, ApplicationId = app.Id, Id = 11, EnvName = "QA" };
        //    var duplicateQa = new DeployEnvironment { Application = app, ApplicationId = app.Id, Id = 12, EnvName = "QA" };

        //    var expectedEnvNames = new List<string> { dev.EnvName, qa.EnvName };

        //    _context.DeployEnvironments.AddRange(dev, qa, duplicateQa);
        //    _context.SaveChanges();
        //    var controller = new FeatureTogglesController(_context);

        //    //act
        //    var results = controller.GetEnvironments(app.Id) as OkObjectResult;

        //    //assert
        //    results.Value.Should().BeEquivalentTo(expectedEnvNames);
        //}

        //[TestMethod]
        //public async Task Updates_CanBeMade_ToExistingFeatureToggle()
        //{
        //    //arrange
        //    var app = new Application { Id = 1, AppName = "TestApp" };
        //    var existingValue = new FeatureToggle { Id = 1, Application = app, ApplicationId = app.Id, ToggleName = "TestToggle", FeatureToggleStatuses = new List<FeatureToggleStatus>(), Notes = "FirstNote", IsPermanent = false };
        //    var updatedValue = new FeatureToggleUpdateModel { Id = 1, FeatureToggleName = "UpdatedFeatureToggleName", Notes = "Update", UserAccepted = true, Statuses = new List<FeatureToggleStatusUpdateModel>(), IsPermanent = true };

        //    _context.FeatureToggles.Add(existingValue);
        //    _context.SaveChanges();
        //    var controller = new FeatureTogglesController(_context);

        //    //act
        //    var result = controller.Update(updatedValue) as OkObjectResult;

        //    //assert
        //    _context.FeatureToggles.FirstOrDefault().ToggleName.Should().Be("UpdatedFeatureToggleName");
        //    _context.FeatureToggles.FirstOrDefault().Notes.Should().Be(updatedValue.Notes);
        //    _context.FeatureToggles.FirstOrDefault().UserAccepted.Should().BeTrue();
        //    _context.FeatureToggles.FirstOrDefault().IsPermanent.Should().BeTrue();
        //}

        //[TestMethod]
        //public async Task AddFeatureToggle_ReturnBadRequestResult_WhenModelStateIsInvalid()
        //{
        //    //arrange
        //    var controller = new FeatureTogglesController(_context);
        //    controller.ModelState.AddModelError("error", "some error");

        //    //act
        //    var result = controller.AddFeatureToggle(new AddFeatureToggleModel());

        //    //assert
        //    result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        //}

        //[TestMethod]
        //public async Task AddFeatureToggle_ReturnBadRequestResult_WhenFeatureAlreadyExists()
        //{
        //    //arrange
        //    var app = new Application { Id = 1, AppName = "BCC" };
        //    var existingFeatureToggle = new FeatureToggle { Application = app, Id = 2, ApplicationId = app.Id, ToggleName = "TestToggle" };
        //    var newFeatureToggle = new AddFeatureToggleModel { ApplicationId = app.Id, FeatureToggleName = "TestToggle" };

        //    _context.FeatureToggles.Add(existingFeatureToggle);
        //    _context.SaveChanges();

        //    var controller = new FeatureTogglesController(_context);

        //    //act
        //    var result = controller.AddFeatureToggle(newFeatureToggle);

        //    //assert
        //    result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        //}

        //[TestMethod]
        //public async Task AddFeatureToggle_ReturnBadRequestResult_WhenApplicationNotSpecified()
        //{
        //    //arrange
        //    var newFeatureToggle = new AddFeatureToggleModel { FeatureToggleName = "TestToggle" };

        //    var controller = new FeatureTogglesController(_context);

        //    //act
        //    var result = controller.AddFeatureToggle(newFeatureToggle);

        //    //assert
        //    result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        //}

        [TestMethod]
        public async Task AddFeatureToggle_FeatureToggleIsCreated()
        {
            //arrange
            var app = new Application { Id = Guid.NewGuid(), AppName = "BCC" };
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

        //[TestMethod]
        //public async Task AddFeatureToggle_FeatureToggleStatus_IsCreated_ForEveryEnvironment()
        //{
        //    //arrange
        //    var app = new Application { Id = 1, AppName = "BCC" };
        //    var newFeatureToggle = new AddFeatureToggleModel { ApplicationId = app.Id, FeatureToggleName = "TestToggle" };
        //    var devEnv = new DeployEnvironment { Application = app, Id = 1, ApplicationId = app.Id, EnvName = "DEV" };
        //    var qaEnv = new DeployEnvironment { Application = app, Id = 2, ApplicationId = app.Id, EnvName = "QA" };


        //    _context.DeployEnvironments.AddRange(devEnv, qaEnv);
        //    _context.Applications.Add(app);
        //    _context.SaveChanges();

        //    var controller = new FeatureTogglesController(_context);

        //    //act
        //    var result = controller.AddFeatureToggle(newFeatureToggle);

        //    //assert
        //    result.Should().BeOfType<OkResult>();
        //    _context.FeatureToggleStatuses.Count().Should().Be(2);
        //}

        //[TestMethod]
        //public async Task RemoveFeatureToggle_FeatureToggleIsDeleted()
        //{
        //    //arrange
        //    var data = new List<FeatureToggle>
        //    {
        //        new FeatureToggle{Id = 1, ToggleName = "Test1"},
        //        new FeatureToggle{ Id = 2, ToggleName = "Test2"}
        //    };

        //    var mockSet = new Mock<DbSet<FeatureToggle>>();
        //    mockSet.As<IQueryable<FeatureToggle>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

        //    var options = new DbContextOptionsBuilder<TogglesContext>()
        //        .Options;

        //    var mockContext = new Mock<TogglesContext>(options);
        //    mockContext
        //        .Setup(m => m.FeatureToggles.Remove(It.IsAny<FeatureToggle>())).Callback<FeatureToggle>((entity) => data.Remove(entity));
        //    var controller = new FeatureTogglesController(mockContext.Object);

        //    //act
        //    var result = controller.RemoveFeatureToggle(1);

        //    //assert
        //    result.Should().BeOfType<OkResult>();
        //    mockContext.Verify(s => s.FeatureToggles.Remove(It.IsAny<FeatureToggle>()), Times.Once);
        //    mockContext.Verify(s => s.SaveChanges(), Times.Once);
        //}

        //[TestMethod]
        //public async Task AddEnvironment_EnvironmentIsBeingCreated()
        //{
        //    //arrange
        //    var newEnvironment = new AddEnvironmentModel { ApplicationId = 1, EnvName = "DEV" };

        //    var controller = new FeatureTogglesController(_context);

        //    //act
        //    var result = controller.AddEnvironment(newEnvironment);

        //    //assert
        //    result.Should().BeOfType<OkResult>();
        //    _context.DeployEnvironments.Count().Should().Be(1);
        //}

        //[TestMethod]
        //public async Task AddEnvironment_ReturnBadRequestResult_WhenModelStateIsInvalid()
        //{
        //    //arrange
        //    var controller = new FeatureTogglesController(_context);
        //    controller.ModelState.AddModelError("error", "some error");

        //    //act
        //    var result = controller.AddEnvironment(new AddEnvironmentModel());

        //    //assert
        //    result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        //}

        //[TestMethod]
        //public async Task AddEnvironment_FeatureToggleStatus_IsCreated_ForEveryFeatureToggle()
        //{
        //    //arrange
        //    var app = new Application { Id = 1, AppName = "TestApp" };
        //    var newEnvironment = new AddEnvironmentModel { ApplicationId = 1, EnvName = "DEV" };
        //    var featureOne = new FeatureToggle { Application = app, Id = 1, ApplicationId = app.Id, ToggleName = "TestToggle" };
        //    var featureTwo = new FeatureToggle { Application = app, Id = 2, ApplicationId = app.Id, ToggleName = "OtherTestToggle" };

        //    _context.Applications.Add(app);
        //    _context.FeatureToggles.AddRange(featureOne, featureTwo);
        //    _context.SaveChanges();
        //    var controller = new FeatureTogglesController(_context);

        //    //act
        //    var result = controller.AddEnvironment(newEnvironment);

        //    //assert
        //    result.Should().BeOfType<OkResult>();
        //    _context.FeatureToggleStatuses.Count().Should().Be(2);
        //}

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

        //[TestMethod]
        //public async Task CreateEnvironment_A_NewEnvironmentIsBeingStoredInDB()
        //{
        //    //arrange
        //    var app = new Application { Id = 1, AppName = "TestApp" };
        //    var createdEnvironment = new AddEnvironmentModel { ApplicationId = app.Id, EnvName = "DEV" };
        //    var controller = new FeatureTogglesController(_context);

        //    //act
        //    var result = controller.AddEnvironment(createdEnvironment);

        //    //assert
        //    result.Should().BeOfType<OkResult>();
        //    _context.DeployEnvironments.FirstOrDefault(x => x.EnvName.Equals(createdEnvironment.EnvName)).Should()
        //        .NotBeNull();
        //    _context.DeployEnvironments.FirstOrDefault(x => x.DefaultToggleValue == false).Should().NotBeNull();
        //}

        //[TestMethod]
        //public async Task EditEnvironment_EnvironmentIsBeingModified()
        //{
        //    //arrange
        //    var app = new Application { Id = 1, AppName = "TestApp" };
        //    var environment = new DeployEnvironment
        //    { Application = app, ApplicationId = app.Id, EnvName = "DEV" };

        //    _context.DeployEnvironments.Add(environment);
        //    _context.SaveChanges();

        //    var controller = new FeatureTogglesController(_context);

        //    var updatedEnvironmentName = "QA";

        //    var updatedEnvironment = new UpdateEnvironmentModel
        //    {
        //        ApplicationId = environment.ApplicationId,
        //        InitialEnvName = environment.EnvName,
        //        NewEnvName = updatedEnvironmentName
        //    };

        //    //act
        //    var result = controller.UpdateEnvironment(updatedEnvironment);

        //    //assert
        //    result.Should().BeOfType<OkResult>();
        //    environment.EnvName.Should().Be(updatedEnvironmentName);
        //}

        //[TestMethod]
        //[ExpectedException(typeof(InvalidOperationException), "Environment does not exist!")]
        //public async Task EditEnvironment_EnvironmentIsModifiedWithInvalidID_ThrowsInvalidOperationException()
        //{
        //    //arrange
        //    var app = new Application { Id = 1, AppName = "TestApp" };
        //    var environment = new DeployEnvironment
        //    { Application = app, ApplicationId = app.Id, EnvName = "DEV" };

        //    _context.DeployEnvironments.Add(environment);
        //    _context.SaveChanges();

        //    var controller = new FeatureTogglesController(_context);

        //    var updatedEnvironmentName = "QA";

        //    var updatedEnvironment = new UpdateEnvironmentModel
        //    {
        //        ApplicationId = environment.ApplicationId + 1,
        //        InitialEnvName = environment.EnvName,
        //        NewEnvName = updatedEnvironmentName
        //    };

        //    //act
        //    controller.UpdateEnvironment(updatedEnvironment);

        //    //assert
        //    //throws InvalidOperationException
        //}

        //[TestMethod]
        //public async Task DeleteEnvironment_EnvironmentIsDeleted_FeatureToggleStatusesAreDeleted()
        //{
        //    //arrange
        //    var app = new Application { Id = 1, AppName = "TestApp" };
        //    var environment = new DeployEnvironment
        //    { Application = app, ApplicationId = app.Id, EnvName = "TestEnv" };

        //    var controller = new FeatureTogglesController(_context);

        //    var firstFeatureStatus = new FeatureToggleStatus { Enabled = false, Id = 1, IsDeployed = false, Environment = environment, EnvironmentId = environment.Id };
        //    var secondFeatureStatus = new FeatureToggleStatus { Enabled = false, Id = 2, IsDeployed = false, Environment = environment, EnvironmentId = environment.Id };
        //    var thirdFeatureStatus = new FeatureToggleStatus { Enabled = false, Id = 3, IsDeployed = false, Environment = environment, EnvironmentId = environment.Id };

        //    _context.FeatureToggleStatuses.AddRange(firstFeatureStatus, secondFeatureStatus, thirdFeatureStatus);
        //    _context.Applications.Add(app);
        //    _context.DeployEnvironments.Add(environment);
        //    _context.SaveChanges();

        //    var environmentToRemove = new DeleteEnvironmentModel
        //    {
        //        ApplicationId = environment.ApplicationId,
        //        EnvName = environment.EnvName
        //    };

        //    //act
        //    var result = controller.RemoveEnvironment(environmentToRemove);

        //    //assert
        //    result.Should().BeOfType<OkResult>();
        //    _context.DeployEnvironments.Count().Should().Be(0);
        //    _context.FeatureToggleStatuses.Count().Should().Be(0);
        //}

        //[TestMethod]
        //public async Task DeleteEnvironment_EnvironmentIsDeleted_FeatureTogglesAreNotDeleted()
        //{
        //    //arrange
        //    var app = new Application { Id = 1, AppName = "TestApp" };
        //    var environment = new DeployEnvironment
        //    { Application = app, ApplicationId = app.Id, EnvName = "TestEnv" };

        //    var featureToggle = new FeatureToggle { Id = 1 };

        //    var controller = new FeatureTogglesController(_context);

        //    var firstFeatureStatus = new FeatureToggleStatus { Enabled = false, Id = 1, FeatureToggle = featureToggle, IsDeployed = false, Environment = environment, EnvironmentId = environment.Id };
        //    var secondFeatureStatus = new FeatureToggleStatus { Enabled = false, Id = 2, FeatureToggle = featureToggle, IsDeployed = false, Environment = environment, EnvironmentId = environment.Id };
        //    var thirdFeatureStatus = new FeatureToggleStatus { Enabled = false, Id = 3, FeatureToggle = featureToggle, IsDeployed = false, Environment = environment, EnvironmentId = environment.Id };

        //    featureToggle.FeatureToggleStatuses.AddRange(new List<FeatureToggleStatus>()
        //    {
        //        firstFeatureStatus, secondFeatureStatus, thirdFeatureStatus
        //    });

        //    _context.FeatureToggles.Add(featureToggle);
        //    _context.FeatureToggleStatuses.AddRange(firstFeatureStatus, secondFeatureStatus, thirdFeatureStatus);
        //    _context.Applications.Add(app);
        //    _context.DeployEnvironments.Add(environment);
        //    _context.SaveChanges();

        //    var environmentToRemove = new DeleteEnvironmentModel
        //    {
        //        ApplicationId = environment.ApplicationId,
        //        EnvName = environment.EnvName
        //    };
        //    //act
        //    var result = controller.RemoveEnvironment(environmentToRemove);

        //    //assert
        //    result.Should().BeOfType<OkResult>();
        //    _context.DeployEnvironments.Count().Should().Be(0);
        //    _context.FeatureToggleStatuses.Count().Should().Be(0);
        //    _context.FeatureToggles.Count().Should().Be(1);
        //    _context.FeatureToggles.FirstOrDefault(x => x.Id == 1).FeatureToggleStatuses.Count().Should().Be(0);
        //}

        //[TestMethod]
        //[ExpectedException(typeof(InvalidOperationException), "Environment does not exist!")]
        //public async Task DeleteEnvironment_EnvironmentIsDeletedWithInvalidID_ThrowsInvalidOperationException()
        //{
        //    //arrange
        //    var app = new Application { Id = 1, AppName = "TestApp" };
        //    var environment = new DeployEnvironment
        //    { Application = app, ApplicationId = app.Id, EnvName = "TestEnv" };

        //    _context.DeployEnvironments.Add(environment);
        //    _context.SaveChanges();

        //    var controller = new FeatureTogglesController(_context);

        //    var environmentToRemove = new DeleteEnvironmentModel
        //    {
        //        ApplicationId = environment.ApplicationId + 1,
        //        EnvName = environment.EnvName
        //    };

        //    //act
        //    controller.RemoveEnvironment(environmentToRemove);

        //    //assert
        //    //throws InvalidOperationException
        //}
    }
}
