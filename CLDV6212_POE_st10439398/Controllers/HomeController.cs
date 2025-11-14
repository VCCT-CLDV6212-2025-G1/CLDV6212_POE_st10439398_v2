//-----------------------Start of File----------------------//
using CLDV6212_POE_st10439398.Models;
using CLDV6212_POE_st10439398.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CLDV6212_POE_st10439398.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITableService _tableService;
        private readonly IBlobService _blobService;
        private readonly IQueueService _queueService;
        private readonly IFileService _fileService;

        public HomeController(
            ILogger<HomeController> logger,
            ITableService tableService,
            IBlobService blobService,
            IQueueService queueService,
            IFileService fileService)
        {
            _logger = logger;
            _tableService = tableService;
            _blobService = blobService;
            _queueService = queueService;
            _fileService = fileService;
        }

        public IActionResult Index()
        {
            // If user is already logged in, redirect to their dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Home"); // Or admin dashboard
                }
                else if (User.IsInRole("Customer"))
                {
                    return RedirectToAction("Dashboard", "CustomerArea");
                }
            }

            // If not logged in, redirect to login page
            return RedirectToAction("Login", "Account");
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
    }
}
//-----------------------End Of File----------------//