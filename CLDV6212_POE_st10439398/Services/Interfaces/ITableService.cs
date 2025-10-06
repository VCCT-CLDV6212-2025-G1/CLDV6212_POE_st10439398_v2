//-----------------------Start Of File----------------//
using CLDV6212_POE_st10439398.Models;

namespace CLDV6212_POE_st10439398.Services.Interfaces
{
    public interface ITableService
    {
        // Customer operations
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerAsync(string customerId);
        Task<bool> AddCustomerAsync(Customer customer);
        Task<bool> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(string customerId);

        // Product operations
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductAsync(string productId);
        Task<bool> AddProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(string productId);

        // Order operations
        Task<bool> SaveOrderAsync(OrderMessage order);
        Task<OrderMessage?> GetOrderAsync(string orderId);
        Task<IEnumerable<OrderMessage>> GetAllOrdersAsync();
        Task<bool> UpdateOrderAsync(OrderMessage order);
        Task<bool> DeleteOrderAsync(string orderId);

        // Utility operations
        Task<bool> CreateTablesIfNotExistsAsync();
    }
}
//-----------------------End Of File----------------//