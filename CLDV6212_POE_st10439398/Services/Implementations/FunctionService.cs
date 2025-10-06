using CLDV6212_POE_st10439398.Models;
using CLDV6212_POE_st10439398.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace CLDV6212_POE_st10439398.Services.Implementations
{
    public class FunctionService : IFunctionService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FunctionService> _logger;
        private readonly string _functionBaseUrl;
        private readonly string _functionKey;

        public FunctionService(HttpClient httpClient, IConfiguration configuration, ILogger<FunctionService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Read from appsettings.json
            _functionBaseUrl = configuration["AzureFunctions:BaseUrl"]
                ?? "https://cldv6212-st10439398-functions.azurewebsites.net/api";
            _functionKey = configuration["AzureFunctions:FunctionKey"] ?? "";
        }

        public async Task<bool> StoreCustomerToTableAsync(Customer customer)
        {
            try
            {
                var url = $"{_functionBaseUrl}/StoreToTable?code={_functionKey}";

                var customerData = new
                {
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    Phone = customer.Phone
                };

                var json = JsonSerializer.Serialize(customerData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"✅ Function call successful: Customer {customer.Email} stored via Azure Function");
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Function call failed: {response.StatusCode} - {errorContent}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling StoreToTable function: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UploadProductImageAsync(IFormFile file, string fileName)
        {
            try
            {
                var url = $"{_functionBaseUrl}/WriteToBlob?code={_functionKey}";

                using var stream = file.OpenReadStream();
                using var content = new StreamContent(stream);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"✅ Function call successful: Image {fileName} uploaded via Azure Function");
                    return true;
                }

                _logger.LogWarning($"Blob upload function failed: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling WriteToBlob function: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UploadContractFileAsync(IFormFile file, string fileName, string description, string contractType)
        {
            try
            {
                var url = $"{_functionBaseUrl}/WriteToFiles?code={_functionKey}";

                using var stream = file.OpenReadStream();
                using var content = new StreamContent(stream);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"✅ Function call successful: File {fileName} uploaded via Azure Function");
                    return true;
                }

                _logger.LogWarning($"File upload function failed: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling WriteToFiles function: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetFunctionHealthAsync()
        {
            try
            {
                var url = $"{_functionBaseUrl}/health";
                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode ? "Healthy" : "Unhealthy";
            }
            catch
            {
                return "Unavailable";
            }
        }
    }
}