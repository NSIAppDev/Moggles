using System;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moggles.Models;
using MogglesContracts;
using NoDb;
using Moggles.Domain;
using System.Linq;
using Moggles.Domain.Repository;
using Moggles.Repository;

namespace Moggles.Controllers
{
    [Produces("application/json")]
    [Route("api/CacheRefresh")]
    public class CacheRefreshController : Controller
    {
        private readonly IBus _bus;
        private readonly IConfiguration _configuration;
        private IRepository<Application> _applicationsRepository;

        public CacheRefreshController(IRepository<Application> applicationsRepository, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _applicationsRepository = applicationsRepository;
            _configuration = configuration;
            _bus = (IBus)serviceProvider.GetService(typeof(IBus));
        }

        [HttpPost]
        [Route("")]
        public IActionResult RefreshCache([FromBody]RefreshCacheModel refreshCacheModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var app = _applicationsRepository.FindById(refreshCacheModel.ApplicationId).Result;

            if (app == null)
                throw new InvalidOperationException("Application ID is invalid");

            _bus.Publish(new RefreshTogglesCache
            {
                Environment = refreshCacheModel.EnvName,
                ApplicationName = app.AppName
            });

            _bus.Publish(new NSTogglesContracts.RefreshTogglesCache
            {
                Environment = refreshCacheModel.EnvName,
                ApplicationName = app.AppName
            });

            return Ok();
        }

        [HttpGet]
        [Route("getCacheRefreshAvailability")]
        public IActionResult GetCacheRefreshAvailability()
        {
            return Ok(Boolean.TryParse(_configuration.GetSection("Messaging")["UseMessaging"], out bool useMassTransitAndMessaging) && useMassTransitAndMessaging);
        }
    }
}