using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moggles.Domain;
using Moggles.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Moggles.Controllers
{
    [Produces("application/json")]
    [Route("api/FeatureToggles")]
    public class FeatureTogglesController : Controller
    {
        private readonly IRepository<Application> _applicationsRepository;
        private readonly IRepository<ToggleSchedule> _toggleScheduleRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FeatureTogglesController(IRepository<Application> applicationsRepository, IHttpContextAccessor httpContextAccessor, IRepository<ToggleSchedule> toggleScheduleRepository)
        {
            _applicationsRepository = applicationsRepository;
            _toggleScheduleRepository = toggleScheduleRepository;
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
                    ChangedDate = ft.FeatureToggleStatuses.OrderByDescending(_ => _.LastUpdated).FirstOrDefault().LastUpdated,
                    IsPermanent = ft.IsPermanent,
                    WorkItemIdentifier = ft.WorkItemIdentifier,
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
                            }).ToList(),
                    ReasonsToChange = ft.ReasonsToChange
                        .Select(ftr =>
                        new FeatureToggleReasonToChangeViewModel
                        {
                            AddedByUser = ftr.AddedByUser,
                            CreatedAt = ftr.DateAdded,
                            Description = ftr.Description,
                            Environments = ftr.Environments
                        }).ToList()
                }).OrderBy(ft => ft.ToggleName);
            return Ok(toggles);

        }

        [HttpGet]
        [Route("environments")]
        public async Task<IActionResult> GetEnvironments(Guid applicationId)
        {
            var app = await _applicationsRepository.FindByIdAsync(applicationId);
            var envs = app.DeploymentEnvironments.OrderBy(e => e.SortOrder).ToList();

            return Ok(envs.Distinct());
        }

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Update([FromBody] FeatureToggleUpdateModel model)
        {
            var app = await _applicationsRepository.FindByIdAsync(model.ApplicationId);
            var toggleData = app.GetFeatureToggleBasicData(model.Id);

            var updatedBy = _httpContextAccessor.HttpContext.User.Identity.Name;

            if (model.ReasonToChange!=null)
            {
                app.UpdateFeatureToggleReasonsToChange(model.Id, updatedBy, model.ReasonToChange.Description, model.ReasonToChange.Environments);
            }


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
            if (!string.IsNullOrEmpty(model.WorkItemIdentifier) && model.WorkItemIdentifier != toggleData.WorkItemIdentifier)
            {
                app.UpdateFeaturetoggleWorkItemIdentifier(model.Id, model.WorkItemIdentifier);
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
                app.AddFeatureToggle(featureToggleModel.FeatureToggleName, featureToggleModel.Notes, featureToggleModel.WorkItemIdentifier, featureToggleModel.IsPermanent);
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(ex.Message);
            }

            await _applicationsRepository.UpdateAsync(app);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveFeatureToggle([FromBody] DeleteFeatureToggleModel featureToggleModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var app = await _applicationsRepository.FindByIdAsync(featureToggleModel.ApplicationId);
            var toggle = app.GetFeatureToggleBasicData(featureToggleModel.FeatureToggleId);
            var toggleSchedulers = _toggleScheduleRepository.GetAllAsync().Result.Where(ft => ft.ToggleName == toggle.ToggleName);

            foreach (var fts in toggleSchedulers)
            {
                await _toggleScheduleRepository.DeleteAsync(fts);
            }

            app.RemoveFeatureToggle(featureToggleModel.FeatureToggleId, toggle.ToggleName, featureToggleModel.Reason);

            await _applicationsRepository.UpdateAsync(app);
            return Ok();
        }

        [HttpDelete]
        [Route("deleteTogglesFromHistory")]
        public async Task<IActionResult> DeleteTogglesFromHistory([FromBody] DeleteTogglesFromHistoryModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var app = await _applicationsRepository.FindByIdAsync(model.ApplicationId);
            app.DeleteToggleFromHistory(model.ToggleIds);

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
                app.AddDeployEnvironment(environmentModel.EnvName, environmentModel.DefaultToggleValue, environmentModel.RequireReasonToChangeWhenToggleEnabled, environmentModel.RequireReasonToChangeWhenToggleDisabled, environmentModel.SortOrder);
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
            var toggleSchedulers = await _toggleScheduleRepository.GetAllAsync();
            foreach (var fts in toggleSchedulers)
            {
                fts.RemoveEnvironment(environmentModel.EnvName);
                await _toggleScheduleRepository.UpdateAsync(fts);
                if (fts.Environments.Count == 0)
                {
                    await _toggleScheduleRepository.DeleteAsync(fts);
                }
            }

            foreach (var featureToggle in app.FeatureToggles)
            {
                featureToggle.RemoveEnvironmentFromReasonToChange(environmentModel.EnvName);
            }
            foreach (var featureToggle in app.FeatureToggles)
            {
                featureToggle.RemoveReasonToChangeWithNoEnvironments();
            }

            app.DeleteDeployEnvironment(environmentModel.EnvName);

            await _applicationsRepository.UpdateAsync(app);
            return Ok();
        }

        [HttpPut]
        [Route("MoveEnvironment")]
        public async Task<IActionResult> MoveEnvironment([FromBody]MoveEnvironmentModel environmentModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var app = await _applicationsRepository.FindByIdAsync(environmentModel.ApplicationId);
            try
            {
                app.ChangeEnvironmentPosition(environmentModel.EnvName, environmentModel.MoveToLeft, environmentModel.MoveToRight);
            }

            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(ex.Message);
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
            var featureTogglesSchedulers = await _toggleScheduleRepository.GetAllAsync();
            try
            {
                app.ChangeEnvironmentValuesToRequireReasonFor(environmentModel.InitialEnvName, environmentModel.RequireReasonForChangeWhenToggleEnabled, environmentModel.RequireReasonForChangeWhenToggleDisabled);
                app.ChangeEnvironmentValuesToRequireReasonFor(environmentModel.InitialEnvName, environmentModel.RequireReasonForChangeWhenToggleEnabled, environmentModel.RequireReasonForChangeWhenToggleDisabled);
                app.ChangeDeployEnvironmentName(environmentModel.InitialEnvName, environmentModel.NewEnvName);
                app.ChangeEnvironmentDefaultValue(environmentModel.NewEnvName, environmentModel.DefaultToggleValue);
                foreach (var fts in featureTogglesSchedulers)
                {
                    fts.ChangeEnvironmentName(environmentModel.InitialEnvName, environmentModel.NewEnvName);
                    await _toggleScheduleRepository.UpdateAsync(fts);
                }
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(ex.Message);
            }

            await _applicationsRepository.UpdateAsync(app);
            return Ok();
        }

        [HttpGet]
        [Route("deletedFeatureToggles")]
        public async Task<IActionResult> GetDeletedFeatureToggles(Guid applicationId)
        {
            var app = await _applicationsRepository.FindByIdAsync(applicationId);
            return Ok(app.DeletedFeatureToggles.OrderByDescending(x => x.DeletionDate));
        }
    }
}