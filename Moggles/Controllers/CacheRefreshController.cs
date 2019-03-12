using System;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moggles.Models;
using Moggles.Data;
using MogglesContracts;

namespace Moggles.Controllers
{
    [Produces("application/json")]
    [Route("api/CacheRefresh")]
    public class CacheRefreshController : Controller
    {
        private readonly IBus _bus;
        private readonly TogglesContext _db;
        private readonly IConfiguration _configuration;

        public CacheRefreshController(TogglesContext db, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _db = db;
            _configuration = configuration;
            _bus = (IBus)serviceProvider.GetService(typeof(IBus));
        }

        [HttpPost]
        [Route("")]
        public IActionResult RefreshCache([FromBody]RefreshCacheModel refreshCacheModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var app = _db.Applications.Find(refreshCacheModel.ApplicationId);
            if (app == null)
                throw new InvalidOperationException("Application ID is invalid");

            _bus.Publish(new RefreshTogglesCache
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