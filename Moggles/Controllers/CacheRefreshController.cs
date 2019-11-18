using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moggles.Domain;
using Moggles.Models;
using MogglesContracts;

namespace Moggles.Controllers
{
    [Produces("application/json")]
    [Route("api/CacheRefresh")]
    public class CacheRefreshController : Controller
    {
        private readonly IBus _bus;
        private readonly IConfiguration _configuration;
        private readonly IRepository<Application> _applicationsRepository;

        public CacheRefreshController(IRepository<Application> applicationsRepository, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _applicationsRepository = applicationsRepository;
            _configuration = configuration;
            _bus = (IBus)serviceProvider.GetService(typeof(IBus));
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> RefreshCache([FromBody]RefreshCacheModel refreshCacheModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var app = await _applicationsRepository.FindByIdAsync(refreshCacheModel.ApplicationId);

            if (app == null)
                throw new InvalidOperationException("Application ID is invalid");

            return Ok();
        }

        [HttpGet]
        [Route("getCacheRefreshAvailability")]
        public IActionResult GetCacheRefreshAvailability()
        {
            return Ok(bool.TryParse(_configuration.GetSection("Messaging")["UseMessaging"], out bool useMassTransitAndMessaging) && useMassTransitAndMessaging);
        }
    }
}