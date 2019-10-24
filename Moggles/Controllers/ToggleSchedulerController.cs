using Microsoft.AspNetCore.Mvc;
using Moggles.Domain;
using Moggles.Models;
using System;
using System.Threading.Tasks;

namespace Moggles.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToggleSchedulerController : ControllerBase
    {
        private readonly IRepository<ToggleSchedule> _toggleScheduleRepository;
        private readonly IRepository<Application> _applicationRepository;

        public ToggleSchedulerController(IRepository<ToggleSchedule> toggleScheduleRepository, IRepository<Application> applicationRepository)
        {
            _applicationRepository = applicationRepository;
            _toggleScheduleRepository = toggleScheduleRepository;
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> ScheduleToggles([FromBody] ScheduleTogglesModel model)
        {

            var user = User.Identity.Name;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var app = await _applicationRepository.FindByIdAsync(model.ApplicationId);
            if (app == null)
                throw new InvalidOperationException("Application does not exist!");

            foreach (var toggle in model.FeatureToggles)
            {
                var toggleSchedule = ToggleSchedule.Create(app.AppName, toggle, model.Environments, model.State, model.ScheduleDate.ToUniversalTime(), user);
                await _toggleScheduleRepository.AddAsync(toggleSchedule);
            }

            return Ok();
        }
    }
}