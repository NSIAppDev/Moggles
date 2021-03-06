﻿using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Controllers;
using Moggles.Domain;
using Moggles.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Moggles.UnitTests.ApplicationsTests
{
    [TestClass]
    public class ApplicationsTest
    {
        private InMemoryApplicationRepository _appApplicationRepository;
        private IHttpContextAccessor _httpContextAccessor;
        private IRepository<ToggleSchedule> _toggleScheduleRepository;

        [TestInitialize]
        public void BeforeTest()
        {
            _appApplicationRepository = new InMemoryApplicationRepository();
            _toggleScheduleRepository = new InMemoryRepository<ToggleSchedule>();
        }

        [TestMethod]
        public async Task GetApplications_ReturnsAllExistingApplications()
        {
            //arrange
            var bccApp = Application.Create("BCC", "dev", false);
            var cmmApp = Application.Create("CMM", "dev", false);

            await _appApplicationRepository.AddAsync(bccApp);
            await _appApplicationRepository.AddAsync(cmmApp);

            var controller = new ApplicationsController(_appApplicationRepository, _toggleScheduleRepository);

            //act
            var result = await controller.GetAllApplications() as OkObjectResult;

            //assert
            Debug.Assert(result != null, nameof(result) + " != null");
            result.Value.As<List<Application>>().Should().BeEquivalentTo(new List<Application> { bccApp, cmmApp });
        }

        #region Add

        [TestMethod]
        public async Task AddApplication_ReturnBadRequestResult_WhenModelStateIsInvalid()
        {
            //arrange
            var controller = new ApplicationsController(_appApplicationRepository, _toggleScheduleRepository);
            controller.ModelState.AddModelError("error", "some error");

            //act
            var result = await controller.AddApplication(new AddApplicationModel());

            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        }

        [TestMethod]
        public async Task AddApplication_ApplicationIsBeingAdded()
        {
            //arrange
            var appModel = new AddApplicationModel { ApplicationName = "BCC" , UpdatedByUser = "updatedBy" };
            var controller = new ApplicationsController(_appApplicationRepository, _toggleScheduleRepository);


            //act
            await controller.AddApplication(appModel);

            //assert
            (await _appApplicationRepository.GetAllAsync()).FirstOrDefault()?.AppName.Should().Be(appModel.ApplicationName);
        }

        [TestMethod]
        public async Task AddApplication_ApplicationIsNotAdded_WhenOneWithTheSameNameAlreadyExists_CaseInsensitive()
        {
            //arrange
            var app = Application.Create("bcc", "dev", false);
            await _appApplicationRepository.AddAsync(app);
            var appModel = new AddApplicationModel { ApplicationName = "BCC" };
            var controller = new ApplicationsController(_appApplicationRepository, _toggleScheduleRepository);

            //act
            var result  = await controller.AddApplication(appModel);

            //assert
            result.Should().BeOfType<BadRequestObjectResult>();
            (await _appApplicationRepository.GetAllAsync()).Single().AppName.Should().Be("bcc");
        }

        [TestMethod]
        public async Task AddApplication_DefaultEnvironmentIsAdded()
        {
            //arrange
            var appModel = new AddApplicationModel { ApplicationName = "BCC", EnvironmentName = "Test", DefaultToggleValue = false};
            var controller = new ApplicationsController(_appApplicationRepository, _toggleScheduleRepository);

            //act
            await controller.AddApplication(appModel);

            //assert
            var app = (await _appApplicationRepository.GetAllAsync()).FirstOrDefault(x => x.AppName.Equals(appModel.ApplicationName));

            Debug.Assert(app != null, nameof(app) + " != null");
            var results = app.DeploymentEnvironments.ToList();
            results.Count.Should().Be(1);
            results[0].EnvName.Should().Be("Test");
            results[0].DefaultToggleValue.Should().BeFalse();
            results[0].RequireReasonWhenToggleEnabled.Should().BeFalse();
            results[0].RequireReasonWhenToggleDisabled.Should().BeFalse();
        }

        #endregion

        #region Update

        [TestMethod]
        public async Task EditApp_AppIsBeingModified()
        {
            //arrange
            var app = Application.Create("TestApp", "dev", false);
            await _appApplicationRepository.AddAsync(app);

            var updatedAppName = "TestAppUpdated";

            var updatedApp = new UpdateApplicationModel
            {
                Id = app.Id,
                ApplicationName = updatedAppName
            };

            var controller = new ApplicationsController(_appApplicationRepository, _toggleScheduleRepository);

            //act
            var result = await controller.UpdateApplication(updatedApp);

            //assert
            result.Should().BeOfType<OkResult>();
            app.AppName.Should().Be(updatedAppName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task EditApp_WithInvalidID_ThrowsInvalidOperationException()
        {
            //arrange
            var app = Application.Create("TestApp", "dev", false);
            await _appApplicationRepository.AddAsync(app);

            var updatedAppName = "TestAppUpdated";

            var updatedApp = new UpdateApplicationModel
            {
                Id = Guid.NewGuid(),
                ApplicationName = updatedAppName
            };

            var controller = new ApplicationsController(_appApplicationRepository, _toggleScheduleRepository);

            //act
            await controller.UpdateApplication(updatedApp);

            //assert
            //throws InvalidOperationException
        }

        [TestMethod]
        public async Task EditApplication_ReturnBadRequestResult_WhenModelStateIsInvalid()
        {
            //arrange
            var controller = new ApplicationsController(_appApplicationRepository, _toggleScheduleRepository);
            controller.ModelState.AddModelError("error", "some error");

            //act
            var result = await controller.UpdateApplication(new UpdateApplicationModel());

            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        }

        [TestMethod]
        public async Task EditApp_WhenAlreadyExistsAppWithTheSameName_RejectTheEdit()
        {
            //arrange
            var app = Application.Create("TestApp", "dev", false);
            var app2 = Application.Create("TestAppUpdated", "dev", false);
            await _appApplicationRepository.AddAsync(app);
            await _appApplicationRepository.AddAsync(app2);

            var updatedApp = new UpdateApplicationModel
            {
                Id = app.Id,
                ApplicationName = "TestAppUpdated"
            };

            var controller = new ApplicationsController(_appApplicationRepository, _toggleScheduleRepository);

            //act
            var result = await controller.UpdateApplication(updatedApp);

            //assert
            result.Should().BeOfType<BadRequestObjectResult>();
            app.AppName.Should().Be("TestApp");
        }
        #endregion

        #region Delete

        [TestMethod]
        public async Task DeleteApp_AppIsDeleted()
        {
            //arrange
            var app = Application.Create("test", "dev", false);

            await _appApplicationRepository.AddAsync(app);

            var controller = new ApplicationsController(_appApplicationRepository, _toggleScheduleRepository);

            //act
            var result = await controller.RemoveApp(app.Id);

            //assert
            result.Should().BeOfType<OkResult>();
            (await _appApplicationRepository.GetAllAsync()).Count().Should().Be(0);

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteApp_WithInvalidID_ThrowsInvalidOperationException()
        {
            //arrange
            var app = Application.Create("TestApp", "dev", false);
            await _appApplicationRepository.AddAsync(app);

            var controller = new ApplicationsController(_appApplicationRepository, _toggleScheduleRepository);

            //act
            await controller.RemoveApp(Guid.NewGuid());

            //assert
            //throws InvalidOperationException
        }

        [TestMethod]
        public async Task DeleteApp_SchedulersForAppAreDeleted()
        {
            //arrange
            var app = Application.Create("TestApp", "dev", false);
            await _appApplicationRepository.AddAsync(app);

            var date = new DateTime(2099, 3, 2, 15, 45, 0);
            app.AddDeployEnvironment("QA", false, false, false);
            app.AddFeatureToggle("t1", null, "workItemID1");
            app.AddFeatureToggle("t2", null, "workItemID2");
            await _appApplicationRepository.AddAsync(app);
            var schedule = ToggleSchedule.Create("TestApp", "t1", new[] { "dev" }, true, date, "updatedBy", true);
            await _toggleScheduleRepository.AddAsync(schedule);

            var controller = new ApplicationsController(_appApplicationRepository, _toggleScheduleRepository);

            //act
            await controller.RemoveApp(app.Id);

            //assert
            var schedulers =await _toggleScheduleRepository.GetAllAsync();
            schedulers.Count().Should().Be(0);
        }
        #endregion
    }
}
