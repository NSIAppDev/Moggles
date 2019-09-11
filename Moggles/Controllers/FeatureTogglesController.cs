using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moggles.Domain;
using Moggles.Models;

namespace Moggles.Controllers
{
    [Produces("application/json")]
    [Route("api/FeatureToggles")]
    public class FeatureTogglesController : Controller
    {
        private readonly TelemetryClient _telemetry = new TelemetryClient();
        private readonly IRepository<Application> _applicationsRepository;


        public FeatureTogglesController(IRepository<Application> applicationsRepository)
        {
            _applicationsRepository = applicationsRepository;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetToggles(Guid applicationId)
        {
            var app = await _applicationsRepository.FindByIdAsync(applicationId);
            return Ok(app.FeatureToggles
                    .Select(ft => new FeatureToggleViewModel
                    {
                        Id = ft.Id,
                        ToggleName = ft.ToggleName,
                        UserAccepted = ft.UserAccepted,
                        Notes = ft.Notes,
                        CreatedDate = ft.CreatedDate,
                        IsPermanent = ft.IsPermanent,
                        Statuses = ft.FeatureToggleStatuses
                            .Select(fts =>
                                new FeatureToggleStatusViewModel
                                {
                                    Environment = app.DeploymentEnvironments.FirstOrDefault(env => env.Id == fts.EnvironmentId)?.EnvName,
                                    Enabled = fts.Enabled,
                                    IsDeployed = fts.IsDeployed,
                                    LastUpdated = fts.LastUpdated,
                                    FirstTimeDeployDate = fts.FirstTimeDeployDate
                                }).ToList()
                    }).OrderByDescending(ft => ft.CreatedDate));

        }

        [HttpGet]
        [Route("environments")]
        public async Task<IActionResult> GetEnvironments(Guid applicationId)
        {
            var app = await _applicationsRepository.FindByIdAsync(applicationId);
            var envs = app.DeploymentEnvironments
                .OrderBy(e => e.SortOrder).ToList();

            return Ok(envs
                .Select(e => e.EnvName)
                .Distinct());
        }



        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Update([FromBody] FeatureToggleUpdateModel model)
        {
            var app = await _applicationsRepository.FindByIdAsync(model.ApplicationId);
            var featureToggle = app.FeatureToggles.Where(ft => ft.Id == model.Id).FirstOrDefault();

            if (featureToggle is null)
                throw new InvalidOperationException("Feature toggle not found!");

            featureToggle.ToggleName = model.FeatureToggleName;
            featureToggle.UserAccepted = model.UserAccepted;
            featureToggle.Notes = model.Notes;
            featureToggle.IsPermanent = model.IsPermanent;
            foreach (var toggleStatus in model.Statuses)
            {
                var status = featureToggle.FeatureToggleStatuses.FirstOrDefault(fts =>
                    fts.EnvironmentId == app.DeploymentEnvironments
                        .Where(env => env.EnvName == toggleStatus.Environment).FirstOrDefault().Id);
                if (status != null)
                {
                    UpdateTimestampOnChange(status, toggleStatus);
                    status.Enabled = toggleStatus.Enabled;
                }
            }

            await _applicationsRepository.UpdateAsync(app);

            return Ok(model);

            void UpdateTimestampOnChange(FeatureToggleStatus status, FeatureToggleStatusUpdateModel toggleStatus)
            {
                if (status.Enabled != toggleStatus.Enabled)
                    status.LastUpdated = DateTime.UtcNow;
            }
        }

        [HttpPost]
        [Route("addFeatureToggle")]
        public async Task<IActionResult> AddFeatureToggle([FromBody]AddFeatureToggleModel featureToggleModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(featureToggleModel.ApplicationId.ToString()))
                return BadRequest("Application not specified!");

            var app = await _applicationsRepository.FindByIdAsync(featureToggleModel.ApplicationId);

            var toggle = app.FeatureToggles.FirstOrDefault(ft =>
                ft.ToggleName == featureToggleModel.FeatureToggleName);
            if (toggle != null)
                return BadRequest("Feature toggle with the same name already exists for this application!");

            var environments = app.DeploymentEnvironments.ToList();

            var featureToggleStatuses = new List<FeatureToggleStatus>();
            foreach (var env in environments)
            {
                featureToggleStatuses.Add(new FeatureToggleStatus
                {
                    Enabled = env.DefaultToggleValue,
                    EnvironmentId = env.Id,
                    LastUpdated = DateTime.UtcNow
                });
            }

            var featureToggle = new FeatureToggle
            {
                Id = Guid.NewGuid(),
                ToggleName = featureToggleModel.FeatureToggleName,
                Notes = featureToggleModel.Notes,
                IsPermanent = featureToggleModel.IsPermanent,
                CreatedDate = DateTime.UtcNow,
                FeatureToggleStatuses = featureToggleStatuses

            };
            app.FeatureToggles.Add(featureToggle);
            await _applicationsRepository.UpdateAsync(app);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveFeatureToggle([FromQuery] Guid id, Guid applicationId)
        {
            var app = await _applicationsRepository.FindByIdAsync(applicationId);
            var toggleToDelete = app.FeatureToggles.FirstOrDefault(ft => ft.Id == id);
            app.FeatureToggles.Remove(toggleToDelete);

            await _applicationsRepository.UpdateAsync(app);

            return Ok();
        }

        [HttpPost]
        [Route("AddEnvironment")]
        public async Task<IActionResult> AddEnvironment([FromBody] AddEnvironmentModel environmentModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(environmentModel.ApplicationId.ToString()))
                throw new InvalidOperationException("Application not specified!");
            var app = await _applicationsRepository.FindByIdAsync(environmentModel.ApplicationId);

            var env = app.DeploymentEnvironments.FirstOrDefault(e => e.EnvName == environmentModel.EnvName);
            if (env != null)
                throw new InvalidOperationException("Environment with the same name already exists for this application!");

            await CreateEnvironment(app, environmentModel);

            return Ok();
        }

        private async Task CreateEnvironment(Application app, AddEnvironmentModel environmentModel)
        {
            var environment = new DeployEnvironment
            {
                Id = Guid.NewGuid(),
                DefaultToggleValue = environmentModel.DefaultToggleValue,
                EnvName = environmentModel.EnvName,
                SortOrder = environmentModel.SortOrder
            };

            app.DeploymentEnvironments.Add(environment);

            var featureToggles = app.FeatureToggles.ToList();

            foreach (var ft in featureToggles)
            {
                ft.FeatureToggleStatuses.Add(new FeatureToggleStatus
                {
                    Enabled = environmentModel.DefaultToggleValue,
                    EnvironmentId = environment.Id,
                    LastUpdated = DateTime.UtcNow
                });
            }

            await _applicationsRepository.UpdateAsync(app);
        }

        [HttpDelete]
        [Route("environments")]
        public async Task<IActionResult> RemoveEnvironment([FromBody]DeleteEnvironmentModel environmentModel)
        {
            var app = await _applicationsRepository.FindByIdAsync(environmentModel.ApplicationId);

            var environmentToDelete = app.DeploymentEnvironments.FirstOrDefault(x => x.EnvName == environmentModel.EnvName);

            if (environmentToDelete == null)
                throw new InvalidOperationException("Environment does not exist!");

            app.DeploymentEnvironments.Remove(environmentToDelete);

            var featureToggleStatuses = app.FeatureToggles.SelectMany(ft =>
                ft.FeatureToggleStatuses).Where(fts => fts.EnvironmentId == environmentToDelete.Id).ToList();

            foreach (var featureToggleStatus in featureToggleStatuses)
            {
                app.FeatureToggles.ForEach(ft => ft.FeatureToggleStatuses.Remove(featureToggleStatus));
            }

            await _applicationsRepository.UpdateAsync(app);
            return Ok();
        }

        [HttpPut]
        [Route("UpdateEnvironment")]
        public async Task<IActionResult> UpdateEnvironment([FromBody] UpdateEnvironmentModel environmentModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var app = await _applicationsRepository.FindByIdAsync(environmentModel.ApplicationId);
            var env = app.DeploymentEnvironments.FirstOrDefault(e => e.EnvName == environmentModel.InitialEnvName);

            if (env == null)
                throw new InvalidOperationException("Environment does not exist!");

            env.EnvName = environmentModel.NewEnvName;
            await _applicationsRepository.UpdateAsync(app);

            return Ok();
        }

        #region public API

        [HttpGet]
        [Route("getApplicationFeatureToggles")]
        [AllowAnonymous]
        public async Task<IActionResult> GetApplicationFeatureToggles(string applicationName, string environment)
        {
            _telemetry.TrackEvent("OnGetAllToggles");

            var apps = await _applicationsRepository.GetAllAsync();
            var app = apps.FirstOrDefault(a => a.AppName == applicationName);
            var featureToggles = app.FeatureToggles
                .Select(x => new ApplicationFeatureToggleViewModel
                {
                    FeatureToggleName = x.ToggleName,
                    IsEnabled = x.FeatureToggleStatuses.FirstOrDefault(fts => fts.EnvironmentId == app.DeploymentEnvironments.FirstOrDefault(env => env.EnvName == environment).Id).Enabled
                });


            return Ok(featureToggles);
        }

        [HttpGet]
        [Route("getApplicationFeatureToggleValue")]
        [AllowAnonymous]
        public async Task<IActionResult> GetApplicationFeatureToggleValue(string applicationName, string environment, string featureToggleName)
        {
            _telemetry.TrackEvent("OnGetSpecificToggle");

            var app = (await _applicationsRepository.GetAllAsync()).FirstOrDefault(a => a.AppName == applicationName);

            var featureToggle = app.FeatureToggles
                .Where(ft => ft.ToggleName == featureToggleName)
                .Select(x => new ApplicationFeatureToggleViewModel
                {
                    FeatureToggleName = x.ToggleName,
                    IsEnabled = x.FeatureToggleStatuses.FirstOrDefault(fts => fts.EnvironmentId == app.DeploymentEnvironments.FirstOrDefault(env => env.EnvName == environment).Id).Enabled
                })
                .FirstOrDefault();

            if (featureToggle == null)
                return NotFound("Feature toggle does not exist!");

            return Ok(featureToggle);
        }

        [HttpPost]
        [Route("createEnvironment")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateEnvironment([FromBody]AddEnvironmentPublicApiModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var app = (await _applicationsRepository.GetAllAsync()).FirstOrDefault(a => a.AppName == model.AppName);
            if (app == null)
                throw new InvalidOperationException("Application does not exist");

            await CreateEnvironment(app, new AddEnvironmentModel
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