//-----------------------Start of File-----------------------//
using Microsoft.AspNetCore.Mvc;
using CLDV6212_POE_st10439398.Models;
using CLDV6212_POE_st10439398.Services.Interfaces;

namespace CLDV6212_POE_st10439398.Controllers
{
    public class OrderController : Controller
    {
        private readonly IQueueService _queueService;
        private readonly ITableService _tableService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IQueueService queueService, ITableService tableService, ILogger<OrderController> logger)
        {
            _queueService = queueService;
            _tableService = tableService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var orders = await _tableService.GetAllOrdersAsync();
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders");
                TempData["ErrorMessage"] = "Failed to load orders.";
                return View(Enumerable.Empty<OrderMessage>());
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var viewModel = new OrderViewModel
                {
                    Customers = (await _tableService.GetAllCustomersAsync()).ToList(),
                    Products = (await _tableService.GetAllProductsAsync()).ToList()
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading form");
                TempData["ErrorMessage"] = "Failed to load form.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var customer = await _tableService.GetCustomerAsync(viewModel.CustomerId);
                    var product = await _tableService.GetProductAsync(viewModel.ProductId);

                    if (customer != null && product != null)
                    {
                        var order = new OrderMessage
                        {
                            CustomerId = customer.CustomerId,
                            CustomerName = customer.FullName,
                            ProductId = product.ProductId,
                            ProductName = product.Name,
                            Quantity = viewModel.Quantity,
                            UnitPrice = product.Price,
                            SpecialInstructions = viewModel.SpecialInstructions
                        };

                        var tableSaveSuccess = await _tableService.SaveOrderAsync(order);
                        var queueSendSuccess = await _queueService.SendOrderMessageAsync(order);

                        if (tableSaveSuccess)
                        {
                            TempData["SuccessMessage"] = $"Order {order.OrderId} created!";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Failed to create order.";
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid customer or product.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating order");
                    TempData["ErrorMessage"] = "Error creating order.";
                }
            }

            viewModel.Customers = (await _tableService.GetAllCustomersAsync()).ToList();
            viewModel.Products = (await _tableService.GetAllProductsAsync()).ToList();
            return View(viewModel);
        }

        public async Task<IActionResult> Queue()
        {
            try
            {
                var queueLength = await _queueService.GetQueueLengthAsync();
                var orders = await _queueService.PeekOrderMessagesAsync(10);
                ViewBag.QueueLength = queueLength;
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading queue");
                TempData["ErrorMessage"] = "Failed to load queue.";
                return View(Enumerable.Empty<OrderMessage>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessNext()
        {
            try
            {
                var order = await _queueService.ReceiveOrderMessageAsync();
                if (order != null)
                {
                    order.Status = "Processing";
                    await _tableService.UpdateOrderAsync(order);
                    await _queueService.ProcessInventoryUpdateAsync(order.ProductId, -order.Quantity, "SALE", $"Order {order.OrderId}");
                    await Task.Delay(1000);
                    order.Status = "Completed";
                    await _tableService.UpdateOrderAsync(order);
                    TempData["SuccessMessage"] = $"Order {order.OrderId} processed!";
                }
                else
                {
                    TempData["InfoMessage"] = "No orders in queue.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order");
                TempData["ErrorMessage"] = "Failed to process order.";
            }
            return RedirectToAction(nameof(Queue));
        }

        [HttpPost]
        public async Task<IActionResult> ClearQueue()
        {
            try
            {
                var success = await _queueService.ClearQueueAsync();
                TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Queue cleared!" : "Failed to clear queue.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing queue");
                TempData["ErrorMessage"] = "Failed to clear queue.";
            }
            return RedirectToAction(nameof(Queue));
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var order = await _tableService.GetOrderAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }
    }
}
//-----------------------End Of File----------------//