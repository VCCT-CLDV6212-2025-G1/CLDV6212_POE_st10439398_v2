//-----------------------Start Of File----------------//
using CLDV6212_POE_st10439398.Models;
using CLDV6212_POE_st10439398.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CLDV6212_POE_st10439398.Services.Implementations
{
  /// Service for managing orders in SQL database

    public class OrderService : IOrderService
    {
        private readonly string _connectionString;
        private readonly ILogger<OrderService> _logger;
        private readonly ICartService _cartService;

        public OrderService(IConfiguration configuration, ILogger<OrderService> logger, ICartService cartService)
        {
            _connectionString = configuration.GetConnectionString("SqlConnection")
                ?? throw new ArgumentNullException("SqlConnection string not found");
            _logger = logger;
            _cartService = cartService;
        }
        /// Creates an order from user's cart

        public async Task<(bool Success, int? OrderId, string Message)> CreateOrderFromCartAsync(int userId, string? shippingAddress, string? specialInstructions)
        {
            try
            {
                // Get user's cart
                var cart = await _cartService.GetOrCreateCartAsync(userId);
                var cartItems = await _cartService.GetCartItemsAsync(cart.CartId);

                if (cartItems == null || !cartItems.Any())
                {
                    return (false, null, "Cart is empty");
                }

                // Calculate total
                var totalAmount = cartItems.Sum(item => item.Quantity * item.UnitPrice);

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Begin transaction
                using var transaction = connection.BeginTransaction();

                try
                {
                    // Create order
                    var orderQuery = @"
                        INSERT INTO Orders (UserId, OrderDate, Status, TotalAmount, ShippingAddress, SpecialInstructions)
                        VALUES (@UserId, @OrderDate, @Status, @TotalAmount, @ShippingAddress, @SpecialInstructions);
                        SELECT CAST(SCOPE_IDENTITY() as int);";

                    int orderId;
                    using (var orderCommand = new SqlCommand(orderQuery, connection, transaction))
                    {
                        orderCommand.Parameters.AddWithValue("@UserId", userId);
                        orderCommand.Parameters.AddWithValue("@OrderDate", DateTime.UtcNow);
                        orderCommand.Parameters.AddWithValue("@Status", "Pending");
                        orderCommand.Parameters.AddWithValue("@TotalAmount", totalAmount);
                        orderCommand.Parameters.AddWithValue("@ShippingAddress", (object?)shippingAddress ?? DBNull.Value);
                        orderCommand.Parameters.AddWithValue("@SpecialInstructions", (object?)specialInstructions ?? DBNull.Value);

                        orderId = (int)await orderCommand.ExecuteScalarAsync();
                    }

                    // Insert order items
                    var orderItemQuery = @"
                        INSERT INTO OrderItems (OrderId, ProductId, ProductName, Quantity, UnitPrice)
                        VALUES (@OrderId, @ProductId, @ProductName, @Quantity, @UnitPrice)";

                    foreach (var item in cartItems)
                    {
                        using var itemCommand = new SqlCommand(orderItemQuery, connection, transaction);
                        itemCommand.Parameters.AddWithValue("@OrderId", orderId);
                        itemCommand.Parameters.AddWithValue("@ProductId", item.ProductId);
                        itemCommand.Parameters.AddWithValue("@ProductName", item.ProductName);
                        itemCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                        itemCommand.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);

                        await itemCommand.ExecuteNonQueryAsync();
                    }

                    // Commit transaction
                    transaction.Commit();

                    // Clear cart after successful order creation
                    await _cartService.ClearCartAsync(cart.CartId);

                    _logger.LogInformation("Order {OrderId} created successfully for user {UserId}", orderId, userId);

                    return (true, orderId, "Order created successfully");
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for user {UserId}", userId);
                return (false, null, "An error occurred while creating the order");
            }
        }


        /// Gets order by ID with items

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT * FROM Orders WHERE OrderId = @OrderId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OrderId", orderId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var order = MapOrderFromReader(reader);
                    reader.Close();

                    // Load order items
                    order.OrderItems = await GetOrderItemsAsync(orderId);

                    return order;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId}", orderId);
                return null;
            }
        }


        /// Gets all orders for a user

        public async Task<List<Order>> GetUserOrdersAsync(int userId)
        {
            var orders = new List<Order>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT * FROM Orders WHERE UserId = @UserId ORDER BY OrderDate DESC";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    orders.Add(MapOrderFromReader(reader));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for user {UserId}", userId);
            }

            return orders;
        }


        /// Gets all orders (admin function)
 
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var orders = new List<Order>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT * FROM Orders ORDER BY OrderDate DESC";
                using var command = new SqlCommand(query, connection);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    orders.Add(MapOrderFromReader(reader));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all orders");
            }

            return orders;
        }


        /// Gets orders by status

        public async Task<List<Order>> GetOrdersByStatusAsync(string status)
        {
            var orders = new List<Order>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT * FROM Orders WHERE Status = @Status ORDER BY OrderDate DESC";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Status", status);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    orders.Add(MapOrderFromReader(reader));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders with status {Status}", status);
            }

            return orders;
        }


        /// Updates order status

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    UPDATE Orders 
                    SET Status = @Status, 
                        ProcessedDate = CASE WHEN @Status = 'PROCESSED' THEN @ProcessedDate ELSE ProcessedDate END
                    WHERE OrderId = @OrderId";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Status", newStatus);
                command.Parameters.AddWithValue("@ProcessedDate", DateTime.UtcNow);
                command.Parameters.AddWithValue("@OrderId", orderId);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, newStatus);
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId} status", orderId);
                return false;
            }
        }

        /// Gets order items for an order

        public async Task<List<OrderItem>> GetOrderItemsAsync(int orderId)
        {
            var items = new List<OrderItem>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT * FROM OrderItems WHERE OrderId = @OrderId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OrderId", orderId);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    items.Add(new OrderItem
                    {
                        OrderItemId = reader.GetInt32("OrderItemId"),
                        OrderId = reader.GetInt32("OrderId"),
                        ProductId = reader.GetString("ProductId"),
                        ProductName = reader.GetString("ProductName"),
                        Quantity = reader.GetInt32("Quantity"),
                        UnitPrice = reader.GetDecimal("UnitPrice")
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order items for order {OrderId}", orderId);
            }

            return items;
        }


        /// Cancels an order

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            return await UpdateOrderStatusAsync(orderId, "Cancelled");
        }


        /// Gets total revenue from completed/processed orders

        public async Task<decimal> GetTotalRevenueAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT ISNULL(SUM(TotalAmount), 0) FROM Orders WHERE Status IN ('Completed', 'PROCESSED')";
                using var command = new SqlCommand(query, connection);

                return (decimal)await command.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total revenue");
                return 0;
            }
        }


        /// Gets order count by status

        public async Task<int> GetOrderCountByStatusAsync(string status)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT COUNT(*) FROM Orders WHERE Status = @Status";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Status", status);

                return (int)await command.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order count for status {Status}", status);
                return 0;
            }
        }


        /// Maps SqlDataReader to Order object

        private Order MapOrderFromReader(SqlDataReader reader)
        {
            return new Order
            {
                OrderId = reader.GetInt32("OrderId"),
                UserId = reader.GetInt32("UserId"),
                OrderDate = reader.GetDateTime("OrderDate"),
                Status = reader.GetString("Status"),
                TotalAmount = reader.GetDecimal("TotalAmount"),
                ShippingAddress = reader.IsDBNull("ShippingAddress") ? null : reader.GetString("ShippingAddress"),
                SpecialInstructions = reader.IsDBNull("SpecialInstructions") ? null : reader.GetString("SpecialInstructions"),
                ProcessedDate = reader.IsDBNull("ProcessedDate") ? null : reader.GetDateTime("ProcessedDate")
            };
        }
    }
}
//-----------------------End Of File----------------//