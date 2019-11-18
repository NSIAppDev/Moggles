using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moggles.Domain;
using Moggles.Models;
using System;
using System.Threading.Tasks;

namespace Moggles.Controllers
{
    [Authorize()]
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
    }
}