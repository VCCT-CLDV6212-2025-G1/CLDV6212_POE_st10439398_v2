//-----------------------Start Of File-----------------
using Azure.Data.Tables;
using CLDV6212_POE_st10439398.Models;
using CLDV6212_POE_st10439398.Services.Interfaces;
using Microsoft.Extensions.Options;
using Azure;
using System.Globalization;

namespace CLDV6212_POE_st10439398.Services.Implementations
{
    public class TableService : ITableService
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly AzureSettings _azureSettings;

        public TableService(TableServiceClient tableServiceClient, IOptions<AzureSettings> azureSettings)
        {
            _tableServiceClient = tableServiceClient;
            _azureSettings = azureSettings.Value;
        }

        public async Task<bool> CreateTablesIfNotExistsAsync()
        {
            try
            {
                await _tableServiceClient.CreateTableIfNotExistsAsync(_azureSettings.TableName.Customers);
                await _tableServiceClient.CreateTableIfNotExistsAsync(_azureSettings.TableName.Products);
                await _tableServiceClient.CreateTableIfNotExistsAsync(_azureSettings.TableName.Orders);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ORDER OPERATIONS
        public async Task<bool> SaveOrderAsync(OrderMessage order)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(_azureSettings.TableName.Orders);
                await tableClient.CreateIfNotExistsAsync();

                // Ensure proper table entity setup
                order.PartitionKey = "Order";
                order.RowKey = order.OrderId;

                // Convert to TableEntity manually to ensure UnitPriceStorage is saved
                var entity = new TableEntity(order.PartitionKey, order.RowKey)
                {
                    ["CustomerId"] = order.CustomerId,
                    ["CustomerName"] = order.CustomerName,
                    ["ProductId"] = order.ProductId,
                    ["ProductName"] = order.ProductName,
                    ["Quantity"] = order.Quantity,
                    ["UnitPriceStorage"] = order.UnitPriceStorage, // Save the string price
                    ["OrderDate"] = order.OrderDate,
                    ["Status"] = order.Status,
                    ["SpecialInstructions"] = order.SpecialInstructions ?? ""
                };

                Console.WriteLine($"=== SAVING ORDER ===");
                Console.WriteLine($"Order ID: {order.OrderId}");
                Console.WriteLine($"UnitPriceStorage being saved: '{order.UnitPriceStorage}'");
                Console.WriteLine($"UnitPrice property: {order.UnitPrice}");

                await tableClient.UpsertEntityAsync(entity);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving order: {ex.Message}");
                return false;
            }
        }

        public async Task<OrderMessage?> GetOrderAsync(string orderId)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(_azureSettings.TableName.Orders);
                var response = await tableClient.GetEntityAsync<TableEntity>("Order", orderId);
                return ConvertTableEntityToOrder(response.Value);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<OrderMessage>> GetAllOrdersAsync()
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(_azureSettings.TableName.Orders);
                var orders = new List<OrderMessage>();

                await foreach (var entity in tableClient.QueryAsync<TableEntity>())
                {
                    var order = ConvertTableEntityToOrder(entity);
                    orders.Add(order);
                }

                return orders.OrderByDescending(o => o.OrderDate);
            }
            catch (Exception)
            {
                return Enumerable.Empty<OrderMessage>();
            }
        }

        public async Task<bool> UpdateOrderAsync(OrderMessage order)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(_azureSettings.TableName.Orders);

                // Ensure proper table entity setup
                order.PartitionKey = "Order";
                order.RowKey = order.OrderId;

                // Convert to TableEntity manually
                var entity = new TableEntity(order.PartitionKey, order.RowKey)
                {
                    ["CustomerId"] = order.CustomerId,
                    ["CustomerName"] = order.CustomerName,
                    ["ProductId"] = order.ProductId,
                    ["ProductName"] = order.ProductName,
                    ["Quantity"] = order.Quantity,
                    ["UnitPriceStorage"] = order.UnitPriceStorage, // Save the string price
                    ["OrderDate"] = order.OrderDate,
                    ["Status"] = order.Status,
                    ["SpecialInstructions"] = order.SpecialInstructions ?? ""
                };

                await tableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteOrderAsync(string orderId)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(_azureSettings.TableName.Orders);
                await tableClient.DeleteEntityAsync("Order", orderId);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Helper method to convert TableEntity to OrderMessage
        private OrderMessage ConvertTableEntityToOrder(TableEntity entity)
        {
            var unitPriceStorage = entity.GetString("UnitPriceStorage") ?? "0.00";

            var order = new OrderMessage
            {
                PartitionKey = entity.PartitionKey,
                RowKey = entity.RowKey,
                ETag = entity.ETag,
                Timestamp = entity.Timestamp,
                CustomerId = entity.GetString("CustomerId") ?? "",
                CustomerName = entity.GetString("CustomerName") ?? "",
                ProductId = entity.GetString("ProductId") ?? "",
                ProductName = entity.GetString("ProductName") ?? "",
                Quantity = entity.GetInt32("Quantity") ?? 1,
                UnitPriceStorage = unitPriceStorage,
                OrderDate = entity.GetDateTime("OrderDate") ?? DateTime.UtcNow,
                Status = entity.GetString("Status") ?? "Pending",
                SpecialInstructions = entity.GetString("SpecialInstructions")
            };

            Console.WriteLine($"=== RETRIEVING ORDER ===");
            Console.WriteLine($"Order ID: {order.OrderId}");
            Console.WriteLine($"Retrieved UnitPriceStorage: '{unitPriceStorage}'");
            Console.WriteLine($"Converted UnitPrice: {order.UnitPrice}");
            Console.WriteLine($"Calculated TotalAmount: {order.TotalAmount}");

            return order;
        }

        // CUSTOMER OPERATIONS
        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(_azureSettings.TableName.Customers);
                var customers = new List<Customer>();

                await foreach (var customer in tableClient.QueryAsync<Customer>())
                {
                    customers.Add(customer);
                }

                return customers;
            }
            catch (Exception)
            {
                return Enumerable.Empty<Customer>();
            }
        }

        public async Task<Customer?> GetCustomerAsync(string customerId)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(_azureSettings.TableName.Customers);
                var response = await tableClient.GetEntityAsync<Customer>("Customer", customerId);
                return response.Value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> AddCustomerAsync(Customer customer)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(_azureSettings.TableName.Customers);
                await tableClient.CreateIfNotExistsAsync();
                await tableClient.AddEntityAsync(customer);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(_azureSettings.TableName.Customers);
                await tableClient.UpdateEntityAsync(customer, ETag.All, TableUpdateMode.Replace);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteCustomerAsync(string customerId)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(_azureSettings.TableName.Customers);
                await tableClient.DeleteEntityAsync("Customer", customerId);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // PRODUCT OPERATIONS
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(_azureSettings.TableName.Products);
                var products = new List<Product>();

                await foreach (var entity in tableClient.QueryAsync<TableEntity>())
                {
                    var product = ConvertTableEntityToProduct(entity);
                    products.Add(product);
                }

                return products;
            }
            catch (Exception)
            {
                return Enumerable.Empty<Product>();
            }
        }

        public async Task<Product?> GetProductAsync(string productId)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(_azureSettings.TableName.Products);
                var response = await tableClient.GetEntityAsync<TableEntity>("Product", productId);
                var product = ConvertTableEntityToProduct(response.Value);

                // DEBUG: Log what we retrieved
                Console.WriteLine($"=== DEBUG: GetProductAsync for {productId} ===");
                Console.WriteLine($"Product Name: {product.Name}");
                Console.WriteLine($"PriceStorage: '{product.PriceStorage}'");
                Console.WriteLine($"Price Property: {product.Price}");
                Console.WriteLine($"PriceInRand: {product.PriceInRand}");

                return product;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting product {productId}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> AddProductAsync(Product product)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(_azureSettings.TableName.Products);
                await tableClient.CreateIfNotExistsAsync();

                var entity = ConvertProductToTableEntity(product);
                await tableClient.AddEntityAsync(entity);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(_azureSettings.TableName.Products);

                var entity = ConvertProductToTableEntity(product);
                await tableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteProductAsync(string productId)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(_azureSettings.TableName.Products);
                await tableClient.DeleteEntityAsync("Product", productId);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // CONVERSION HELPER METHODS
        private TableEntity ConvertProductToTableEntity(Product product)
        {
            var entity = new TableEntity(product.PartitionKey, product.RowKey)
            {
                ["Name"] = product.Name,
                ["Description"] = product.Description,
                ["PriceStorage"] = product.PriceStorage, // Store the string price
                ["Category"] = product.Category,
                ["StockQuantity"] = product.StockQuantity,
                ["ImageUrl"] = product.ImageUrl,
                ["IsAvailable"] = product.IsAvailable,
                ["CreatedDate"] = product.CreatedDate
            };

            Console.WriteLine($"=== STORING PRODUCT ===");
            Console.WriteLine($"Name: {product.Name}");
            Console.WriteLine($"PriceStorage being stored: '{product.PriceStorage}'");
            Console.WriteLine($"Price property value: {product.Price}");

            return entity;
        }

        private Product ConvertTableEntityToProduct(TableEntity entity)
        {
            var priceStorageValue = entity.GetString("PriceStorage") ?? "0.00";

            var product = new Product
            {
                PartitionKey = entity.PartitionKey,
                RowKey = entity.RowKey,
                ETag = entity.ETag,
                Timestamp = entity.Timestamp,
                Name = entity.GetString("Name") ?? "",
                Description = entity.GetString("Description") ?? "",
                PriceStorage = priceStorageValue, // Get the string price
                Category = entity.GetString("Category") ?? "",
                StockQuantity = entity.GetInt32("StockQuantity") ?? 0,
                ImageUrl = entity.GetString("ImageUrl") ?? "",
                IsAvailable = entity.GetBoolean("IsAvailable") ?? true,
                CreatedDate = entity.GetDateTime("CreatedDate") ?? DateTime.UtcNow
            };

            // DEBUG: Log what we're converting
            Console.WriteLine($"=== CONVERTING ENTITY TO PRODUCT ===");
            Console.WriteLine($"Name: {product.Name}");
            Console.WriteLine($"Retrieved PriceStorage: '{priceStorageValue}'");
            Console.WriteLine($"Converted Price: {product.Price}");

            return product;
        }
    }
}
//-----------------------End Of File----------------//