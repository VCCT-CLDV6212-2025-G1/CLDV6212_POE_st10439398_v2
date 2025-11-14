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

        public async Task<IActionResult> Index()
        {
            // If user is NOT authenticated, redirect to login
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account");
            }

            // User IS authenticated - show them the appropriate dashboard
            if (User.IsInRole("Customer"))
            {
                // Customer goes to their shopping dashboard
                return RedirectToAction("Dashboard", "CustomerArea");
            }
            else if (User.IsInRole("Admin"))
            {
                // Admin sees the Home/Index view with stats
                try
                {
                    // Get stats for admin dashboard
                    var customerCount = (await _tableService.GetAllCustomersAsync()).Count();
                    var productCount = (await _tableService.GetAllProductsAsync()).Count();
                    var orderCount = (await _tableService.GetAllOrdersAsync()).Count();
                    var queueLength = await _queueService.GetQueueLengthAsync();

                    ViewBag.CustomerCount = customerCount;
                    ViewBag.ProductCount = productCount;
                    ViewBag.OrderCount = orderCount;
                    ViewBag.QueueLength = queueLength;

                    return View(); // Show admin dashboard
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading admin dashboard");
                    ViewBag.CustomerCount = 0;
                    ViewBag.ProductCount = 0;
                    ViewBag.OrderCount = 0;
                    ViewBag.QueueLength = 0;
                    return View();
                }
            }

            // Fallback: Unknown role - redirect to login
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