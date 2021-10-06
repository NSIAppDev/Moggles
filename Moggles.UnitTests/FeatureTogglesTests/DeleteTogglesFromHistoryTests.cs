using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Controllers;
using Moggles.Domain;
using Moggles.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Moggles.UnitTests.FeatureTogglesTests
{
    public class DeleteTogglesFromHistoryTests
    {
        [TestClass]
        public class RemoveFeatureToggleTests
        {
            private IRepository<Application> _appRepository;
            private IHttpContextAccessor _httpContextAccessor;
            private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
            private IRepository<ToggleSchedule> _toggleScheduleRepository;
            private FeatureTogglesController _featureToggleController;

            [TestInitialize]
            public void BeforeTest()
            {
                _appRepository = new InMemoryApplicationRepository();
                _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
                _mockHttpContextAccessor.Setup(x => x.HttpContext.User.Identity.Name).Returns("bla");
                _httpContextAccessor = _mockHttpContextAccessor.Object;
                _toggleScheduleRepository = new InMemoryRepository<ToggleSchedule>();
                _featureToggleController = new FeatureTogglesController(_appRepository, _httpContextAccessor, _toggleScheduleRepository);
            }

            [TestMethod]
            public async Task DeleteToggleFromHistory_FeatureToggleIsDeleted()
            {
                //arrange
                var app = Application.Create("TestDelete", "PROD", false);
                var toggleId = Guid.NewGuid();
                app.DeletedFeatureToggles.Add(new DeletedFeatureToggle
                {
                    Id = toggleId,
                    ToggleName = "someName",
                    Reason = "someReason",
                    DeletionDate = DateTime.UtcNow
                });
                await _appRepository.AddAsync(app);

                var model = new DeleteTogglesFromHistoryModel
                {
                    ToggleIds = new List<Guid> { toggleId},
                    ApplicationId = app.Id
                };

                //act
                var result = await _featureToggleController.DeleteTogglesFromHistory(model);

                //assert
                result.Should().BeOfType<OkResult>();
                (await _appRepository.FindByIdAsync(app.Id)).DeletedFeatureToggles.Count.Should().Be(0);
            }
        }
    }
}
