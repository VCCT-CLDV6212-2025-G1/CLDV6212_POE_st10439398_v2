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

        // POST: CustomerArea/AddToCart
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
                    return Json(new { success = true, message = "Added to cart!" });
                }

                return Json(new { success = false, message = "Failed to add to cart" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart");
                return Json(new { success = false, message = "An error occurred" });
            }
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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = GetCurrentUserId();
                var (success, orderId, message) = await _orderService.CreateOrderFromCartAsync(
                    userId,
                    model.ShippingAddress,
                    model.SpecialInstructions);

                if (success)
                {
                    TempData["SuccessMessage"] = $"Order #{orderId} placed successfully!";
                    return RedirectToAction(nameof(OrderConfirmation), new { id = orderId });
                }

                TempData["ErrorMessage"] = message;
                return RedirectToAction(nameof(Cart));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing checkout");
                TempData["ErrorMessage"] = "Failed to place order. Please try again.";
                return View(model);
            }
        }

        // GET: CustomerArea/OrderConfirmation
        public async Task<IActionResult> OrderConfirmation(int id)
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
                _logger.LogError(ex, "Error loading order confirmation");
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