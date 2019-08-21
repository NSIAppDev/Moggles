using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Internal.Account.Manage;
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
                return BadRequest("Application already exists!");
                
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
            var app = _db.Applications.FirstOrDefault(x => x.Id == id);

            if (app == null)
                throw new InvalidOperationException("Application does not exists!");

            var toggles = _db.FeatureToggles
                .Where(e => e.ApplicationId == id);

            var environments = _db.DeployEnvironments
                .Where(e => e.ApplicationId == id).ToList();
            var environmentIds = environments.Select(_ => _.Id);

            var ftsToDelete = _db.FeatureToggleStatuses.Where(_ => environmentIds.Contains(_.EnvironmentId));

            _db.FeatureToggleStatuses.RemoveRange(ftsToDelete);
            _db.FeatureToggles.RemoveRange(toggles);
            _db.DeployEnvironments.RemoveRange(environments);
            _db.Applications.Remove(app);

            _db.SaveChanges();

            return Ok();
        }

    }
}