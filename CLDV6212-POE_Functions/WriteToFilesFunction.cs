using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Files.Shares;

namespace CLDV_POE_Functions
{
    public class WriteToFilesFunction
    {
        private readonly ILogger _logger;

        public WriteToFilesFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<WriteToFilesFunction>();
        }

        [Function("WriteToFiles")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("WriteToFiles function triggered");

            try
            {
                var fileName = $"contract_{Guid.NewGuid()}.pdf";
                var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

                var shareClient = new ShareClient(connectionString, "contracts");
                await shareClient.CreateIfNotExistsAsync();

                var directoryClient = shareClient.GetRootDirectoryClient();
                var fileClient = directoryClient.GetFileClient(fileName);

                using var ms = new MemoryStream();
                await req.Body.CopyToAsync(ms);
                ms.Position = 0;

                await fileClient.CreateAsync(ms.Length);
                await fileClient.UploadAsync(ms);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync($"File uploaded: {fileName}");
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