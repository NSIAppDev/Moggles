using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Moggles.Models;
using Moggles.Data;
using Moggles.Domain;

namespace Moggles.Controllers
{
    [Produces("application/json")]
    [Route("api/Applications")]
    public class ApplicationsController : Controller
    {
        private readonly TogglesContext _db;

        public ApplicationsController(TogglesContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult GetAllApplications()

        {
            return Ok(_db.Applications.ToList());
        }

        [HttpPost]
        [Route("add")]
        public IActionResult AddApplication([FromBody]AddApplicationModel applicationModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var app = _db.Applications.FirstOrDefault(a => a.AppName == applicationModel.ApplicationName);
            
            if (app != null)
                throw new InvalidOperationException("Application already exists!");

            var application = _db.Applications.Add(new Application
            {
                AppName = applicationModel.ApplicationName
            });

            _db.SaveChanges();

            _db.DeployEnvironments.Add(
                new DeployEnvironment
                {
                    EnvName = applicationModel.EnvironmentName,
                    ApplicationId = application.Entity.Id,
                    DefaultToggleValue = applicationModel.DefaultToggleValue,
                    SortOrder = 1
                });

            _db.SaveChanges();

            return Ok(application.Entity);
        }

        [HttpPut]
        [Route("update")]
        public IActionResult UpdateApplication([FromBody]UpdateApplicationModel applicationModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var app = _db.Applications.FirstOrDefault(a => a.Id == applicationModel.Id);

            if (app == null)
                throw new InvalidOperationException("Application does not exists!");

            app.AppName = applicationModel.ApplicationName;

            _db.SaveChanges();

            return Ok();
        }

        [HttpDelete]
        public IActionResult RemoveApp([FromQuery] int id)
        {
            var toggles = _db.FeatureToggles
                .Where(e => e.ApplicationId == id)
                .Select(x => new FeatureToggle
                {
                    Id = x.Id
                });

            foreach (var toggle in toggles)
            {
                _db.FeatureToggles.Remove(toggle);
            }

            _db.SaveChanges();

            var environments = _db.DeployEnvironments
                .Where(e => e.ApplicationId == id)
                .Select(x => new DeployEnvironment
                {
                    Id = x.Id
                });

            foreach (var env in environments)
            {
                var featureToggleStatuses = _db.FeatureToggleStatuses
                    .Where(e => e.EnvironmentId == env.Id)
                    .Select(x => new FeatureToggleStatus
                    {
                        Id = x.Id
                    });

                foreach (var status in featureToggleStatuses)
                {
                    _db.FeatureToggleStatuses.Remove(status);

                }

                _db.SaveChanges();

                _db.DeployEnvironments.Remove(env);

                _db.SaveChanges();
            }

            var app = new Application
            {
                Id = id
            };

            _db.Applications.Remove(app);

            _db.SaveChanges();

            return Ok();
        }

    }
}