using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Qadaya.Models;

namespace Qadaya.Controllers
{
    public class ErrorController : Controller
    {
        [Route("/Error")]
        public IActionResult Index()
        {
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
