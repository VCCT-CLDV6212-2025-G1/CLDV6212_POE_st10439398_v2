//-----------------------Start Of File----------------//
using CLDV6212_POE_st10439398.Models;
using CLDV6212_POE_st10439398.Services.Interfaces;
using Microsoft.Data.SqlClient;

namespace CLDV6212_POE_st10439398.Services.Implementations
{

    /// Cart service for managing shopping cart operations

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

                // Check if cart exists
                var query = "SELECT * FROM Carts WHERE UserId = @UserId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Cart
                    {
                        CartId = reader.GetInt32(reader.GetOrdinal("CartId")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                        LastModifiedDate = reader.GetDateTime(reader.GetOrdinal("LastModifiedDate"))
                    };
                }

                reader.Close();

                // Create new cart
                var insertQuery = @"
                    INSERT INTO Carts (UserId, CreatedDate, LastModifiedDate)
                    VALUES (@UserId, @CreatedDate, @LastModifiedDate);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                using var insertCommand = new SqlCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@UserId", userId);
                insertCommand.Parameters.AddWithValue("@CreatedDate", DateTime.UtcNow);
                insertCommand.Parameters.AddWithValue("@LastModifiedDate", DateTime.UtcNow);

                var cartId = (int)await insertCommand.ExecuteScalarAsync();

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
                _logger.LogError(ex, "Error getting/creating cart for user {UserId}", userId);
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
                    return new Cart
                    {
                        CartId = reader.GetInt32(reader.GetOrdinal("CartId")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                        LastModifiedDate = reader.GetDateTime(reader.GetOrdinal("LastModifiedDate"))
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart {CartId}", cartId);
                return null;
            }
        }


        /// Adds a product to cart

        public async Task<bool> AddToCartAsync(int userId, string productId, string productName, decimal unitPrice, int quantity = 1)
        {
            try
            {
                var cart = await GetOrCreateCartAsync(userId);

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Check if item already exists in cart
                var checkQuery = "SELECT CartItemId, Quantity FROM CartItems WHERE CartId = @CartId AND ProductId = @ProductId";
                using var checkCommand = new SqlCommand(checkQuery, connection);
                checkCommand.Parameters.AddWithValue("@CartId", cart.CartId);
                checkCommand.Parameters.AddWithValue("@ProductId", productId);

                using var reader = await checkCommand.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    // Item exists, update quantity
                    var cartItemId = reader.GetInt32(0);
                    var currentQuantity = reader.GetInt32(1);
                    reader.Close();

                    var updateQuery = "UPDATE CartItems SET Quantity = @Quantity WHERE CartItemId = @CartItemId";
                    using var updateCommand = new SqlCommand(updateQuery, connection);
                    updateCommand.Parameters.AddWithValue("@Quantity", currentQuantity + quantity);
                    updateCommand.Parameters.AddWithValue("@CartItemId", cartItemId);
                    await updateCommand.ExecuteNonQueryAsync();
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
                }

                // Update cart last modified date
                await UpdateCartModifiedDateAsync(cart.CartId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart");
                return false;
            }
        }


        /// Updates cart item quantity

        public async Task<bool> UpdateCartItemQuantityAsync(int cartItemId, int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    return await RemoveFromCartAsync(cartItemId);
                }

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "UPDATE CartItems SET Quantity = @Quantity WHERE CartItemId = @CartItemId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Quantity", quantity);
                command.Parameters.AddWithValue("@CartItemId", cartItemId);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item quantity");
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

                var query = "DELETE FROM CartItems WHERE CartItemId = @CartItemId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CartItemId", cartItemId);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from cart");
                return false;
            }
        }


        /// Gets all cart items

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
                        CartItemId = reader.GetInt32(reader.GetOrdinal("CartItemId")),
                        CartId = reader.GetInt32(reader.GetOrdinal("CartId")),
                        ProductId = reader.GetString(reader.GetOrdinal("ProductId")),
                        ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                        Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                        UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                        AddedDate = reader.GetDateTime(reader.GetOrdinal("AddedDate"))
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart items");
            }

            return items;
        }

      /// Clears cart

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
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return false;
            }
        }

 
        /// Gets cart item count
 
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

                var count = (int)await command.ExecuteScalarAsync();
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart count");
                return 0;
            }
        }

    
        /// Gets cart total

        public async Task<decimal> GetCartTotalAsync(int cartId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT ISNULL(SUM(Quantity * UnitPrice), 0) FROM CartItems WHERE CartId = @CartId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CartId", cartId);

                var total = (decimal)await command.ExecuteScalarAsync();
                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart total");
                return 0;
            }
        }


        /// Updates cart modified date

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
                _logger.LogError(ex, "Error updating cart modified date");
            }
        }
    }
}
//-----------------------End Of File----------------//