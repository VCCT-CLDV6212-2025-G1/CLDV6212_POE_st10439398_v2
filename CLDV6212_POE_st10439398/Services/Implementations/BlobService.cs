//-----------------------Start Of File-----------------
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CLDV6212_POE_st10439398.Models;
using CLDV6212_POE_st10439398.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace CLDV6212_POE_st10439398.Services.Implementations
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly AzureSettings _azureSettings;

        public BlobService(BlobServiceClient blobServiceClient, IOptions<AzureSettings> azureSettings)
        {
            _blobServiceClient = blobServiceClient;
            _azureSettings = azureSettings.Value;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string fileName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_azureSettings.BlobContainerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var blobClient = containerClient.GetBlobClient(fileName);

                var metadata = new Dictionary<string, string>
                {
                    ["OriginalFileName"] = file.FileName,
                    ["ContentType"] = file.ContentType,
                    ["UploadedDate"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                };

                using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                });

                await blobClient.SetMetadataAsync(metadata);

                return blobClient.Uri.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<IEnumerable<BlobInfoModel>> GetAllBlobsAsync()
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_azureSettings.BlobContainerName);
                var blobs = new List<BlobInfoModel>();

                await foreach (var blobItem in containerClient.GetBlobsAsync(BlobTraits.Metadata))
                {
                    var blobClient = containerClient.GetBlobClient(blobItem.Name);

                    blobs.Add(new BlobInfoModel
                    {
                        BlobName = blobItem.Name,
                        ContentType = blobItem.Properties.ContentType ?? "application/octet-stream",
                        Size = blobItem.Properties.ContentLength ?? 0,
                        LastModified = blobItem.Properties.LastModified?.DateTime ?? DateTime.MinValue,
                        Url = blobClient.Uri.ToString(),
                        Metadata = (Dictionary<string, string>)blobItem.Metadata
                    });
                }

                return blobs;
            }
            catch (Exception)
            {
                return Enumerable.Empty<BlobInfoModel>();
            }
        }

        public async Task<bool> DeleteBlobAsync(string blobName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_azureSettings.BlobContainerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                var response = await blobClient.DeleteIfExistsAsync();
                return response.Value;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Stream> DownloadBlobAsync(string blobName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_azureSettings.BlobContainerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                var response = await blobClient.DownloadStreamingAsync();
                return response.Value.Content;
            }
            catch (Exception)
            {
                return Stream.Null;
            }
        }

        public async Task<bool> BlobExistsAsync(string blobName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_azureSettings.BlobContainerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                var response = await blobClient.ExistsAsync();
                return response.Value;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> GetBlobUrlAsync(string blobName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_azureSettings.BlobContainerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                var exists = await blobClient.ExistsAsync();
                return exists.Value ? blobClient.Uri.ToString() : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
//-----------------------End Of File----------------//