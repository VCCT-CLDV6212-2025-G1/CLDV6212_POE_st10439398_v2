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
            // Initialize with proper typed empty collections
            ViewBag.CustomerCount = 0;
            ViewBag.ProductCount = 0;
            ViewBag.QueueLength = 0;
            ViewBag.FileCount = 0;
            ViewBag.BlobCount = 0;
            ViewBag.RecentCustomers = Enumerable.Empty<Customer>();
            ViewBag.RecentProducts = Enumerable.Empty<Product>();
            ViewBag.ErrorMessage = "";

            try
            {
                // Initialize tables if they don't exist
                await _tableService.CreateTablesIfNotExistsAsync();

                // Get counts for dashboard
                var customers = await _tableService.GetAllCustomersAsync();
                var products = await _tableService.GetAllProductsAsync();
                var queueLength = await _queueService.GetQueueLengthAsync();
                var files = await _fileService.GetAllFilesAsync();
                var blobs = await _blobService.GetAllBlobsAsync();

                ViewBag.CustomerCount = customers?.Count() ?? 0;
                ViewBag.ProductCount = products?.Count() ?? 0;
                ViewBag.QueueLength = queueLength;
                ViewBag.FileCount = files?.Count() ?? 0;
                ViewBag.BlobCount = blobs?.Count() ?? 0;

                // Recent data for dashboard 
                ViewBag.RecentCustomers = customers?.OrderByDescending(c => c.CreatedDate).Take(5) ?? Enumerable.Empty<Customer>();
                ViewBag.RecentProducts = products?.OrderByDescending(p => p.CreatedDate).Take(5) ?? Enumerable.Empty<Product>();

                _logger.LogInformation("Dashboard loaded successfully. Customers: {CustomerCount}, Products: {ProductCount}",
                    (object)ViewBag.CustomerCount, (object)ViewBag.ProductCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");

                // Ensure ViewBag properties are still properly typed even on error
                ViewBag.RecentCustomers = Enumerable.Empty<Customer>();
                ViewBag.RecentProducts = Enumerable.Empty<Product>();

                ViewBag.ErrorMessage = "Unable to connect to Azure Storage services. Please check your configuration.";

                // More specific error messages based on exception type
                if (ex.Message.Contains("No connection could be made") || ex.Message.Contains("connection was refused"))
                {
                    ViewBag.ErrorMessage += " The storage service appears to be unreachable. Please verify your connection string and that Azure Storage is accessible.";
                }
                else if (ex.Message.Contains("authenticate"))
                {
                    ViewBag.ErrorMessage += " Authentication failed. Please check your storage account key in the connection string.";
                }
                else if (ex.Message.Contains("resolve service"))
                {
                    ViewBag.ErrorMessage += " Service configuration error. Please ensure all services are properly registered.";
                }
            }

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
    }
}
//-----------------------End Of File----------------//