using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;

namespace CLDV_POE_Functions
{
    public class StoreToTableFunction
    {
        private readonly ILogger _logger;

        public StoreToTableFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<StoreToTableFunction>();
        }

        [Function("StoreToTable")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get","post")] HttpRequestData req)
        {
            _logger.LogInformation("StoreToTable function triggered");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonSerializer.Deserialize<CustomerData>(requestBody);

                if (data == null || string.IsNullOrEmpty(data.FirstName))
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteStringAsync("Invalid data");
                    return badResponse;
                }

                var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                var tableClient = new TableClient(connectionString, "Customers");
                await tableClient.CreateIfNotExistsAsync();

                var entity = new TableEntity("Customer", Guid.NewGuid().ToString())
                {
                    ["FirstName"] = data.FirstName,
                    ["LastName"] = data.LastName ?? "",
                    ["Email"] = data.Email ?? "",
                    ["Phone"] = data.Phone ?? ""
                };

                await tableClient.AddEntityAsync(entity);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync("Customer stored successfully");
                return response;
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

    public class CustomerData
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
    }
}