//-----------------------Start Of File----------------//
using CLDV6212_POE_st10439398.Models;

namespace CLDV6212_POE_st10439398.Services.Interfaces { 

    /// Interface for cart service
    /// Manages shopping cart operations for customers

    public interface ICartService
    {

        /// Gets or creates a cart for a user

        Task<Cart> GetOrCreateCartAsync(int userId);


        /// Gets cart by cart ID

        Task<Cart?> GetCartByIdAsync(int cartId);


        /// Adds a product to the cart

        Task<bool> AddToCartAsync(int userId, string productId, string productName, decimal unitPrice, int quantity = 1);


        /// Updates quantity of a cart item

        Task<bool> UpdateCartItemQuantityAsync(int cartItemId, int quantity);


        /// Removes an item from the cart

        Task<bool> RemoveFromCartAsync(int cartItemId);


        /// Gets all items in a cart

        Task<List<CartItem>> GetCartItemsAsync(int cartId);


        /// Clears all items from a cart

        Task<bool> ClearCartAsync(int cartId);


        /// Gets cart item count for a user

        Task<int> GetCartItemCountAsync(int userId);

 
        /// Gets cart total amount

        Task<decimal> GetCartTotalAsync(int cartId);
    }
}
//-----------------------End Of File----------------//