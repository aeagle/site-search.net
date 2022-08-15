using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SiteSearch.Core.Models;
using SiteSearch.Test.Models;
using System;
using System.Diagnostics;

namespace SiteSearch.Test.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly SearchContext searchContext;

        public HomeController(
            ILogger<HomeController> logger,
            SearchContext searchContext)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(searchContext));
            this.searchContext = searchContext ?? throw new ArgumentNullException(nameof(searchContext));
        }

        public IActionResult Index()
        {
            return RedirectToAction("Search");
        }

        [Route("search")]
        public IActionResult Search()
        {
            return View(searchContext.Get<SearchItem>());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
