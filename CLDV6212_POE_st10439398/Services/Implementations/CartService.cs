//-----------------------Start Of File----------------//
using CLDV6212_POE_st10439398.Models;
using CLDV6212_POE_st10439398.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CLDV6212_POE_st10439398.Services.Implementations
{

    /// Service for managing shopping cart operations

    public class CartService : ICartService
    {
        private readonly string _connectionString;
        private readonly ILogger<CartService> _logger;

        public CartService(IConfiguration configuration, ILogger<CartService> logger)
        {
            _connectionString = configuration.GetConnectionString("SqlConnection")
                ?? throw new ArgumentNullException("SqlConnection string not found");
            _logger = logger;
        }


        /// Gets or creates a cart for a user

        public async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Try to get existing cart
                var query = "SELECT * FROM Carts WHERE UserId = @UserId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Cart
                    {
                        CartId = reader.GetInt32("CartId"),
                        UserId = reader.GetInt32("UserId"),
                        CreatedDate = reader.GetDateTime("CreatedDate"),
                        LastModifiedDate = reader.GetDateTime("LastModifiedDate")
                    };
                }
                reader.Close();

                // Create new cart if doesn't exist
                var insertQuery = @"
                    INSERT INTO Carts (UserId, CreatedDate, LastModifiedDate) 
                    VALUES (@UserId, @CreatedDate, @LastModifiedDate);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                using var insertCommand = new SqlCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@UserId", userId);
                insertCommand.Parameters.AddWithValue("@CreatedDate", DateTime.UtcNow);
                insertCommand.Parameters.AddWithValue("@LastModifiedDate", DateTime.UtcNow);

                var cartId = (int)await insertCommand.ExecuteScalarAsync();

                _logger.LogInformation("Created new cart {CartId} for user {UserId}", cartId, userId);

                return new Cart
                {
                    CartId = cartId,
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating cart for user {UserId}", userId);
                throw;
            }
        }


        /// Gets cart by ID
 
        public async Task<Cart?> GetCartByIdAsync(int cartId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT * FROM Carts WHERE CartId = @CartId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CartId", cartId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var cart = new Cart
                    {
                        CartId = reader.GetInt32("CartId"),
                        UserId = reader.GetInt32("UserId"),
                        CreatedDate = reader.GetDateTime("CreatedDate"),
                        LastModifiedDate = reader.GetDateTime("LastModifiedDate")
                    };
                    reader.Close();

                    // Load cart items
                    cart.CartItems = await GetCartItemsAsync(cartId);
                    return cart;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart {CartId}", cartId);
                return null;
            }
        }


        /// Adds product to cart

        public async Task<bool> AddToCartAsync(int userId, string productId, string productName, decimal unitPrice, int quantity = 1)
        {
            try
            {
                var cart = await GetOrCreateCartAsync(userId);

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Check if product already in cart
                var checkQuery = "SELECT CartItemId, Quantity FROM CartItems WHERE CartId = @CartId AND ProductId = @ProductId";
                using var checkCommand = new SqlCommand(checkQuery, connection);
                checkCommand.Parameters.AddWithValue("@CartId", cart.CartId);
                checkCommand.Parameters.AddWithValue("@ProductId", productId);

                using var reader = await checkCommand.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    // Update existing item
                    var cartItemId = reader.GetInt32("CartItemId");
                    var currentQuantity = reader.GetInt32("Quantity");
                    reader.Close();

                    var updateQuery = "UPDATE CartItems SET Quantity = @Quantity WHERE CartItemId = @CartItemId";
                    using var updateCommand = new SqlCommand(updateQuery, connection);
                    updateCommand.Parameters.AddWithValue("@Quantity", currentQuantity + quantity);
                    updateCommand.Parameters.AddWithValue("@CartItemId", cartItemId);
                    await updateCommand.ExecuteNonQueryAsync();

                    _logger.LogInformation("Updated quantity for product {ProductId} in cart {CartId}", productId, cart.CartId);
                }
                else
                {
                    reader.Close();

                    // Add new item
                    var insertQuery = @"
                        INSERT INTO CartItems (CartId, ProductId, ProductName, Quantity, UnitPrice, AddedDate)
                        VALUES (@CartId, @ProductId, @ProductName, @Quantity, @UnitPrice, @AddedDate)";

                    using var insertCommand = new SqlCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@CartId", cart.CartId);
                    insertCommand.Parameters.AddWithValue("@ProductId", productId);
                    insertCommand.Parameters.AddWithValue("@ProductName", productName);
                    insertCommand.Parameters.AddWithValue("@Quantity", quantity);
                    insertCommand.Parameters.AddWithValue("@UnitPrice", unitPrice);
                    insertCommand.Parameters.AddWithValue("@AddedDate", DateTime.UtcNow);
                    await insertCommand.ExecuteNonQueryAsync();

                    _logger.LogInformation("Added product {ProductId} to cart {CartId}", productId, cart.CartId);
                }

                // Update cart last modified date
                await UpdateCartModifiedDateAsync(cart.CartId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product {ProductId} to cart for user {UserId}", productId, userId);
                return false;
            }
        }


        /// Updates cart item quantity

        public async Task<bool> UpdateCartItemQuantityAsync(int cartItemId, int quantity)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                if (quantity <= 0)
                {
                    // Remove item if quantity is 0 or less
                    return await RemoveFromCartAsync(cartItemId);
                }

                var query = "UPDATE CartItems SET Quantity = @Quantity WHERE CartItemId = @CartItemId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Quantity", quantity);
                command.Parameters.AddWithValue("@CartItemId", cartItemId);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    // Get cart ID to update modified date
                    var getCartIdQuery = "SELECT CartId FROM CartItems WHERE CartItemId = @CartItemId";
                    using var getCartIdCommand = new SqlCommand(getCartIdQuery, connection);
                    getCartIdCommand.Parameters.AddWithValue("@CartItemId", cartItemId);
                    var cartId = (int)await getCartIdCommand.ExecuteScalarAsync();

                    await UpdateCartModifiedDateAsync(cartId);
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item {CartItemId} quantity", cartItemId);
                return false;
            }
        }


        /// Removes item from cart

        public async Task<bool> RemoveFromCartAsync(int cartItemId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Get cart ID before deleting
                var getCartIdQuery = "SELECT CartId FROM CartItems WHERE CartItemId = @CartItemId";
                using var getCartIdCommand = new SqlCommand(getCartIdQuery, connection);
                getCartIdCommand.Parameters.AddWithValue("@CartItemId", cartItemId);
                var cartId = (int?)await getCartIdCommand.ExecuteScalarAsync();

                // Delete cart item
                var query = "DELETE FROM CartItems WHERE CartItemId = @CartItemId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CartItemId", cartItemId);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0 && cartId.HasValue)
                {
                    await UpdateCartModifiedDateAsync(cartId.Value);
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item {CartItemId}", cartItemId);
                return false;
            }
        }


        /// Gets all items in a cart

        public async Task<List<CartItem>> GetCartItemsAsync(int cartId)
        {
            var items = new List<CartItem>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT * FROM CartItems WHERE CartId = @CartId ORDER BY AddedDate DESC";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CartId", cartId);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    items.Add(new CartItem
                    {
                        CartItemId = reader.GetInt32("CartItemId"),
                        CartId = reader.GetInt32("CartId"),
                        ProductId = reader.GetString("ProductId"),
                        ProductName = reader.GetString("ProductName"),
                        Quantity = reader.GetInt32("Quantity"),
                        UnitPrice = reader.GetDecimal("UnitPrice"),
                        AddedDate = reader.GetDateTime("AddedDate")
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart items for cart {CartId}", cartId);
            }

            return items;
        }


        /// Clears all items from cart

        public async Task<bool> ClearCartAsync(int cartId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "DELETE FROM CartItems WHERE CartId = @CartId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CartId", cartId);

                await command.ExecuteNonQueryAsync();
                await UpdateCartModifiedDateAsync(cartId);

                _logger.LogInformation("Cleared cart {CartId}", cartId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart {CartId}", cartId);
                return false;
            }
        }


        /// Gets cart item count for a user

        public async Task<int> GetCartItemCountAsync(int userId)
        {
            try
            {
                var cart = await GetOrCreateCartAsync(userId);

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT ISNULL(SUM(Quantity), 0) FROM CartItems WHERE CartId = @CartId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CartId", cart.CartId);

                return (int)await command.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart item count for user {UserId}", userId);
                return 0;
            }
        }


        /// Gets cart total amount

        public async Task<decimal> GetCartTotalAsync(int cartId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT ISNULL(SUM(Quantity * UnitPrice), 0) FROM CartItems WHERE CartId = @CartId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CartId", cartId);

                return (decimal)await command.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart total for cart {CartId}", cartId);
                return 0;
            }
        }


        /// Updates cart last modified date

        private async Task UpdateCartModifiedDateAsync(int cartId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "UPDATE Carts SET LastModifiedDate = @LastModifiedDate WHERE CartId = @CartId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@LastModifiedDate", DateTime.UtcNow);
                command.Parameters.AddWithValue("@CartId", cartId);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart modified date for cart {CartId}", cartId);
            }
        }
    }
}
//-----------------------End Of File----------------//