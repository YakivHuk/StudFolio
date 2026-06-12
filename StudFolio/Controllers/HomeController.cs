using Microsoft.AspNetCore.Mvc;
using StudFolio.Models;
using System.Diagnostics;

namespace StudFolio.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("Home/Error404")]
        public IActionResult Error404()
        {
            Response.StatusCode = 404;
            return View("NotFound");
        }
    }
}
