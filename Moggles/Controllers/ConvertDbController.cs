using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Moggles.Data;
using Moggles.Domain;
using Moggles.Repository;

namespace Moggles.Controllers
{
    [Produces("application/json")]
    [Route("api/ConvertDb")]
    public class ConvertDbController : Controller
    {
        private readonly TogglesContext _db;
        private IRepository<Application> _applicationsRepository;
        private IRepository<DeployEnvironment> _deployEnvironmentRepository;
        private IRepository<FeatureToggle> _featureToggleRepository;
        private IRepository<FeatureToggleStatus> _featureToggleStatusRepository;

        public ConvertDbController(TogglesContext db, IRepository<Application> applicationsRepository, IRepository<DeployEnvironment> deployEnvironmentRepository, IRepository<FeatureToggle> featureToggleRepository, IRepository<FeatureToggleStatus> featureToggleStatusRepository)
        {
            _db = db;
            _applicationsRepository = applicationsRepository;
            _deployEnvironmentRepository = deployEnvironmentRepository;
            _featureToggleRepository = featureToggleRepository;
            _featureToggleStatusRepository = featureToggleStatusRepository;
        }

        [HttpGet]
        public async Task<IActionResult> ConvertExistingDb()
        {
            // This query will only grab the details where an app has a feature toggle
            var all = _db.FeatureToggles.Include(ft => ft.FeatureToggleStatuses).ThenInclude(fts => fts.Environment).ThenInclude(env=> env.Application).ToImmutableList();

            var allFeatureToggles = all.Select(ft => new FeatureToggle
            {
                Id = ft.NewId,
                CreatedDate = ft.CreatedDate,
                ApplicationId = ft.Application.NewId,
                IsPermanent = ft.IsPermanent,
                Notes = ft.Notes,
                ToggleName = ft.ToggleName,
                UserAccepted = ft.UserAccepted
            }).GroupBy(ft => ft.Id).Select(x => x.FirstOrDefault());

            var allFeatureToggleStatus = all.SelectMany(ft => ft.FeatureToggleStatuses.Select(fts => new FeatureToggleStatus
            {
                Id = fts.NewId,
                FeatureToggleId = fts.FeatureToggle.NewId,
                EnvironmentId = fts.Environment.NewId,
                Enabled = fts.Enabled,
                FirstTimeDeployDate = fts.FirstTimeDeployDate,
                IsDeployed = fts.IsDeployed,
                LastDeployStatusUpdate = fts.LastDeployStatusUpdate,
                LastUpdated = fts.LastUpdated
            })).GroupBy(fts=>fts.Id).Select(x => x.FirstOrDefault());

            var allApplications = all.SelectMany(ft => ft.FeatureToggleStatuses.Select(fts => fts.Environment)
                .Select(env => env.Application).Select(app => new Application
                {
                    Id = app.NewId,
                    AppName = app.AppName
                })).GroupBy(app=>app.Id).Select(x=>x.FirstOrDefault());

            var allDeploymentEnvironments = all.SelectMany(ft => ft.FeatureToggleStatuses.Select(fts => fts.Environment).Select(env => new DeployEnvironment
            {
                Id = env.NewId,
                EnvName = env.EnvName,
                DefaultToggleValue = env.DefaultToggleValue,
                ApplicationId = env.Application.NewId,
                SortOrder = env.SortOrder
            })).GroupBy(env => env.Id).Select(x => x.FirstOrDefault());

            // in order to get apps/envs without feature toggles, we need to do something else
            var allApplicationIds = all.SelectMany(ft => ft.FeatureToggleStatuses.Select(fts => fts.Environment)
                .Select(env => env.Application).Select(app => app.Id)).Distinct();
            var missingData = _db.DeployEnvironments.Include(env => env.Application)
                .Where(env => !allApplicationIds.Contains(env.ApplicationId)).ToImmutableList();

            var missingApplications = missingData.Select(env=>env.Application).Select(app => new Application
            {
                Id=app.NewId,
                AppName = app.AppName
            }).GroupBy(app => app.Id).Select(x => x.FirstOrDefault());

            var missingDeploymentEnvironments = missingData.Select(env => new DeployEnvironment
            {
                Id = env.NewId,
                EnvName = env.EnvName,
                DefaultToggleValue = env.DefaultToggleValue,
                ApplicationId = env.Application.NewId,
                SortOrder = env.SortOrder
            }).GroupBy(env => env.Id).Select(x => x.FirstOrDefault());

            allApplications = allApplications.Union(missingApplications);
            allDeploymentEnvironments = allDeploymentEnvironments.Union(missingDeploymentEnvironments);

            allApplications.ToList().ForEach(a => _applicationsRepository.Add(a));
            allDeploymentEnvironments.ToList().ForEach(d => _deployEnvironmentRepository.Add(d));
            allFeatureToggles.ToList().ForEach(ft => _featureToggleRepository.Add(ft));
            allFeatureToggleStatus.ToList().ForEach(fts => _featureToggleStatusRepository.Add(fts));

            return RedirectToAction("index", "home");
        }
    }
}
