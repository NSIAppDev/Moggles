using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Internal.Account.Manage;
using Microsoft.AspNetCore.Mvc;
using Moggles.Models;
using Moggles.Domain;
using Moggles.Repository;

namespace Moggles.Controllers
{
    [Produces("application/json")]
    [Route("api/Applications")]
    public class ApplicationsController : Controller
    {
        private IRepository<Application> _applicationsRepository;

        public ApplicationsController(IRepository<Application> applicationsRepository)
        {
            _applicationsRepository = applicationsRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllApplications()

        {
            return Ok(_applicationsRepository.GetAll().Result.ToList().OrderBy(a=>a.AppName));
        }


        [HttpPost]
        [Route("add")]
        public IActionResult AddApplication([FromBody]AddApplicationModel applicationModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var apps = _applicationsRepository.GetAll().Result.ToList();
            var app = apps.FirstOrDefault(a => a.AppName == applicationModel.ApplicationName);
            
            if (app != null)
                throw new InvalidOperationException("Application already exists!");

            var application = new Application
            {
                Id = Guid.NewGuid(),
                AppName = applicationModel.ApplicationName,
                DeploymentEnvironments = new List<DeployEnvironment>() {
                    new DeployEnvironment()
                    {
                        Id = Guid.NewGuid(),
                        EnvName = applicationModel.EnvironmentName,
                        DefaultToggleValue = applicationModel.DefaultToggleValue,
                        SortOrder = 1
                    }
                }
            };


            _applicationsRepository.Add(application);

            return Ok(application);
        }

        [HttpPut]
        [Route("update")]
        public IActionResult UpdateApplication([FromBody]UpdateApplicationModel applicationModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var app = _applicationsRepository.FindById(applicationModel.Id).Result;


            if (app == null)
                throw new InvalidOperationException("Application does not exists!");
            app.AppName = applicationModel.ApplicationName; // Make the change
            _applicationsRepository.Update(app);

            return Ok();
        }

        [HttpDelete]
        public IActionResult RemoveApp([FromQuery] Guid id)
        {
            var app = _applicationsRepository.FindById(id).Result;

            if (app == null)
                throw new InvalidOperationException("Application does not exists!");

            _applicationsRepository.Delete(app);


            return Ok();
        }

    }
}