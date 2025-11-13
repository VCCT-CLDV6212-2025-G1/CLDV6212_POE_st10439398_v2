//-----------------------Start Of File----------------//
using CLDV6212_POE_st10439398.Models;

namespace CLDV6212_POE_st10439398.Services.Interfaces
{

    /// Interface for order service
    /// Manages order operations in SQL database

    public interface IOrderService
    {

        /// Creates an order from a cart

        Task<(bool Success, int? OrderId, string Message)> CreateOrderFromCartAsync(int userId, string? shippingAddress, string? specialInstructions);


        /// Gets an order by ID

        Task<Order?> GetOrderByIdAsync(int orderId);


        /// Gets all orders for a user

        Task<List<Order>> GetUserOrdersAsync(int userId);

        /// Gets all orders (admin)

        Task<List<Order>> GetAllOrdersAsync();


        /// Gets orders by status

        Task<List<Order>> GetOrdersByStatusAsync(string status);


        /// Updates order status

        Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus);


        /// Gets order items for an order
 
        Task<List<OrderItem>> GetOrderItemsAsync(int orderId);

   
        /// Cancels an order

        Task<bool> CancelOrderAsync(int orderId);


        /// Gets total revenue from completed orders

        Task<decimal> GetTotalRevenueAsync();


        /// Gets order count by status

        Task<int> GetOrderCountByStatusAsync(string status);
    }
}
//-----------------------End Of File----------------//