using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Moggles.Domain;
using Moggles.Models;

namespace Moggles.Controllers
{
    [Produces("application/json")]
    [Route("api/public")]
    public class PublicApiController : Controller
    {
        private readonly TelemetryClient _telemetry = new TelemetryClient();
        private readonly IRepository<Application> _applicationsRepository;

        public PublicApiController(IRepository<Application> applicationsRepository)
        {
            _applicationsRepository = applicationsRepository;
        }

        [HttpGet]
        [Route("getApplicationFeatureToggles")]
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
                        IsEnabled = x.FeatureToggleStatuses.First(fts => string.Compare(fts.EnvironmentName, environment, StringComparison.OrdinalIgnoreCase) == 0).Enabled
                    });

                return Ok(featureToggles);
            }

            return BadRequest("Environment or Application does not exist");

        }

        [HttpGet]
        [Route("getApplicationFeatureToggleValue")]
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
                        IsEnabled = x.FeatureToggleStatuses.First(fts => string.Compare(fts.EnvironmentName, environment, StringComparison.OrdinalIgnoreCase) == 0).Enabled
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
        public async Task<IActionResult> CreateEnvironment([FromBody] AddEnvironmentPublicApiModel model)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var app = await GetApplicationByName(model.AppName);
            if (app == null)
                throw new InvalidOperationException("Application does not exist");

            app.AddDeployEnvironment(model.EnvName, false, false, false, 500);

            await _applicationsRepository.UpdateAsync(app);
            return Ok();
        }

        private async Task<Application> GetApplicationByName(string applicationName)
        {
            var apps = await _applicationsRepository.GetAllAsync();
            return apps.FirstOrDefault(a => string.Compare(a.AppName, applicationName, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}
