using System.Net;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Queues;

namespace CLDV_POE_Functions
{
    public class QueueOperationsFunction
    {
        private readonly ILogger _logger;

        public QueueOperationsFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<QueueOperationsFunction>();
        }

        // HTTP trigger to manually process order queue
        [Function("ProcessOrderQueue")]
        public async Task<HttpResponseData> ProcessOrderQueue(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("Processing order queue via HTTP trigger");

            try
            {
                var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                var queueClient = new QueueClient(connectionString, "orderprocessing");
                
                var message = await queueClient.ReceiveMessageAsync();
                
                if (message.Value != null)
                {
                    var messageText = Encoding.UTF8.GetString(Convert.FromBase64String(message.Value.MessageText));
                    _logger.LogInformation($"Processing order: {messageText}");
                    
                    // Delete message after processing
                    await queueClient.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt);
                    
                    var response = req.CreateResponse(HttpStatusCode.OK);
                    await response.WriteStringAsync($"Order processed: {messageText}");
                    return response;
                }
                else
                {
                    var response = req.CreateResponse(HttpStatusCode.OK);
                    await response.WriteStringAsync("No orders in queue");
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }

        // HTTP trigger to manually process inventory queue
        [Function("ProcessInventoryQueue")]
        public async Task<HttpResponseData> ProcessInventoryQueue(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("Processing inventory queue via HTTP trigger");

            try
            {
                var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                var queueClient = new QueueClient(connectionString, "inventorymanagement");
                
                var message = await queueClient.ReceiveMessageAsync();
                
                if (message.Value != null)
                {
                    var messageText = Encoding.UTF8.GetString(Convert.FromBase64String(message.Value.MessageText));
                    _logger.LogInformation($"Processing inventory: {messageText}");
                    
                    // Delete message after processing
                    await queueClient.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt);
                    
                    var response = req.CreateResponse(HttpStatusCode.OK);
                    await response.WriteStringAsync($"Inventory processed: {messageText}");
                    return response;
                }
                else
                {
                    var response = req.CreateResponse(HttpStatusCode.OK);
                    await response.WriteStringAsync("No inventory updates in queue");
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }
    }
}