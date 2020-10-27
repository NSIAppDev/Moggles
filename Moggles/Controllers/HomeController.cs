using Microsoft.AspNetCore.Mvc;
using Moggles.Models;
using System.Diagnostics;

namespace Moggles.Controllers
{
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
