using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moggles.Models;
using Moggles;

namespace Moggles.Controllers
{
    [Authorize(Policy = "OnlyAdmins")]
    public class HomeController : Controller
    {
        [CustomExceptionFilter]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
