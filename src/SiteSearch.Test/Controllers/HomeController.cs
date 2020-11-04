using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SiteSearch.Core.Models;
using SiteSearch.Test.Models;
using System.Diagnostics;

namespace SiteSearch.Test.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Search");
        }

        [Route("search")]
        public IActionResult Search()
        {
            return View((SearchResult<SearchItem>)Request.HttpContext.Items["_search_result"]);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
