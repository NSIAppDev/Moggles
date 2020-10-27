using Microsoft.AspNetCore.Mvc;
using Moggles.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace Moggles.Controllers
{
    [Produces("application/json")]
    [Route("api/ResetEnvironmentsSortOrder")]
    public class ResetEnvironmentsSortOrderController :Controller
    {
        private readonly IRepository<Application> _applicationsRepository;

        public ResetEnvironmentsSortOrderController(IRepository<Application> applicationRepository)
        {
            _applicationsRepository = applicationRepository;
        }

        [HttpGet("")]
        public async Task<IActionResult> ResetSortOrderForEnvironments()
        {
            var applications = await _applicationsRepository.GetAllAsync();

            foreach(var application in applications)
            {
                await ResetEnvironmentsSortOrderForApplication(application);
            }

            return Ok("SortOrder for environments has been changed!");
        }

        private async Task ResetEnvironmentsSortOrderForApplication(Application application)
        {
            var environments = application.DeploymentEnvironments.OrderBy(env => env.SortOrder);
            int sortOrder = 0;

            foreach(var env in environments)
            {
                env.SortOrder = sortOrder;
                sortOrder++;
            }

            await _applicationsRepository.UpdateAsync(application);
        }
    }
}
