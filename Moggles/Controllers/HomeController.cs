using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moggles.Domain;
using Moggles.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Moggles.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository<Application> _applicationsRepository;

        public HomeController(IRepository<Application> applicationRepository)
        {
            _applicationsRepository = applicationRepository;
        }

        [CustomExceptionFilter]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        [Route("MigrateData")]
        public async Task<IActionResult> MigrateData()
        {
            var apps = await _applicationsRepository.GetAllAsync();

            foreach (var app in apps)
            {
                foreach (var toggle in app.FeatureToggles)
                {
                    if (toggle.UserAccepted)
                        app.UpdateFeatureToggleStatus(toggle.Id, 1);
                    else
                        app.UpdateFeatureToggleStatus(toggle.Id, 0);
                }

                await _applicationsRepository.UpdateAsync(app);
            }

            return Redirect("/");
        }

        [HttpGet]
        [Route("RevertMigration")]
        public async Task<IActionResult> RevertMigration()
        {
            var apps = await _applicationsRepository.GetAllAsync();

            foreach (var app in apps)
            {
                foreach (var toggle in app.FeatureToggles)
                {
                    switch (toggle.Status)
                    {
                        case 0:
                            app.FeatureRejectedByUser(toggle.Id);
                            break;
                        case 1:
                            app.FeatureAcceptedByUser(toggle.Id);
                            break;
                        default:
                            break;
                    }
                    app.UpdateFeatureToggleStatus(toggle.Id, null);
                    app.UpdateFeatureToggleHoldReason(toggle.Id, null);
                }

                await _applicationsRepository.UpdateAsync(app);
            }

            return Redirect("/");
        }
    }
}
