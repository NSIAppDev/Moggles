using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moggles.Domain;
using Moggles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moggles.Controllers
{
    [Authorize(Policy = "OnlyAdmins")]
    [Produces("application/json")]
    [Route("api/FeatureToggles")]
    public class FeatureTogglesController : Controller
    {
        private readonly TelemetryClient _telemetry = new TelemetryClient();
        private readonly IRepository<Application> _applicationsRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public FeatureTogglesController(IRepository<Application> applicationsRepository, IHttpContextAccessor httpContextAccessor)
        {
            _applicationsRepository = applicationsRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetToggles(Guid applicationId)
        {
            var app = await _applicationsRepository.FindByIdAsync(applicationId);
            var toggles = app.FeatureToggles
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
                                Environment = fts.EnvironmentName,
                                Enabled = fts.Enabled,
                                IsDeployed = fts.IsDeployed,
                                LastUpdated = fts.LastUpdated,
                                FirstTimeDeployDate = fts.FirstTimeDeployDate,
                                UpdatedByUser = fts.UpdatedbyUser
                            }).ToList()
                }).OrderByDescending(ft => ft.CreatedDate);
            return Ok(toggles);

        }

        [HttpGet]
        [Route("environments")]
        public async Task<IActionResult> GetEnvironments(Guid applicationId)
        {
            var app = await _applicationsRepository.FindByIdAsync(applicationId);
            var envs = app.DeploymentEnvironments.OrderBy(e => e.SortOrder).ToList();

            return Ok(envs
                .Select(e => e.EnvName)
                .Distinct());
        }

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Update([FromBody] FeatureToggleUpdateModel model)
        {
            var app = await _applicationsRepository.FindByIdAsync(model.ApplicationId);
            var toggleData = app.GetFeatureToggleBasicData(model.Id);

            var updatedBy = _httpContextAccessor.HttpContext.User.Identity.Name;


            if (model.IsPermanent != toggleData.IsPermanent)
            {
                app.UpdateFeatureTogglePermanentStatus(model.Id, model.IsPermanent);
            }

            if (model.Notes != toggleData.Notes)
            {
                app.UpdateFeatureToggleNotes(model.Id, model.Notes);
            }

            if (model.UserAccepted != toggleData.UserAccepted)
            {
                if (model.UserAccepted)
                {
                    app.FeatureAcceptedByUser(model.Id);
                }
                else
                {
                    app.FeatureRejectedByUser(model.Id);
                }

            }

            if (model.FeatureToggleName != toggleData.ToggleName)
            {
                try
                {
                    app.ChangeFeatureToggleName(model.Id, model.FeatureToggleName);
                }
                catch (BusinessRuleValidationException ex)
                {
                    return BadRequest(ex.Message);
                }

            }
            foreach (var newStatus in model.Statuses)
            {
                app.SetToggle(model.Id, newStatus.Environment, newStatus.Enabled, updatedBy);
            }


            await _applicationsRepository.UpdateAsync(app);
            return Ok(model);
        }

        [HttpPost]
        [Route("addFeatureToggle")]
        public async Task<IActionResult> AddFeatureToggle([FromBody]AddFeatureToggleModel featureToggleModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!featureToggleModel.ApplicationId.HasValue || featureToggleModel.ApplicationId == Guid.Empty)
                return BadRequest("Application not specified!");

            var app = await _applicationsRepository.FindByIdAsync(featureToggleModel.ApplicationId.Value);

            try
            {
                app.AddFeatureToggle(featureToggleModel.FeatureToggleName, featureToggleModel.Notes, featureToggleModel.IsPermanent);
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(ex.Message);
            }

            await _applicationsRepository.UpdateAsync(app);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveFeatureToggle([FromQuery] Guid id, Guid applicationId)
        {
            var app = await _applicationsRepository.FindByIdAsync(applicationId);

            app.RemoveFeatureToggle(id);

            await _applicationsRepository.UpdateAsync(app);
            return Ok();
        }

        [HttpPost]
        [Route("AddEnvironment")]
        public async Task<IActionResult> AddEnvironment([FromBody] AddEnvironmentModel environmentModel)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (environmentModel.ApplicationId == Guid.Empty)
            {
                throw new InvalidOperationException("Application not specified!");
            }

            var app = await _applicationsRepository.FindByIdAsync(environmentModel.ApplicationId);

            try
            {
                app.AddDeployEnvironment(environmentModel.EnvName, environmentModel.DefaultToggleValue, environmentModel.SortOrder);
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(ex.Message);
            }

            await _applicationsRepository.UpdateAsync(app);
            return Ok();
        }

        [HttpDelete]
        [Route("environments")]
        public async Task<IActionResult> RemoveEnvironment([FromBody]DeleteEnvironmentModel environmentModel)
        {
            var app = await _applicationsRepository.FindByIdAsync(environmentModel.ApplicationId);

            app.DeleteDeployEnvironment(environmentModel.EnvName);

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
            try
            {
                app.ChangeDeployEnvironmentName(environmentModel.InitialEnvName, environmentModel.NewEnvName);
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(ex.Message);
            }

            await _applicationsRepository.UpdateAsync(app);
            return Ok();
        }

        private async Task<Application> GetApplicationByName(string applicationName)
        {
            var apps = await _applicationsRepository.GetAllAsync();
            return apps.FirstOrDefault(a => string.Compare(a.AppName, applicationName, StringComparison.OrdinalIgnoreCase) == 0);
        }

        #region public API

        [HttpGet]
        [Route("getApplicationFeatureToggles")]
        [AllowAnonymous]
        public async Task<IActionResult> GetApplicationFeatureToggles(string applicationName, string environment)
        {
            _telemetry.TrackEvent("OnGetAllToggles");

            var app = await GetApplicationByName(applicationName);
            if (app != null && app.DeploymentEnvironments.Exists(env => string.Compare(env.EnvName, environment, StringComparison.OrdinalIgnoreCase) == 0))
            {
                var featureToggles = app.FeatureToggles
                    .Select(x => new ApplicationFeatureToggleViewModel
                    {
                        FeatureToggleName = x.ToggleName,
                        IsEnabled = x.FeatureToggleStatuses.FirstOrDefault(fts => string.Compare(fts.EnvironmentName, environment, StringComparison.OrdinalIgnoreCase) == 0).Enabled
                    });

                return Ok(featureToggles);
            }

            return BadRequest("Environment or Application does not exist");

        }

        [HttpGet]
        [Route("getApplicationFeatureToggleValue")]
        [AllowAnonymous]
        public async Task<IActionResult> GetApplicationFeatureToggleValue(string applicationName, string environment, string featureToggleName)
        {
            _telemetry.TrackEvent("OnGetSpecificToggle");

            var app = await GetApplicationByName(applicationName);
            if (app != null && app.DeploymentEnvironments.Exists(env => string.Compare(env.EnvName, environment, StringComparison.OrdinalIgnoreCase) == 0))
            {
                var featureToggle = app.FeatureToggles
                    .Where(ft => string.Compare(ft.ToggleName, featureToggleName, StringComparison.OrdinalIgnoreCase) == 0)
                    .Select(x => new ApplicationFeatureToggleViewModel
                    {
                        FeatureToggleName = x.ToggleName,
                        IsEnabled = x.FeatureToggleStatuses.FirstOrDefault(fts => string.Compare(fts.EnvironmentName, environment, StringComparison.OrdinalIgnoreCase) == 0).Enabled
                    })
                    .FirstOrDefault();

                if (featureToggle == null)
                    return NotFound("Feature toggle does not exist!");

                return Ok(featureToggle);
            }

            return BadRequest("Environment or Application does not exist");
        }

        [HttpPost]
        [Route("createEnvironment")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateEnvironment([FromBody]AddEnvironmentPublicApiModel model)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var app = await GetApplicationByName(model.AppName);
            if (app == null)
                throw new InvalidOperationException("Application does not exist");

            app.AddDeployEnvironment(model.EnvName, false, 500);

            await _applicationsRepository.UpdateAsync(app);
            return Ok();
        }

        #endregion
    }
}