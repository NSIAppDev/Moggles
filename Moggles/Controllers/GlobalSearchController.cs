using Microsoft.AspNetCore.Mvc;
using Moggles.Domain;
using Moggles.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Moggles.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class GlobalSearchController(IRepository<Application> applicationRepository) : ControllerBase
	{
		[HttpPost]
		public async Task<IActionResult> Search([FromBody] GlobalSearchModel model)
		{
			var allApplications = await applicationRepository.GetAllAsync();

			var featureToggles = allApplications.Where(a => !a.IsDeleted)
			.SelectMany(app => app.FeatureToggles.Select(f => new
			{
				Application = app,
				Toggle = f
			}));

			if (!string.IsNullOrWhiteSpace(model.Keyword))
				featureToggles = featureToggles.Where(f => f.Toggle.ToggleName.Trim().Contains(model.Keyword.Trim(), System.StringComparison.OrdinalIgnoreCase) || f.Toggle.Notes.Trim().Contains(model.Keyword.Trim(), System.StringComparison.OrdinalIgnoreCase));

			if (model.ApplicationIds.Count > 0)
			{
				featureToggles = featureToggles.Where(x => model.ApplicationIds.Contains(x.Application.Id));
			}

			if (!string.IsNullOrWhiteSpace(model.ProgressStatus))
				featureToggles = featureToggles.Where(f => f.Toggle.ProgressStatus == model.ProgressStatus);

			if (model.CreatedStart.HasValue)
				featureToggles = featureToggles.Where(f => f.Toggle.CreatedDate >= model.CreatedStart.Value);

			if (model.CreatedEnd.HasValue)
				featureToggles = featureToggles.Where(f => f.Toggle.CreatedDate <= model.CreatedEnd.Value);

			foreach (var env in model.Environments.Where(e => !string.IsNullOrEmpty(e.Name) && e.Enabled != null))
			{
				featureToggles = featureToggles.Where(e => e.Toggle.FeatureToggleStatuses.Any(s => s.EnvironmentName.Trim().Equals(env.Name.Trim(), System.StringComparison.OrdinalIgnoreCase) && s.Enabled == env.Enabled));
			}

			if (!string.IsNullOrWhiteSpace(model.WorkItemIdentifier))
				featureToggles = featureToggles.Where(f => !string.IsNullOrWhiteSpace(f.Toggle.WorkItemIdentifier) && f.Toggle.WorkItemIdentifier.Trim()
						.Contains(model.WorkItemIdentifier?.Trim(), StringComparison.OrdinalIgnoreCase));

			if (model.AssignedTo.Count > 0)
			{
				featureToggles = featureToggles.Where(x => model.AssignedTo.Contains(x.Application.AssignedTo));
			}

			if (model.IsPermanent != null)
				featureToggles = featureToggles.Where(f => f.Toggle.IsPermanent == model.IsPermanent);

			if (model.UserAccepted != null)
				featureToggles = featureToggles.Where(f => f.Toggle.UserAccepted == model.UserAccepted);

			if (model.ChangedStart.HasValue)
			{
				featureToggles = featureToggles.Where(f =>
				{
					var statuses = f.Toggle.FeatureToggleStatuses;
					if (statuses == null || !statuses.Any())
						return false;

					var latest = statuses.OrderByDescending(s => s.LastUpdated).FirstOrDefault();
					return latest != null && latest.LastUpdated >= model.ChangedStart.Value;
				});
			}

			if (model.ChangedEnd.HasValue)
			{
				featureToggles = featureToggles.Where(f =>
				{
					var statuses = f.Toggle.FeatureToggleStatuses;
					if (statuses == null || !statuses.Any())
						return false;

					var latest = statuses.OrderByDescending(s => s.LastUpdated).FirstOrDefault();
					return latest != null && latest.LastUpdated <= model.ChangedEnd.Value;
				});
			}

			var result = featureToggles
				.Select(f => new GlobalSearchFeatureToggleViewModel
				{
					Id = f.Toggle.Id,
					ToggleName = f.Toggle.ToggleName,
					ApplicationName = f.Application.AppName,
					ApplicationId = f.Application.Id,
					Environments = f.Toggle.FeatureToggleStatuses
						.Select(fts =>
							new FeatureToggleStatusViewModel
							{
								Environment = fts.EnvironmentName,
								Enabled = fts.Enabled,
								IsDeployed = fts.IsDeployed,
								LastUpdated = fts.LastUpdated,
								FirstTimeDeployDate = fts.FirstTimeDeployDate,
								UpdatedByUser = fts.UpdatedbyUser
							}).ToList(),
					AssignedTo = f.Application.AssignedTo,
					WorkItemIdentifier = f.Toggle.WorkItemIdentifier,
					Notes = f.Toggle.Notes,
					ProgressStatus = f.Toggle.ProgressStatus,
					UserAccepted = f.Toggle.UserAccepted,
					CreatedDate = f.Toggle.CreatedDate,
					ChangedDate = f.Toggle.FeatureToggleStatuses.OrderByDescending(_ => _.LastUpdated).FirstOrDefault().LastUpdated,
					IsPermanent = f.Toggle.IsPermanent,
					HasBeenMigrated = f.Application.HasBeenMigrated
				})
				.OrderBy(f => f.ToggleName)
				.ToList();

			return Ok(result);
		}
	}
}
