//-----------------------Start of File-----------------------//
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CLDV6212_POE_st10439398.Models;
using CLDV6212_POE_st10439398.Models.ViewModels;
using CLDV6212_POE_st10439398.Services.Interfaces;
using System.Security.Claims;

namespace CLDV6212_POE_st10439398.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerAreaController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly ITableService _tableService;
        private readonly ILogger<CustomerAreaController> _logger;

        public CustomerAreaController(
            IAuthService authService,
            ICartService cartService,
            IOrderService orderService,
            ITableService tableService,
            ILogger<CustomerAreaController> logger)
        {
            _authService = authService;
            _cartService = cartService;
            _orderService = orderService;
            _tableService = tableService;
            _logger = logger;
        }

        // Helper method to get current user ID
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim);
        }

        // GET: CustomerArea/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _authService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get all available products from Azure Table Storage
                var products = await _tableService.GetAllProductsAsync();
                var availableProducts = products.Where(p => p.IsAvailable && p.StockQuantity > 0).ToList();

                // Get cart item count
                var cartItemCount = await _cartService.GetCartItemCountAsync(userId);

                // Get recent orders
                var orders = await _orderService.GetUserOrdersAsync(userId);

                var viewModel = new CustomerDashboardViewModel
                {
                    User = user,
                    AvailableProducts = availableProducts,
                    CartItemCount = cartItemCount,
                    RecentOrders = orders.Take(5).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer dashboard");
                TempData["ErrorMessage"] = "Failed to load dashboard. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: CustomerArea/AddToCart - FIXED VERSION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(string productId, string productName, decimal unitPrice, int quantity = 1)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _cartService.AddToCartAsync(userId, productId, productName, unitPrice, quantity);

                if (success)
                {
                    TempData["SuccessMessage"] = $"{productName} added to cart!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to add to cart.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart");
                TempData["ErrorMessage"] = "An error occurred while adding to cart.";
            }

            // FIXED: Redirect back to dashboard instead of returning JSON
            return RedirectToAction(nameof(Dashboard));
        }

        // GET: CustomerArea/Cart
        public async Task<IActionResult> Cart()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _authService.GetUserByIdAsync(userId);
                var cart = await _cartService.GetOrCreateCartAsync(userId);
                var cartItems = await _cartService.GetCartItemsAsync(cart.CartId);
                var totalAmount = await _cartService.GetCartTotalAsync(cart.CartId);

                var viewModel = new CartViewModel
                {
                    Cart = cart,
                    CartItems = cartItems,
                    TotalAmount = totalAmount,
                    TotalItems = cartItems.Sum(ci => ci.Quantity),
                    UserEmail = user.Email,
                    UserFullName = user.FullName
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart");
                TempData["ErrorMessage"] = "Failed to load cart.";
                return RedirectToAction(nameof(Dashboard));
            }
        }

        // POST: CustomerArea/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            try
            {
                var success = await _cartService.UpdateCartItemQuantityAsync(cartItemId, quantity);

                if (success)
                {
                    TempData["SuccessMessage"] = "Cart updated!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update cart.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart quantity");
                TempData["ErrorMessage"] = "An error occurred.";
            }

            return RedirectToAction(nameof(Cart));
        }

        // POST: CustomerArea/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            try
            {
                var success = await _cartService.RemoveFromCartAsync(cartItemId);

                if (success)
                {
                    TempData["SuccessMessage"] = "Item removed from cart.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to remove item.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from cart");
                TempData["ErrorMessage"] = "An error occurred.";
            }

            return RedirectToAction(nameof(Cart));
        }

        // GET: CustomerArea/Checkout
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _authService.GetUserByIdAsync(userId);
                var cart = await _cartService.GetOrCreateCartAsync(userId);
                var cartItems = await _cartService.GetCartItemsAsync(cart.CartId);

                if (!cartItems.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty!";
                    return RedirectToAction(nameof(Cart));
                }

                var totalAmount = await _cartService.GetCartTotalAsync(cart.CartId);

                var viewModel = new CheckoutViewModel
                {
                    Cart = cart,
                    CartItems = cartItems,
                    TotalAmount = totalAmount,
                    UserFullName = user.FullName,
                    UserEmail = user.Email,
                    ShippingAddress = user.Address ?? ""
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading checkout");
                TempData["ErrorMessage"] = "Failed to load checkout.";
                return RedirectToAction(nameof(Cart));
            }
        }

        // POST: CustomerArea/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            _logger.LogInformation("Checkout POST started. ModelState valid: {IsValid}", ModelState.IsValid);

            // Remove Cart and CartItems from ModelState validation since they're not posted from the form
            ModelState.Remove("Cart");
            ModelState.Remove("CartItems");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid. Errors: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
                
                // Reload cart data to display on the form
                try
                {
                    var userId = GetCurrentUserId();
                    var cart = await _cartService.GetOrCreateCartAsync(userId);
                    var user = await _authService.GetUserByIdAsync(userId);
                    model.CartItems = await _cartService.GetCartItemsAsync(cart.CartId);
                    model.TotalAmount = await _cartService.GetCartTotalAsync(cart.CartId);
                    model.UserEmail = user.Email;
                    model.UserFullName = user.FullName;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reloading cart data for invalid ModelState");
                }
                
                return View(model);
            }

            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("Checkout processing for user {UserId}", userId);
                
                var (success, orderId, message) = await _orderService.CreateOrderFromCartAsync(
                    userId,
                    model.ShippingAddress,
                    model.SpecialInstructions);

                _logger.LogInformation("CreateOrderFromCart result - Success: {Success}, OrderId: {OrderId}, Message: {Message}", 
                    success, orderId, message);

                if (success && orderId.HasValue)
                {
                    _logger.LogInformation("Order {OrderId} created successfully, redirecting to confirmation", orderId.Value);
                    TempData["SuccessMessage"] = $"Order #{orderId} placed successfully!";
                    
                    // Redirect to order confirmation with the order ID
                    return RedirectToAction(nameof(OrderConfirmation), new { id = orderId.Value });
                }
                else
                {
                    _logger.LogError("Order creation failed. Success: {Success}, OrderId: {OrderId}, Message: {Message}", 
                        success, orderId, message);
                    TempData["ErrorMessage"] = message ?? "Failed to create order. Please try again.";
                    
                    // Reload cart data and redisplay checkout
                    try
                    {
                        var cart = await _cartService.GetOrCreateCartAsync(userId);
                        var user = await _authService.GetUserByIdAsync(userId);
                        model.CartItems = await _cartService.GetCartItemsAsync(cart.CartId);
                        model.TotalAmount = await _cartService.GetCartTotalAsync(cart.CartId);
                        model.UserEmail = user.Email;
                        model.UserFullName = user.FullName;
                    }
                    catch (Exception reloadEx)
                    {
                        _logger.LogError(reloadEx, "Error reloading cart after failed order creation");
                    }
                    
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during checkout for user {UserId}. Stack trace: {StackTrace}", 
                    GetCurrentUserId(), ex.StackTrace);
                TempData["ErrorMessage"] = "An error occurred while processing your order. Please try again.";
                
                // Reload cart data and redisplay checkout
                try
                {
                    var userId = GetCurrentUserId();
                    var cart = await _cartService.GetOrCreateCartAsync(userId);
                    var user = await _authService.GetUserByIdAsync(userId);
                    model.CartItems = await _cartService.GetCartItemsAsync(cart.CartId);
                    model.TotalAmount = await _cartService.GetCartTotalAsync(cart.CartId);
                    model.UserEmail = user.Email;
                    model.UserFullName = user.FullName;
                }
                catch (Exception reloadEx)
                {
                    _logger.LogError(reloadEx, "Error reloading cart after exception");
                }
                
                return View(model);
            }
        }

        // GET: CustomerArea/OrderConfirmation
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            try
            {
                _logger.LogInformation("OrderConfirmation requested for order ID: {OrderId}", id);

                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found", id);
                    return NotFound();
                }

                // Verify this order belongs to current user
                var userId = GetCurrentUserId();
                if (order.UserId != userId)
                {
                    _logger.LogWarning("Unauthorized access to order {OrderId} by user {UserId}", id, userId);
                    return Forbid();
                }

                _logger.LogInformation("Order {OrderId} retrieved successfully. Items: {ItemCount}, Total: {Total}", 
                    id, order.OrderItems?.Count ?? 0, order.TotalAmount);

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order confirmation for order ID: {OrderId}", id);
                TempData["ErrorMessage"] = "Failed to load order confirmation.";
                return RedirectToAction(nameof(Dashboard));
            }
        }

        // GET: CustomerArea/Orders
        public async Task<IActionResult> Orders()
        {
            try
            {
                var userId = GetCurrentUserId();
                var orders = await _orderService.GetUserOrdersAsync(userId);

                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders");
                TempData["ErrorMessage"] = "Failed to load orders.";
                return RedirectToAction(nameof(Dashboard));
            }
        }

        // GET: CustomerArea/OrderDetails
        public async Task<IActionResult> OrderDetails(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                {
                    return NotFound();
                }

                // Verify this order belongs to current user
                var userId = GetCurrentUserId();
                if (order.UserId != userId)
                {
                    return Forbid();
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order details");
                return RedirectToAction(nameof(Orders));
            }
        }
    }
}
//-----------------------End of File-----------------------//