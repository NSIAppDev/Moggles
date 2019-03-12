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
    }
}