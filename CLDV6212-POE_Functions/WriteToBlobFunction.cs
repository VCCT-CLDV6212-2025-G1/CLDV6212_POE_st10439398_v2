using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;

namespace CLDV_POE_Functions
{
    public class WriteToBlobFunction
    {
        private readonly ILogger _logger;

        public WriteToBlobFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<WriteToBlobFunction>();
        }

        [Function("WriteToBlob")]
        public async Task<HttpResponseData> Run(
           [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("WriteToBlob function triggered");

            try
            {
                var fileName = $"image_{Guid.NewGuid()}.jpg";
                var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient("productimages");
                await containerClient.CreateIfNotExistsAsync();

                var blobClient = containerClient.GetBlobClient(fileName);
                await blobClient.UploadAsync(req.Body, overwrite: true);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync($"Blob uploaded: {fileName}");
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
}