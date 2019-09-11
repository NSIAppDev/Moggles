using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moggles.Domain;
using Moggles.Models;

namespace Moggles.Controllers
{
    [Produces("application/json")]
    [Route("api/Applications")]
    public class ApplicationsController : Controller
    {
        private readonly IRepository<Application> _applicationsRepository;

        public ApplicationsController(IRepository<Application> applicationsRepository)
        {
            _applicationsRepository = applicationsRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllApplications()
        {
            var allApps = await _applicationsRepository.GetAllAsync();
            return Ok(allApps.OrderBy(a => a.AppName).ToList());
        }

        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddApplication([FromBody]AddApplicationModel applicationModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var apps = await _applicationsRepository.GetAllAsync();
            var app = apps.FirstOrDefault(a => a.AppName == applicationModel.ApplicationName);
            
            if (app != null)
                throw new InvalidOperationException("Application already exists!");

            var application = new Application
            {
                Id = Guid.NewGuid(),
                AppName = applicationModel.ApplicationName,
                DeploymentEnvironments = new List<DeployEnvironment>
                {
                    new DeployEnvironment
                    {
                        Id = Guid.NewGuid(),
                        EnvName = applicationModel.EnvironmentName,
                        DefaultToggleValue = applicationModel.DefaultToggleValue,
                        SortOrder = 1
                    }
                }
            };

            await _applicationsRepository.AddAsync(application);

            return Ok(application);
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> UpdateApplication([FromBody]UpdateApplicationModel applicationModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var app = await _applicationsRepository.FindByIdAsync(applicationModel.Id);

            if (app == null)
                throw new InvalidOperationException("Application does not exists!");

            app.AppName = applicationModel.ApplicationName; 
            await _applicationsRepository.UpdateAsync(app);

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveApp([FromQuery] Guid id)
        {
            var app = await _applicationsRepository.FindByIdAsync(id);

            if (app == null)
                throw new InvalidOperationException("Application does not exists!");

            await _applicationsRepository.DeleteAsync(app);

            return Ok();
        }

    }
}