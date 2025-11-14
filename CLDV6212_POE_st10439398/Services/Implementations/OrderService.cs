//-----------------------Start Of File----------------//
using CLDV6212_POE_st10439398.Models;
using CLDV6212_POE_st10439398.Services.Interfaces;
using Microsoft.Data.SqlClient;

namespace CLDV6212_POE_st10439398.Services.Implementations
{

    /// Order service for managing order operations

    public class OrderService : IOrderService
    {
        private readonly string _connectionString;
        private readonly ICartService _cartService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IConfiguration configuration, ICartService cartService, ILogger<OrderService> logger)
        {
            _connectionString = configuration.GetConnectionString("SqlConnection")
                ?? throw new ArgumentNullException("SqlConnection string not found");
            _cartService = cartService;
            _logger = logger;
        }

  
        /// Creates an order from cart
   
        public async Task<(bool Success, int? OrderId, string Message)> CreateOrderFromCartAsync(int userId, string? shippingAddress, string? specialInstructions)
        {
            try
            {
                var cart = await _cartService.GetOrCreateCartAsync(userId);
                var cartItems = await _cartService.GetCartItemsAsync(cart.CartId);

                if (!cartItems.Any())
                {
                    return (false, null, "Cart is empty");
                }

                var totalAmount = await _cartService.GetCartTotalAsync(cart.CartId);

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

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

                        var result = await orderCommand.ExecuteScalarAsync();
                        if (result == null || result == DBNull.Value)
                        {
                            throw new InvalidOperationException("Failed to retrieve new OrderId after insert.");
                        }

                        orderId = Convert.ToInt32(result);
                    }

                    _logger.LogInformation("Order {OrderId} inserted successfully", orderId);

                    // Create order items
                    foreach (var item in cartItems)
                    {
                        var itemQuery = @"
                            INSERT INTO OrderItems (OrderId, ProductId, ProductName, Quantity, UnitPrice)
                            VALUES (@OrderId, @ProductId, @ProductName, @Quantity, @UnitPrice)";

                        using var itemCommand = new SqlCommand(itemQuery, connection, transaction);
                        itemCommand.Parameters.AddWithValue("@OrderId", orderId);
                        itemCommand.Parameters.AddWithValue("@ProductId", item.ProductId);
                        itemCommand.Parameters.AddWithValue("@ProductName", item.ProductName);
                        itemCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                        itemCommand.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);

                        await itemCommand.ExecuteNonQueryAsync();
                    }

                    _logger.LogInformation("Order {OrderId} items inserted: {ItemCount}", orderId, cartItems.Count);

                    // Clear cart items within the same transaction to ensure atomicity
                    using (var deleteCommand = new SqlCommand("DELETE FROM CartItems WHERE CartId = @CartId", connection, transaction))
                    {
                        deleteCommand.Parameters.AddWithValue("@CartId", cart.CartId);
                        await deleteCommand.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();

                    _logger.LogInformation("Order {OrderId} transaction committed successfully. OrderId: {OrderIdValue}", orderId, orderId);

                    return (true, orderId, "Order placed successfully");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Error during order creation transaction. Rolling back.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order from cart for user {UserId}", userId);
                return (false, null, $"Failed to create order: {ex.Message}");
            }
        }


        /// Gets order by ID

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT o.*, u.Email, u.FirstName, u.LastName, u.Phone
                    FROM Orders o
                    INNER JOIN Users u ON o.UserId = u.UserId
                    WHERE o.OrderId = @OrderId";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OrderId", orderId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var order = MapOrderFromReader(reader);
                    reader.Close();

                    // Get order items
                    order.OrderItems = await GetOrderItemsAsync(orderId);

                    return order;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId}", orderId);
                return null;
            }
        }


        /// Gets orders for a user
 
        public async Task<List<Order>> GetUserOrdersAsync(int userId)
        {
            var orders = new List<Order>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT o.*, u.Email, u.FirstName, u.LastName, u.Phone
                    FROM Orders o
                    INNER JOIN Users u ON o.UserId = u.UserId
                    WHERE o.UserId = @UserId
                    ORDER BY o.OrderDate DESC";

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
                _logger.LogError(ex, "Error getting orders for user {UserId}", userId);
            }

            return orders;
        }

  
        /// Gets all orders
    
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var orders = new List<Order>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT o.*, u.Email, u.FirstName, u.LastName, u.Phone
                    FROM Orders o
                    INNER JOIN Users u ON o.UserId = u.UserId
                    ORDER BY o.OrderDate DESC";

                using var command = new SqlCommand(query, connection);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    orders.Add(MapOrderFromReader(reader));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all orders");
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

                var query = @"
                    SELECT o.*, u.Email, u.FirstName, u.LastName, u.Phone
                    FROM Orders o
                    INNER JOIN Users u ON o.UserId = u.UserId
                    WHERE o.Status = @Status
                    ORDER BY o.OrderDate DESC";

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
                _logger.LogError(ex, "Error getting orders by status");
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
                _logger.LogError(ex, "Error updating order status");
                return false;
            }
        }

          /// Gets order items

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
                        OrderItemId = reader.GetInt32(reader.GetOrdinal("OrderItemId")),
                        OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                        ProductId = reader.GetString(reader.GetOrdinal("ProductId")),
                        ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                        Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                        UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice"))
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order items");
            }

            return items;
        }

      /// Cancels an order

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            return await UpdateOrderStatusAsync(orderId, "Cancelled");
        }


        /// Gets total revenue

        public async Task<decimal> GetTotalRevenueAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT ISNULL(SUM(TotalAmount), 0) FROM Orders WHERE Status IN ('Completed', 'PROCESSED')";
                using var command = new SqlCommand(query, connection);

                var total = (decimal)await command.ExecuteScalarAsync();
                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total revenue");
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

                var count = (int)await command.ExecuteScalarAsync();
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order count");
                return 0;
            }
        }


        /// Maps order from reader

        private Order MapOrderFromReader(SqlDataReader reader)
        {
            return new Order
            {
                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                ShippingAddress = reader.IsDBNull(reader.GetOrdinal("ShippingAddress")) ? null : reader.GetString(reader.GetOrdinal("ShippingAddress")),
                SpecialInstructions = reader.IsDBNull(reader.GetOrdinal("SpecialInstructions")) ? null : reader.GetString(reader.GetOrdinal("SpecialInstructions")),
                ProcessedDate = reader.IsDBNull(reader.GetOrdinal("ProcessedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ProcessedDate")),
                User = new User
                {
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone"))
                }
            };
        }
    }
}
//-----------------------End Of File----------------//