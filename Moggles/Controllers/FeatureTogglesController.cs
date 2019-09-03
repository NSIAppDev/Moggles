using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moggles.Models;
using Moggles.Domain;
using Moggles.Domain.Repository;
using Moggles.Repository;

namespace Moggles.Controllers
{
    [Produces("application/json")]
    [Route("api/FeatureToggles")]
    public class FeatureTogglesController : Controller
    {
        private readonly TelemetryClient _telemetry = new TelemetryClient();
        private IRepository<Application> _applicationsRepository;
        private IRepository<DeployEnvironment> _deployEnvironmentRepository;
        private IRepository<FeatureToggle> _featureToggleRepository;
        private IRepository<FeatureToggleStatus> _featureToggleStatusRepository;

        public FeatureTogglesController(IRepository<Application> applicationsRepository, IRepository<DeployEnvironment> deployEnvironmentRepository, IRepository<FeatureToggle> featureToggleRepository, IRepository<FeatureToggleStatus> featureToggleStatusRepository)
        {
            _applicationsRepository = applicationsRepository;
            _deployEnvironmentRepository = deployEnvironmentRepository;
            _featureToggleRepository = featureToggleRepository;
            _featureToggleStatusRepository = featureToggleStatusRepository;
        }

        [HttpGet]
        [Route("")]
        public IActionResult GetToggles(Guid applicationId)
        {

            var featureToggleStatus = _featureToggleStatusRepository.GetAll().Result.AsQueryable();
            var environments = _deployEnvironmentRepository.GetAll().Result;
            return Ok(_featureToggleRepository.GetAll().Result.AsQueryable().Where(ft => ft.ApplicationId == applicationId)
                .OrderByDescending(ft => ft.CreatedDate)
                .Select(ft => new FeatureToggleViewModel
                {
                    Id = ft.Id,
                    ToggleName = ft.ToggleName,
                    UserAccepted = ft.UserAccepted,
                    Notes = ft.Notes,
                    CreatedDate = ft.CreatedDate,
                    IsPermanent = ft.IsPermanent,
                    Statuses = featureToggleStatus.Where(fts => fts.FeatureToggleId == ft.Id)
                        .Select(fts =>
                            new FeatureToggleStatusViewModel
                            {
                                Id = fts.Id,
                                Environment = environments.FirstOrDefault(env => env.Id == fts.EnvironmentId).EnvName,
                                Enabled = fts.Enabled,
                                IsDeployed = fts.IsDeployed,
                                LastUpdated = fts.LastUpdated,
                                FirstTimeDeployDate = fts.FirstTimeDeployDate
                            }).ToList()
                }));
        }

        [HttpGet]
        [Route("environments")]
        public IActionResult GetEnvironments(Guid applicationId)
        {
            List<DeployEnvironment> envs = GetEnvironmentsPerApp(applicationId);

            return Ok(envs
                .Select(e => e.EnvName)
                .Distinct());
        }

        private List<DeployEnvironment> GetEnvironmentsPerApp(Guid applicationId)
        {

            return _deployEnvironmentRepository.GetAll().Result
                .Where(e => e.ApplicationId == applicationId)
                .OrderBy(e => e.SortOrder).ToList();
        }

        [HttpPut]
        [Route("")]
        public IActionResult Update([FromBody] FeatureToggleUpdateModel model)
        {
            var featureToggle = _featureToggleRepository.GetAll().Result.AsQueryable().Where(ft => ft.Id == model.Id).FirstOrDefault();
            var featureToggleStatus = _featureToggleStatusRepository.GetAll().Result;
            var environments = _deployEnvironmentRepository.GetAll().Result;
            if (featureToggle is null)
                throw new InvalidOperationException("Feature toggle not found!");

            featureToggle.ToggleName = model.FeatureToggleName;
            featureToggle.UserAccepted = model.UserAccepted;
            featureToggle.Notes = model.Notes;
            featureToggle.IsPermanent = model.IsPermanent;
            foreach (var toggleStatus in model.Statuses)
            {
                var status = featureToggleStatus.FirstOrDefault(fts =>
                    fts.FeatureToggleId == model.Id && fts.EnvironmentId ==
                        environments.FirstOrDefault(env => env.EnvName == toggleStatus.Environment).Id);
                if (status != null)
                {
                    UpdateTimestampOnChange(status, toggleStatus);
                    status.Enabled = toggleStatus.Enabled;
                    _featureToggleStatusRepository.Update(status);
                }
            }

            _featureToggleRepository.Update(featureToggle);

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

            if (String.IsNullOrEmpty(featureToggleModel.ApplicationId.ToString()))
                return BadRequest("Application not specified!");

            var toggle = _featureToggleRepository.GetAll().Result.FirstOrDefault(ft =>
                ft.ToggleName == featureToggleModel.FeatureToggleName &&
                ft.ApplicationId == featureToggleModel.ApplicationId);
            if (toggle != null)
                return BadRequest("Feature toggle with the same name already exists for this application!");

            var environments = GetEnvironmentsPerApp(featureToggleModel.ApplicationId);

            var featureToggle = new FeatureToggle
            {
                Id = Guid.NewGuid(),
                ToggleName = featureToggleModel.FeatureToggleName,
                ApplicationId = featureToggleModel.ApplicationId,
                Notes = featureToggleModel.Notes,
                IsPermanent = featureToggleModel.IsPermanent,
                CreatedDate = DateTime.UtcNow

            };

            _featureToggleRepository.Add(featureToggle);

            var featureToggleStatus = new FeatureToggleStatus();
            foreach (var env in environments)
            {
                featureToggleStatus = new FeatureToggleStatus
                {
                    Id = Guid.NewGuid(),
                    Enabled = env.DefaultToggleValue,
                    EnvironmentId = env.Id,
                    FeatureToggleId = featureToggle.Id,
                    LastUpdated = DateTime.UtcNow
                };
                _featureToggleStatusRepository.Add(featureToggleStatus);
                
            }

            return Ok();
        }

        [HttpDelete]
        public IActionResult RemoveFeatureToggle([FromQuery] Guid id)
        {
            var toggleToDelete = new FeatureToggle { Id = id };

            _featureToggleRepository.Delete(toggleToDelete);

            return Ok();
        }

        [HttpPost]
        [Route("AddEnvironment")]
        public IActionResult AddEnvironment([FromBody] AddEnvironmentModel environmentModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (String.IsNullOrEmpty(environmentModel.ApplicationId.ToString()))
                    throw new InvalidOperationException("Application not specified!");

            var env = _deployEnvironmentRepository.GetAll().Result.FirstOrDefault(e => e.EnvName == environmentModel.EnvName &&
                                                                 e.ApplicationId == environmentModel.ApplicationId);
            if (env != null)
                throw new InvalidOperationException("Environment with the same name already exists for this application!");

            CreateEnvironment(environmentModel);

            return Ok();
        }

        private void CreateEnvironment(AddEnvironmentModel environmentModel)
        {
            var environment = new DeployEnvironment
            {
                Id = Guid.NewGuid(),
                ApplicationId = environmentModel.ApplicationId,
                DefaultToggleValue = environmentModel.DefaultToggleValue,
                EnvName = environmentModel.EnvName,
                SortOrder = environmentModel.SortOrder
            };

            _deployEnvironmentRepository.Add(environment);

            var featureToggles = _featureToggleRepository.GetAll().Result
                .Where(x => x.ApplicationId == environmentModel.ApplicationId)
                .ToList();

            var featureToggleStatus = new FeatureToggleStatus();
            foreach (var ft in featureToggles)
            {
                featureToggleStatus = new FeatureToggleStatus
                {
                    Id = Guid.NewGuid(),
                    FeatureToggleId = ft.Id,
                    Enabled = environmentModel.DefaultToggleValue,
                    EnvironmentId = environment.Id,
                    LastUpdated = DateTime.UtcNow
                };
                _featureToggleStatusRepository.Add(featureToggleStatus);
            }

        }

        [HttpDelete]
        [Route("environments")]
        public IActionResult RemoveEnvironment([FromBody]DeleteEnvironmentModel environmentModel)
        {
            var environmentToDelete = _deployEnvironmentRepository.GetAll().Result.FirstOrDefault(x =>
                x.ApplicationId == environmentModel.ApplicationId && x.EnvName == environmentModel.EnvName);

            if (environmentToDelete == null)
                throw new InvalidOperationException("Environment does not exist!");

            var featureToggleStatuses = _featureToggleStatusRepository.GetAll().Result
                .Where(e => e.EnvironmentId == environmentToDelete.Id);

            foreach (var featureToggleStatus in featureToggleStatuses)
            {
                _featureToggleStatusRepository.Delete(featureToggleStatus);
            }
            _deployEnvironmentRepository.Delete(environmentToDelete);

            return Ok();
        }

        [HttpPut]
        [Route("UpdateEnvironment")]
        public IActionResult UpdateEnvironment([FromBody] UpdateEnvironmentModel environmentModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var env = _deployEnvironmentRepository.GetAll().Result.FirstOrDefault(e => e.ApplicationId == environmentModel.ApplicationId && e.EnvName == environmentModel.InitialEnvName);

            if (env == null)
                throw new InvalidOperationException("Environment does not exist!");

            env.EnvName = environmentModel.NewEnvName;
            _deployEnvironmentRepository.Update(env);

            return Ok();
        }

        #region public API

        [HttpGet]
        [Route("getApplicationFeatureToggles")]
        [AllowAnonymous]
        public IActionResult GetApplicationFeatureToggles(string applicationName, string environment)
        {
            _telemetry.TrackEvent("OnGetAllToggles");

            var featureToggles = _featureToggleStatusRepository.GetAll().Result
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

            var featureToggle = _featureToggleStatusRepository.GetAll().Result
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

            var app = _applicationsRepository.GetAll().Result.FirstOrDefault(a => a.AppName == model.AppName);
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