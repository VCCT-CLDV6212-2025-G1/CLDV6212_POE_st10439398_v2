//-----------------------Start of File-----------------------//
using CLDV6212_POE_st10439398.Models;
using CLDV6212_POE_st10439398.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CLDV6212_POE_st10439398.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly IQueueService _queueService;
        private readonly ITableService _tableService;
        private readonly ILogger<OrderController> _logger;
        private readonly IOrderService _orderService; 

        public OrderController(
            IQueueService queueService,
            ITableService tableService,
            ILogger<OrderController> logger,
            IOrderService orderService) 
        {
            _queueService = queueService;
            _tableService = tableService;
            _logger = logger;
            _orderService = orderService; 
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

                        _logger.LogInformation("Creating order {OrderId} for customer {CustomerName}", order.OrderId, order.CustomerName);

                        // Step 1: Save to Table Storage
                        var tableSaveSuccess = await _tableService.SaveOrderAsync(order);
                        if (!tableSaveSuccess)
                        {
                            _logger.LogError("Failed to save order {OrderId} to table storage", order.OrderId);
                            TempData["ErrorMessage"] = "Failed to create order in table storage.";
                            viewModel.Customers = (await _tableService.GetAllCustomersAsync()).ToList();
                            viewModel.Products = (await _tableService.GetAllProductsAsync()).ToList();
                            return View(viewModel);
                        }

                        // Step 2: Send to Queue for processing
                        var queueSendSuccess = await _queueService.SendOrderMessageAsync(order);
                        if (!queueSendSuccess)
                        {
                            _logger.LogError("Failed to send order {OrderId} to queue", order.OrderId);
                            TempData["ErrorMessage"] = "Failed to queue order for processing.";
                            viewModel.Customers = (await _tableService.GetAllCustomersAsync()).ToList();
                            viewModel.Products = (await _tableService.GetAllProductsAsync()).ToList();
                            return View(viewModel);
                        }

                        _logger.LogInformation("Order {OrderId} created successfully and queued for processing", order.OrderId);
                        TempData["SuccessMessage"] = $"Order {order.OrderId} created and queued for processing!";
                        return RedirectToAction(nameof(Queue));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid customer or product.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating order");
                    TempData["ErrorMessage"] = $"Error creating order: {ex.Message}";
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
                _logger.LogInformation("ProcessNext called");
                var order = await _queueService.ReceiveOrderMessageAsync();
                if (order != null)
                {
                    _logger.LogInformation("Processing order {OrderId}", order.OrderId);
                    order.Status = "Processing";
                    await _tableService.UpdateOrderAsync(order);
                    
                    _logger.LogInformation("Updating inventory for product {ProductId}, quantity: {Quantity}", order.ProductId, -order.Quantity);
                    await _queueService.ProcessInventoryUpdateAsync(order.ProductId, -order.Quantity, "SALE", $"Order {order.OrderId}");
                    
                    await Task.Delay(1000);
                    
                    order.Status = "Completed";
                    await _tableService.UpdateOrderAsync(order);
                    
                    _logger.LogInformation("Order {OrderId} completed successfully", order.OrderId);
                    TempData["SuccessMessage"] = $"Order {order.OrderId} processed successfully!";
                }
                else
                {
                    _logger.LogInformation("No orders in queue to process");
                    TempData["InfoMessage"] = "No orders in queue to process.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order");
                TempData["ErrorMessage"] = $"Failed to process order: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
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

        // POST: Order/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, string newStatus)
        {
            try
            {
                _logger.LogInformation("UpdateStatus called for orderId: {OrderId}, newStatus: {NewStatus}", orderId, newStatus);
                
                var success = await _orderService.UpdateOrderStatusAsync(orderId, newStatus);
                
                if (success)
                {
                    _logger.LogInformation("Order {OrderId} status updated successfully to {Status}", orderId, newStatus);
                    TempData["SuccessMessage"] = $"Order #{orderId} status updated to {newStatus}!";
                }
                else
                {
                    _logger.LogWarning("Order {OrderId} status update failed", orderId);
                    TempData["ErrorMessage"] = "Failed to update order status.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for orderId {OrderId}", orderId);
                TempData["ErrorMessage"] = "An error occurred while updating the order.";
            }
            
            // Redirect back to SqlOrders if it's an SQL order, otherwise back to Index
            return RedirectToAction(nameof(SqlOrders));
        }

        // GET: Order/SqlOrders
        public async Task<IActionResult> SqlOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                return View("SqlOrders", orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading SQL orders");
                TempData["ErrorMessage"] = "Failed to load orders.";
                return View(new List<CLDV6212_POE_st10439398.Models.Order>());
            }
        }
    }
}
//-----------------------End Of File----------------//