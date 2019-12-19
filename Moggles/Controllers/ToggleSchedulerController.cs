using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moggles.Domain;
using Moggles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moggles.Controllers
{
    [Authorize(Policy = "OnlyAdmins")]
    [Route("api/[controller]")]
    [ApiController]
    public class ToggleSchedulerController : ControllerBase
    {
        private readonly IRepository<ToggleSchedule> _toggleScheduleRepository;
        private readonly IRepository<Application> _applicationRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ToggleSchedulerController(IRepository<ToggleSchedule> toggleScheduleRepository, IRepository<Application> applicationRepository, IHttpContextAccessor httpContextAccessor)
        {
            _applicationRepository = applicationRepository;
            _toggleScheduleRepository = toggleScheduleRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> ScheduleToggles([FromBody] ScheduleTogglesModel model)
        {
            var updatedBy = _httpContextAccessor.HttpContext.User.Identity.Name;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var app = await _applicationRepository.FindByIdAsync(model.ApplicationId);
            if (app == null)
                throw new InvalidOperationException("Application does not exist!");

            foreach (var toggle in model.FeatureToggles)
            {
                var toggleSchedule = ToggleSchedule.Create(app.AppName, toggle, model.Environments, model.State, model.ScheduleDate.ToUniversalTime(), updatedBy);
                await _toggleScheduleRepository.AddAsync(toggleSchedule);
            }

            return Ok();
        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetScheduledToggles(Guid applicationId)
        {
            var app = await _applicationRepository.FindByIdAsync(applicationId);
            var scheduledToggles = (await _toggleScheduleRepository.GetAllAsync()).ToList().Where(fts => fts.ApplicationName == app.AppName);
            return Ok(scheduledToggles);
        }
        [HttpGet]
        [Route("getToggleScheduler")]
        public async Task<IActionResult> GetFeatureSchedule([FromQuery] Guid toggleId)
        {
            var toggle = await _toggleScheduleRepository.FindByIdAsync(toggleId);
            return Ok(toggle);
        }

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Update([FromBody] UpdateFeatureToggleSchedulerModel model)
        {
            var updatedBy = _httpContextAccessor.HttpContext.User.Identity.Name;
            var toggleSchedule = await _toggleScheduleRepository.FindByIdAsync(model.Id);
            toggleSchedule.Environments = new List<string>();

            foreach (var env in  model.Environments)
            {
                toggleSchedule.AddEnvironment(env);
            }
            toggleSchedule.ChangeDate(model.ScheduledDate);
            toggleSchedule.ChangeState(model.ScheduledState);
            toggleSchedule.ChangeUpdatedBy(updatedBy);
            
            await _toggleScheduleRepository.UpdateAsync(toggleSchedule);
            return Ok(toggleSchedule);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] Guid id)
        {
            var toggleScheduler = await _toggleScheduleRepository.FindByIdAsync(id);

            if (toggleScheduler == null)
                throw new InvalidOperationException("Toggle Scheduler does not exist!");

            await _toggleScheduleRepository.DeleteAsync(toggleScheduler);

            return Ok();
        }
    }
}