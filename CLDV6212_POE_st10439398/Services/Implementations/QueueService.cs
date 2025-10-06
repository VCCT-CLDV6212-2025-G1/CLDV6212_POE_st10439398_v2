//-----------------------Start Of File-----------------
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using CLDV6212_POE_st10439398.Models;
using CLDV6212_POE_st10439398.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Text;

namespace CLDV6212_POE_st10439398.Services.Implementations
{
    public class QueueService : IQueueService
    {
        private readonly QueueServiceClient _queueServiceClient;
        private readonly AzureSettings _azureSettings;
        private readonly ITableService _tableService;

        public QueueService(QueueServiceClient queueServiceClient, IOptions<AzureSettings> azureSettings, ITableService tableService)
        {
            _queueServiceClient = queueServiceClient;
            _azureSettings = azureSettings.Value;
            _tableService = tableService;
        }

        #region Order Queue Operations (existing)

        public async Task<bool> SendOrderMessageAsync(OrderMessage order)
        {
            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(_azureSettings.QueueName);
                await queueClient.CreateIfNotExistsAsync();

                var messageJson = order.ToJson();
                var messageBytes = Encoding.UTF8.GetBytes(messageJson);
                var base64Message = Convert.ToBase64String(messageBytes);

                await queueClient.SendMessageAsync(base64Message);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<OrderMessage?> ReceiveOrderMessageAsync()
        {
            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(_azureSettings.QueueName);
                var response = await queueClient.ReceiveMessageAsync();

                if (response.Value != null)
                {
                    var message = response.Value;
                    var messageBytes = Convert.FromBase64String(message.MessageText);
                    var messageJson = Encoding.UTF8.GetString(messageBytes);

                    var order = OrderMessage.FromJson(messageJson);

                    // Delete the message after processing
                    await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);

                    return order;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<OrderMessage>> PeekOrderMessagesAsync(int maxMessages = 10)
        {
            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(_azureSettings.QueueName);
                var response = await queueClient.PeekMessagesAsync(maxMessages);
                var orders = new List<OrderMessage>();

                foreach (var message in response.Value)
                {
                    try
                    {
                        var messageBytes = Convert.FromBase64String(message.MessageText);
                        var messageJson = Encoding.UTF8.GetString(messageBytes);
                        var order = OrderMessage.FromJson(messageJson);
                        orders.Add(order);
                    }
                    catch (Exception)
                    {
                        // Skip malformed messages
                        continue;
                    }
                }

                return orders;
            }
            catch (Exception)
            {
                return Enumerable.Empty<OrderMessage>();
            }
        }

        public async Task<int> GetQueueLengthAsync()
        {
            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(_azureSettings.QueueName);
                var properties = await queueClient.GetPropertiesAsync();
                return properties.Value.ApproximateMessagesCount;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<bool> ClearQueueAsync()
        {
            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(_azureSettings.QueueName);
                await queueClient.ClearMessagesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Inventory Queue Operations (new)

        public async Task<bool> SendInventoryMessageAsync(InventoryMessage inventoryMessage)
        {
            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(_azureSettings.InventoryQueueName);
                await queueClient.CreateIfNotExistsAsync();

                var messageJson = inventoryMessage.ToJson();
                var messageBytes = Encoding.UTF8.GetBytes(messageJson);
                var base64Message = Convert.ToBase64String(messageBytes);

                await queueClient.SendMessageAsync(base64Message);

                Console.WriteLine($"=== INVENTORY MESSAGE SENT ===");
                Console.WriteLine($"Product: {inventoryMessage.ProductName}");
                Console.WriteLine($"Operation: {inventoryMessage.OperationType}");
                Console.WriteLine($"Quantity Change: {inventoryMessage.QuantityChange}");
                Console.WriteLine($"New Stock: {inventoryMessage.NewStock}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending inventory message: {ex.Message}");
                return false;
            }
        }

        public async Task<InventoryMessage?> ReceiveInventoryMessageAsync()
        {
            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(_azureSettings.InventoryQueueName);
                var response = await queueClient.ReceiveMessageAsync();

                if (response.Value != null)
                {
                    var message = response.Value;
                    var messageBytes = Convert.FromBase64String(message.MessageText);
                    var messageJson = Encoding.UTF8.GetString(messageBytes);

                    var inventoryMessage = InventoryMessage.FromJson(messageJson);

                    // Delete the message after processing
                    await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);

                    Console.WriteLine($"=== INVENTORY MESSAGE PROCESSED ===");
                    Console.WriteLine($"Product: {inventoryMessage.ProductName}");
                    Console.WriteLine($"Operation: {inventoryMessage.OperationType}");
                    Console.WriteLine($"Previous Stock: {inventoryMessage.PreviousStock}");
                    Console.WriteLine($"New Stock: {inventoryMessage.NewStock}");

                    return inventoryMessage;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving inventory message: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<InventoryMessage>> PeekInventoryMessagesAsync(int maxMessages = 10)
        {
            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(_azureSettings.InventoryQueueName);
                var response = await queueClient.PeekMessagesAsync(maxMessages);
                var inventoryMessages = new List<InventoryMessage>();

                foreach (var message in response.Value)
                {
                    try
                    {
                        var messageBytes = Convert.FromBase64String(message.MessageText);
                        var messageJson = Encoding.UTF8.GetString(messageBytes);
                        var inventoryMessage = InventoryMessage.FromJson(messageJson);
                        inventoryMessages.Add(inventoryMessage);
                    }
                    catch (Exception)
                    {
                        // Skip malformed messages
                        continue;
                    }
                }

                return inventoryMessages;
            }
            catch (Exception)
            {
                return Enumerable.Empty<InventoryMessage>();
            }
        }

        public async Task<int> GetInventoryQueueLengthAsync()
        {
            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(_azureSettings.InventoryQueueName);
                var properties = await queueClient.GetPropertiesAsync();
                return properties.Value.ApproximateMessagesCount;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<bool> ClearInventoryQueueAsync()
        {
            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(_azureSettings.InventoryQueueName);
                await queueClient.ClearMessagesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ProcessInventoryUpdateAsync(string productId, int quantityChange, string operationType, string reason = "")
        {
            try
            {
                // Get current product details
                var product = await _tableService.GetProductAsync(productId);
                if (product == null)
                {
                    Console.WriteLine($"Product {productId} not found for inventory update");
                    return false;
                }

                var previousStock = product.StockQuantity;
                var newStock = previousStock + quantityChange;

                // Ensure stock doesn't go negative
                if (newStock < 0)
                {
                    Console.WriteLine($"Cannot reduce stock below 0. Current: {previousStock}, Attempted change: {quantityChange}");
                    newStock = 0;
                    quantityChange = -previousStock; // Adjust the change to only reduce to 0
                }

                // Create inventory message
                var inventoryMessage = new InventoryMessage
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    OperationType = operationType,
                    QuantityChange = quantityChange,
                    PreviousStock = previousStock,
                    NewStock = newStock,
                    Reason = reason,
                    ProcessedBy = "System"
                };

                // Send to inventory queue
                var queueSuccess = await SendInventoryMessageAsync(inventoryMessage);

                // Update the actual product stock
                if (queueSuccess)
                {
                    product.StockQuantity = newStock;
                    var updateSuccess = await _tableService.UpdateProductAsync(product);

                    if (updateSuccess)
                    {
                        Console.WriteLine($"Successfully updated stock for {product.Name}: {previousStock} -> {newStock}");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"Failed to update product stock in database for {product.Name}");
                        return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing inventory update: {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}
//-----------------------End Of File----------------//