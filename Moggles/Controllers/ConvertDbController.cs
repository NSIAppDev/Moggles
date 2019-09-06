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

        public ConvertDbController(TogglesContext db, IRepository<Application> applicationsRepository)
        {
            _db = db;
            _applicationsRepository = applicationsRepository;
        }

        [HttpGet]
        public async Task<IActionResult> ConvertExistingDb()
        {
            // This query will only grab the details where an app has a feature toggle
            var all = _db.FeatureToggles.Include(ft => ft.FeatureToggleStatuses).ThenInclude(fts => fts.Environment).ThenInclude(env=> env.Application).ToImmutableList();


            var allApplications = all.SelectMany(ft => ft.FeatureToggleStatuses.Select(fts => fts.Environment)
                .Select(env => env.Application).Select(app => new Application
                {
                    Id = app.NewId,
                    AppName = app.AppName,
                    DeploymentEnvironments = all
                                            .SelectMany(ft1 => ft1.FeatureToggleStatuses
                                            .Select(fts1 => fts1.Environment)
                                            .Where(env=> env.ApplicationId==app.Id)
                                            .Select(env => new DeployEnvironment
                                            {
                                                Id = env.NewId,
                                                EnvName = env.EnvName,
                                                DefaultToggleValue = env.DefaultToggleValue,
                                                SortOrder = env.SortOrder
                                            })).GroupBy(env => env.Id).Select(x => x.FirstOrDefault()).ToList(),
                    FeatureToggles = all
                                    .Where(ft2 => ft2.ApplicationId == app.Id)
                                    .Select(ft2 => new FeatureToggle
                                    {
                                        Id = ft2.NewId,
                                        CreatedDate = ft2.CreatedDate,
                                        IsPermanent = ft2.IsPermanent,
                                        Notes = ft2.Notes,
                                        ToggleName = ft2.ToggleName,
                                        UserAccepted = ft2.UserAccepted,
                                        FeatureToggleStatuses = ft2.FeatureToggleStatuses
                                                                    .Select(fts2 => new FeatureToggleStatus
                                                                    {
                                                                        EnvironmentId = fts2.Environment.NewId,
                                                                        Enabled = fts2.Enabled,
                                                                        FirstTimeDeployDate = fts2.FirstTimeDeployDate,
                                                                        IsDeployed = fts2.IsDeployed,
                                                                        LastDeployStatusUpdate = fts2.LastDeployStatusUpdate,
                                                                        LastUpdated = fts2.LastUpdated
                                                                    }).ToList()
                                    }).GroupBy(ft2 => ft2.Id).Select(x => x.FirstOrDefault()).ToList(),
                })).GroupBy(app=>app.Id).Select(x=>x.FirstOrDefault());



            // in order to get apps/envs without feature toggles, we need to do something else
            var allApplicationIds = all.SelectMany(ft => ft.FeatureToggleStatuses.Select(fts => fts.Environment)
                .Select(env => env.Application).Select(app => app.Id)).Distinct();
            var missingData = _db.DeployEnvironments.Include(env => env.Application)
                .Where(env => !allApplicationIds.Contains(env.ApplicationId)).ToImmutableList();

            var missingApplications = missingData.Select(env=>env.Application).Select(app => new Application
            {
                Id=app.NewId,
                AppName = app.AppName,
                DeploymentEnvironments = missingData
                    .Where(env=>env.ApplicationId==app.Id)
                    .Select(env => new DeployEnvironment
                {
                    Id = env.NewId,
                    EnvName = env.EnvName,
                    DefaultToggleValue = env.DefaultToggleValue,
                    SortOrder = env.SortOrder
                }).GroupBy(env => env.Id).Select(x => x.FirstOrDefault()).ToList()
            }).GroupBy(app => app.Id).Select(x => x.FirstOrDefault());






            allApplications.ToList().ForEach(a => _applicationsRepository.Add(a));
            missingApplications.ToList().ForEach(a => _applicationsRepository.Add(a));

            return RedirectToAction("index", "home");
        }
    }
}
