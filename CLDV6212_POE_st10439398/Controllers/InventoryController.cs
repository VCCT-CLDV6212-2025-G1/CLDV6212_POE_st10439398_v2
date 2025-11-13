//-----------------------Start of File-----------------------//
using Microsoft.AspNetCore.Mvc;
using CLDV6212_POE_st10439398.Models;
using CLDV6212_POE_st10439398.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace CLDV6212_POE_st10439398.Controllers
{
    [Authorize(Roles = "Admin")]
    public class InventoryController : Controller
    {
        private readonly IQueueService _queueService;
        private readonly ITableService _tableService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(IQueueService queueService, ITableService tableService, ILogger<InventoryController> logger)
        {
            _queueService = queueService;
            _tableService = tableService;
            _logger = logger;
        }

        // GET: Inventory
        public async Task<IActionResult> Index()
        {
            try
            {
                var queueLength = await _queueService.GetInventoryQueueLengthAsync();
                var inventoryMessages = await _queueService.PeekInventoryMessagesAsync(20);

                ViewBag.QueueLength = queueLength;
                return View(inventoryMessages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inventory queue");
                TempData["ErrorMessage"] = "Failed to load inventory information.";
                return View(Enumerable.Empty<InventoryMessage>());
            }
        }

        // GET: Inventory/Adjust
        public async Task<IActionResult> Adjust()
        {
            try
            {
                var viewModel = new InventoryAdjustmentModel
                {
                    Products = (await _tableService.GetAllProductsAsync()).ToList()
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inventory adjustment form");
                TempData["ErrorMessage"] = "Failed to load inventory adjustment form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Inventory/Adjust
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adjust(InventoryAdjustmentModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var product = await _tableService.GetProductAsync(model.ProductId);
                    if (product != null)
                    {
                        var success = await _queueService.ProcessInventoryUpdateAsync(
                            model.ProductId,
                            model.QuantityChange,
                            model.AdjustmentType,
                            model.Reason ?? "Manual adjustment"
                        );

                        if (success)
                        {
                            TempData["SuccessMessage"] = $"Inventory adjustment processed! {product.Name} stock changed by {model.QuantityChange}.";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Failed to process inventory adjustment.";
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Product not found.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing inventory adjustment");
                    TempData["ErrorMessage"] = "An error occurred while processing the adjustment.";
                }
            }

            // Reload form data
            try
            {
                model.Products = (await _tableService.GetAllProductsAsync()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading form data");
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: Inventory/ProcessNext
        [HttpPost]
        public async Task<IActionResult> ProcessNext()
        {
            try
            {
                var inventoryMessage = await _queueService.ReceiveInventoryMessageAsync();
                if (inventoryMessage != null)
                {
                    TempData["SuccessMessage"] = $"Processed inventory update: {inventoryMessage.ProductName} - {inventoryMessage.OperationType} - Change: {inventoryMessage.QuantityChange}";
                }
                else
                {
                    TempData["InfoMessage"] = "No inventory updates in queue to process.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing inventory message");
                TempData["ErrorMessage"] = "Failed to process inventory update.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Inventory/ClearQueue
        [HttpPost]
        public async Task<IActionResult> ClearQueue()
        {
            try
            {
                var success = await _queueService.ClearInventoryQueueAsync();
                if (success)
                {
                    TempData["SuccessMessage"] = "Inventory queue cleared successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to clear inventory queue.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing inventory queue");
                TempData["ErrorMessage"] = "Failed to clear inventory queue.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Inventory/Restock
        public async Task<IActionResult> Restock()
        {
            try
            {
                var products = await _tableService.GetAllProductsAsync();
                var lowStockProducts = products.Where(p => p.StockQuantity < 10).ToList(); // Products with less than 10 in stock

                ViewBag.AllProducts = products.ToList();
                return View(lowStockProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading restock page");
                TempData["ErrorMessage"] = "Failed to load restock information.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Inventory/QuickRestock
        [HttpPost]
        public async Task<IActionResult> QuickRestock(string productId, int restockQuantity)
        {
            try
            {
                var product = await _tableService.GetProductAsync(productId);
                if (product != null && restockQuantity > 0)
                {
                    var success = await _queueService.ProcessInventoryUpdateAsync(
                        productId,
                        restockQuantity,
                        "RESTOCK",
                        $"Quick restock of {restockQuantity} units"
                    );

                    if (success)
                    {
                        TempData["SuccessMessage"] = $"Restocked {product.Name} with {restockQuantity} units!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to restock product.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid product or quantity.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing quick restock");
                TempData["ErrorMessage"] = "Failed to process restock.";
            }

            return RedirectToAction(nameof(Restock));
        }
    }
}
//-----------------------End Oof File----------------//