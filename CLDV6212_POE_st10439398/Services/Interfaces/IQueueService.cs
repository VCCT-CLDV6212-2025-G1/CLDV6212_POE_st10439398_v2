//-----------------------Start Of File----------------//
using CLDV6212_POE_st10439398.Models;

namespace CLDV6212_POE_st10439398.Services.Interfaces
{
    public interface IQueueService
    {
        // Order Queue Operations 
        Task<bool> SendOrderMessageAsync(OrderMessage order);
        Task<OrderMessage?> ReceiveOrderMessageAsync();
        Task<IEnumerable<OrderMessage>> PeekOrderMessagesAsync(int maxMessages = 10);
        Task<int> GetQueueLengthAsync();
        Task<bool> ClearQueueAsync();

        // Inventory Queue Operations 
        Task<bool> SendInventoryMessageAsync(InventoryMessage inventoryMessage);
        Task<InventoryMessage?> ReceiveInventoryMessageAsync();
        Task<IEnumerable<InventoryMessage>> PeekInventoryMessagesAsync(int maxMessages = 10);
        Task<int> GetInventoryQueueLengthAsync();
        Task<bool> ClearInventoryQueueAsync();
        Task<bool> ProcessInventoryUpdateAsync(string productId, int quantityChange, string operationType, string reason = "");
    }
}//-----------------------End Of File----------------//