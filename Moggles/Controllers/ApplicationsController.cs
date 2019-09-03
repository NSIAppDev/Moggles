using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Internal.Account.Manage;
using Microsoft.AspNetCore.Mvc;
using Moggles.Models;
using Moggles.Data;
using Moggles.Domain;
using Moggles.Repository;

namespace Moggles.Controllers
{
    [Produces("application/json")]
    [Route("api/Applications")]
    public class ApplicationsController : Controller
    {
        private IRepository<Application> _applicationsRepository;
        private IRepository<DeployEnvironment> _deployEnvironmentRepository;
        private IRepository<FeatureToggle> _featureToggleRepository;
        private IRepository<FeatureToggleStatus> _featureToggleStatusRepository;

        public ApplicationsController(IRepository<Application> applicationsRepository, IRepository<DeployEnvironment> deployEnvironmentRepository, IRepository<FeatureToggle> featureToggleRepository, IRepository<FeatureToggleStatus> featureToggleStatusRepository)
        {
            _applicationsRepository = applicationsRepository;
            _deployEnvironmentRepository = deployEnvironmentRepository;
            _featureToggleRepository = featureToggleRepository;
            _featureToggleStatusRepository = featureToggleStatusRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllApplications()

        {
            return Ok(_applicationsRepository.GetAll().Result.ToList());
        }


        [HttpPost]
        [Route("add")]
        public IActionResult AddApplication([FromBody]AddApplicationModel applicationModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var apps = _applicationsRepository.GetAll().Result.ToList();
            var app = apps.FirstOrDefault(a => a.AppName == applicationModel.ApplicationName);
            
            if (app != null)
                throw new InvalidOperationException("Application already exists!");

            var application = new Application
            {
                Id = Guid.NewGuid(),
                AppName = applicationModel.ApplicationName
            };

            _applicationsRepository.Add(application);

            var deployenv = new DeployEnvironment
            {
                Id = Guid.NewGuid(),
                EnvName = applicationModel.EnvironmentName,
                ApplicationId = application.Id,
                DefaultToggleValue = applicationModel.DefaultToggleValue,
                SortOrder = 1
            };

            _deployEnvironmentRepository.Add(deployenv);

            return Ok(application);
        }

        [HttpPut]
        [Route("update")]
        public IActionResult UpdateApplication([FromBody]UpdateApplicationModel applicationModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var app = _applicationsRepository.FindById(applicationModel.Id).Result;


            if (app == null)
                throw new InvalidOperationException("Application does not exists!");
            app.AppName = applicationModel.ApplicationName; // Make the change
            _applicationsRepository.Update(app);

            return Ok();
        }

        [HttpDelete]
        public IActionResult RemoveApp([FromQuery] Guid id)
        {
            var app = _applicationsRepository.FindById(id).Result;

            if (app == null)
                throw new InvalidOperationException("Application does not exists!");

            var allDeployEnvironments = _deployEnvironmentRepository.GetAll().Result;
            var appSpecificDeployEnvironments = allDeployEnvironments.Where(e => e.ApplicationId == id).ToList();
            var appSpecificDeployEnvironmentIds = appSpecificDeployEnvironments.Select(_ => _.Id);
            foreach (var deployEnvironment in appSpecificDeployEnvironments)
            {
                _deployEnvironmentRepository.Delete(deployEnvironment);
            }
            var allFeatureToggles = _featureToggleRepository.GetAll().Result;
            var appSpecificFeatureToggles = allFeatureToggles.Where(e => e.ApplicationId == id).ToList();
            foreach (var featureToggle in appSpecificFeatureToggles)
            {
                _featureToggleRepository.Delete(featureToggle);
            }

            var allFeatureToggleStatus = _featureToggleStatusRepository.GetAll().Result;
            var appSpecificFeatureToggleStatus = allFeatureToggleStatus.Where(_ => appSpecificDeployEnvironmentIds.Contains(_.EnvironmentId));
            foreach (var featureToggleStatus in appSpecificFeatureToggleStatus)
            {
                _featureToggleStatusRepository.Delete(featureToggleStatus);
            }

            _applicationsRepository.Delete(app);


            return Ok();
        }

    }
}