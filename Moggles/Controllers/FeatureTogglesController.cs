using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moggles.Models;
using Moggles.Data;
using Moggles.Domain;

namespace Moggles.Controllers
{
    [Produces("application/json")]
    [Route("api/FeatureToggles")]
    public class FeatureTogglesController : Controller
    {
        private readonly TogglesContext _db;
        private readonly TelemetryClient _telemetry = new TelemetryClient();

        public FeatureTogglesController(TogglesContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("")]
        public IActionResult GetToggles(int applicationId)
        {
            return Ok(_db.FeatureToggles
                .Where(ft => ft.ApplicationId == applicationId)
                .Include(ft => ft.FeatureToggleStatuses)
                .ThenInclude(fts => fts.Environment)
                .OrderByDescending(ft => ft.CreatedDate)
                .Select(ft => new FeatureToggleViewModel
                {
                    Id = ft.Id,
                    ToggleName = ft.ToggleName,
                    UserAccepted = ft.UserAccepted,
                    Notes = ft.Notes,
                    CreatedDate = ft.CreatedDate,
                    IsPermanent = ft.IsPermanent,
                    Statuses = ft.FeatureToggleStatuses.Select(fts => new FeatureToggleStatusViewModel
                    {
                        Id = fts.Id,
                        Environment = fts.Environment.EnvName,
                        Enabled = fts.Enabled,
                        IsDeployed = fts.IsDeployed,
                        LastUpdated = fts.LastUpdated,
                        FirstTimeDeployDate = fts.FirstTimeDeployDate
                    }).ToList()
                }));
        }

        [HttpGet]
        [Route("environments")]
        public IActionResult GetEnvironments(int applicationId)
        {
            List<DeployEnvironment> envs = GetEnvironmentsPerApp(applicationId);

            return Ok(envs
                .Select(e => e.EnvName)
                .Distinct());
        }

        private List<DeployEnvironment> GetEnvironmentsPerApp(int applicationId)
        {
            return _db.DeployEnvironments
                .Where(e => e.ApplicationId == applicationId)
                .OrderBy(e => e.SortOrder).ToList();
        }

        [HttpPut]
        [Route("")]
        public IActionResult Update([FromBody] FeatureToggleUpdateModel model)
        {
            var featureToggle = _db.FeatureToggles.Where(ft => ft.Id == model.Id)
                .Include(ft => ft.FeatureToggleStatuses).ThenInclude(fts => fts.Environment).FirstOrDefault();
            if (featureToggle is null)
                throw new InvalidOperationException("Feature toggle not found!");

            featureToggle.ToggleName = model.FeatureToggleName;
            featureToggle.UserAccepted = model.UserAccepted;
            featureToggle.Notes = model.Notes;
            featureToggle.IsPermanent = model.IsPermanent;
            foreach (var toggleStatus in model.Statuses)
            {
                var status = featureToggle.FeatureToggleStatuses.FirstOrDefault(s => s.Environment.EnvName == toggleStatus.Environment);
                if (status != null)
                {
                    UpdateTimestampOnChange(status, toggleStatus);
                    status.Enabled = toggleStatus.Enabled;
                }
            }

            _db.SaveChanges();

            return Ok(model);

            void UpdateTimestampOnChange(FeatureToggleStatus status, FeatureToggleStatusUpdateModel toggleStatus)
            {
                if (status.Enabled != toggleStatus.Enabled)
                    status.LastUpdated = DateTime.UtcNow;
            }
        }

        [HttpPost]
        [Route("addFeatureToggle")]
        public IActionResult AddFeatureToggle([FromBody]AddFeatureToggleModel featureToggleModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (featureToggleModel.ApplicationId <= 0)
                return BadRequest("Application not speficied!");

            var toggle = _db.FeatureToggles.FirstOrDefault(ft =>
                ft.ToggleName == featureToggleModel.FeatureToggleName &&
                ft.ApplicationId == featureToggleModel.ApplicationId);
            if (toggle != null)
                return BadRequest("Feature toggle with the same name already exists for this application!");

            var environments = GetEnvironmentsPerApp(featureToggleModel.ApplicationId);

            var featureToggle = _db.Add(new FeatureToggle
            {
                ToggleName = featureToggleModel.FeatureToggleName,
                ApplicationId = featureToggleModel.ApplicationId,
                Notes = featureToggleModel.Notes,
                IsPermanent = featureToggleModel.IsPermanent
            });

            _db.SaveChanges();

            foreach (var env in environments)
            {
                _db.Add(new FeatureToggleStatus
                {
                    Enabled = env.DefaultToggleValue,
                    EnvironmentId = env.Id,
                    FeatureToggleId = featureToggle.Entity.Id
                });
            }

            _db.SaveChanges();

            return Ok();
        }

        [HttpDelete]
        public IActionResult RemoveFeatureToggle([FromQuery] int id)
        {
            var toggleToDelete = new FeatureToggle { Id = id };

            _db.FeatureToggles.Attach(toggleToDelete);
            _db.FeatureToggles.Remove(toggleToDelete);
            _db.SaveChanges();

            return Ok();
        }

        [HttpPost]
        [Route("AddEnvironment")]
        public IActionResult AddEnvironment([FromBody] AddEnvironmentModel environmentModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (environmentModel.ApplicationId <= 0)
                throw new InvalidOperationException("Application not specified!");

            var env = _db.DeployEnvironments.FirstOrDefault(e => e.EnvName == environmentModel.EnvName &&
                                                                 e.ApplicationId == environmentModel.ApplicationId);
            if (env != null)
                throw new InvalidOperationException("Environment with the same name already exists for this application!");

            CreateEnvironment(environmentModel);

            return Ok();
        }

        private void CreateEnvironment(AddEnvironmentModel environmentModel)
        {
            var environment = _db.Add(new DeployEnvironment
            {
                ApplicationId = environmentModel.ApplicationId,
                DefaultToggleValue = environmentModel.DefaultToggleValue,
                EnvName = environmentModel.EnvName,
                SortOrder = environmentModel.SortOrder
            });

            _db.SaveChanges();

            var featureToggles = _db.FeatureToggles
                .Where(x => x.ApplicationId == environmentModel.ApplicationId)
                .ToList();

            foreach (var ft in featureToggles)
            {
                _db.Add(new FeatureToggleStatus
                {
                    FeatureToggleId = ft.Id,
                    Enabled = environmentModel.DefaultToggleValue,
                    EnvironmentId = environment.Entity.Id,
                });
            }

            _db.SaveChanges();
        }

        [HttpDelete]
        [Route("environments")]
        public IActionResult RemoveEnvironment([FromBody]DeleteEnvironmentModel environmentModel)
        {
            var environmentToDelete = _db.DeployEnvironments.FirstOrDefault(x =>
                x.ApplicationId == environmentModel.ApplicationId && x.EnvName == environmentModel.EnvName);

            if (environmentToDelete == null)
                throw new InvalidOperationException("Environment does not exist!");

            var featureToggleStatuses = _db.FeatureToggleStatuses
                .Where(e => e.EnvironmentId == environmentToDelete.Id);

            _db.FeatureToggleStatuses.RemoveRange(featureToggleStatuses);

            _db.DeployEnvironments.Remove(environmentToDelete);

            _db.SaveChanges();

            return Ok();
        }

        [HttpPut]
        [Route("UpdateEnvironment")]
        public IActionResult UpdateEnvironment([FromBody] UpdateEnvironmentModel environmentModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var env = _db.DeployEnvironments.FirstOrDefault(e => e.ApplicationId == environmentModel.ApplicationId && e.EnvName == environmentModel.InitialEnvName);

            if (env == null)
                throw new InvalidOperationException("Environment does not exist!");

            env.EnvName = environmentModel.NewEnvName;
            _db.SaveChanges();

            return Ok();
        }

        #region public API

        [HttpGet]
        [Route("getApplicationFeatureToggles")]
        [AllowAnonymous]
        public IActionResult GetApplicationFeatureToggles(string applicationName, string environment)
        {
            _telemetry.TrackEvent("OnGetAllToggles");

            var featureToggles = _db.FeatureToggleStatuses
                .Where(x => x.FeatureToggle.Application.AppName == applicationName)
                .Where(x => x.Environment.EnvName == environment)
                .Select(x => new ApplicationFeatureToggleViewModel
                {
                    FeatureToggleName = x.FeatureToggle.ToggleName,
                    IsEnabled = x.Enabled
                });

            return Ok(featureToggles);
        }

        [HttpGet]
        [Route("getApplicationFeatureToggleValue")]
        [AllowAnonymous]
        public IActionResult GetApplicationFeatureToggleValue(string applicationName, string environment, string featureToggleName)
        {
            _telemetry.TrackEvent("OnGetSpecificToggle");

            var featureToggle = _db.FeatureToggleStatuses
                .Where(x => x.FeatureToggle.ToggleName == featureToggleName)
                .Where(x => x.FeatureToggle.Application.AppName == applicationName)
                .Where(x => x.Environment.EnvName == environment)
                .Select(x => new ApplicationFeatureToggleViewModel
                {
                    FeatureToggleName = x.FeatureToggle.ToggleName,
                    IsEnabled = x.Enabled
                })
                .FirstOrDefault();

            if (featureToggle == null)
                return NotFound("Feature toggle does not exist!");

            return Ok(featureToggle);
        }

        [HttpPost]
        [Route("createEnvironment")]
        [AllowAnonymous]
        public IActionResult CreateEnvironment([FromBody]AddEnvironmentPublicApiModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var app = _db.Applications.FirstOrDefault(a => a.AppName == model.AppName);
            if (app == null)
                throw new InvalidOperationException("Application does not exist");

            CreateEnvironment(new AddEnvironmentModel
            {
                EnvName = model.EnvName,
                ApplicationId = app.Id,
                DefaultToggleValue = false,
                SortOrder = 500
            });

            return Ok();
        }

        #endregion
    }
}