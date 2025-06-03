using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moggles.Domain;
using Moggles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moggles.Controllers
{
    [Produces("application/json")]
    [Route("api/Applications")]
    public class ApplicationsController : Controller
    {
        private readonly IRepository<Application> _applicationsRepository;
        private readonly IRepository<ToggleSchedule> _toggleScheduleRepository;
        private readonly IConfiguration _configuration;

        public ApplicationsController(
            IRepository<Application> applicationsRepository, 
            IRepository<ToggleSchedule> toggleScheduleRepository,
            IConfiguration configuration)
        {
            _applicationsRepository = applicationsRepository;
            _toggleScheduleRepository = toggleScheduleRepository;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllApplications()
        {
            var allApps = await _applicationsRepository.GetAllAsync();

            return Ok(allApps.Select(a => new ApplicationSummary //return summary objects to avoid sending un-needed data
            {
                Id = a.Id,
                AppName = a.AppName,
                AssignedTo = a.AssignedTo,
                HasBeenMigrated = a.HasBeenMigrated,
                IsDeleted = a.IsDeleted
            }).AsEnumerable()
            .OrderBy(a => a.AppName)
            .ToList());
        }

        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddApplication([FromBody] AddApplicationModel applicationModel)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var apps = await _applicationsRepository.GetAllAsync();
            var app = apps.FirstOrDefault(a => string.Compare(a.AppName, applicationModel.ApplicationName, StringComparison.OrdinalIgnoreCase) == 0);

            var hasBeenMigrated = apps.Any(a => a.HasBeenMigrated);

            if (app != null)
                return BadRequest("Application with same name already exists!");

            var application = Application.Create(applicationModel.ApplicationName, applicationModel.EnvironmentName, applicationModel.DefaultToggleValue, hasBeenMigrated);

            await _applicationsRepository.AddAsync(application);

            return Ok(application);
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> UpdateApplication([FromBody] UpdateApplicationModel applicationModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var app = await _applicationsRepository.FindByIdAsync(applicationModel.Id);
            if (app == null)
                throw new InvalidOperationException("Application does not exist!");

            var apps = await _applicationsRepository.GetAllAsync();
            var existingApp = apps.FirstOrDefault(a =>
                string.Compare(a.AppName, applicationModel.ApplicationName, StringComparison.OrdinalIgnoreCase) == 0 && a.Id != applicationModel.Id);

            if (existingApp != null)
                return BadRequest("Application with same name already exists!");

            var appName = applicationModel.ApplicationName ?? app.AppName;
            var assignedTo = applicationModel.ApplicationAssignedTo ?? app.AssignedTo;
            var isDeleted = applicationModel.isDeleted ?? app.IsDeleted;
            app.Update(appName, isDeleted, assignedTo);
            await _applicationsRepository.UpdateAsync(app);

            return Ok();
        }

        [HttpGet("assignedto-options")]
        public IActionResult GetAssignedToOptions()
        {
            var options = _configuration.GetSection("AssignedToOptions").Get<List<string>>();
            return Ok(options);
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveApp([FromQuery] Guid id)
        {
            var app = await _applicationsRepository.FindByIdAsync(id);

            if (app == null)
                throw new InvalidOperationException("Application does not exist!");

            app.MarkAsDeleted();
            await _applicationsRepository.UpdateAsync(app);

            await DeleteAllSchedulersForApp(app.AppName);

            return Ok();
        }

        private async Task DeleteAllSchedulersForApp(string appName)
        {
            var schedulers = (await _toggleScheduleRepository.GetAllAsync()).Where(sch => sch.ApplicationName.Equals(appName)).ToList();
            schedulers.ForEach(async _ => await _toggleScheduleRepository.DeleteAsync(_));
        }
    }
}