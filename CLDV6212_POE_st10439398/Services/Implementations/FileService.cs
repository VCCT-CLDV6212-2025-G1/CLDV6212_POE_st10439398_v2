//-----------------------start Of File-----------------
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using CLDV6212_POE_st10439398.Models;
using CLDV6212_POE_st10439398.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace CLDV6212_POE_st10439398.Services.Implementations
{
    public class FileService : IFileService
    {
        private readonly ShareServiceClient _shareServiceClient;
        private readonly AzureSettings _azureSettings;

        public FileService(ShareServiceClient shareServiceClient, IOptions<AzureSettings> azureSettings)
        {
            _shareServiceClient = shareServiceClient;
            _azureSettings = azureSettings.Value;
        }

        public async Task<bool> UploadFileAsync(IFormFile file, string fileName, string description = "", string contractType = "")
        {
            try
            {
                var shareClient = _shareServiceClient.GetShareClient(_azureSettings.FileShareName);
                await shareClient.CreateIfNotExistsAsync();

                var directoryClient = shareClient.GetRootDirectoryClient();
                var fileClient = directoryClient.GetFileClient(fileName);

                var metadata = new Dictionary<string, string>
                {
                    ["OriginalFileName"] = file.FileName,
                    ["ContentType"] = file.ContentType,
                    ["UploadedDate"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    ["Description"] = description,
                    ["ContractType"] = contractType
                };

                using var stream = file.OpenReadStream();
                await fileClient.CreateAsync(stream.Length);
                await fileClient.UploadAsync(stream);
                await fileClient.SetMetadataAsync(metadata);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<FileInfoModel>> GetAllFilesAsync()
        {
            try
            {
                var shareClient = _shareServiceClient.GetShareClient(_azureSettings.FileShareName);
                var directoryClient = shareClient.GetRootDirectoryClient();
                var files = new List<FileInfoModel>();

                await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync())
                {
                    if (item.IsDirectory == false)
                    {
                        var fileClient = directoryClient.GetFileClient(item.Name);
                        var properties = await fileClient.GetPropertiesAsync();

                        var description = "";
                        var contractType = "General";

                        if (properties.Value.Metadata.ContainsKey("Description"))
                        {
                            description = properties.Value.Metadata["Description"];
                        }

                        if (properties.Value.Metadata.ContainsKey("ContractType"))
                        {
                            contractType = properties.Value.Metadata["ContractType"];
                        }

                        files.Add(new FileInfoModel
                        {
                            FileName = item.Name,
                            ContentType = properties.Value.ContentType,
                            Size = properties.Value.ContentLength,
                            LastModified = properties.Value.LastModified.DateTime,
                            DownloadUrl = fileClient.Uri.ToString(),
                            Description = description,
                            ContractType = contractType
                        });
                    }
                }

                return files;
            }
            catch (Exception)
            {
                return Enumerable.Empty<FileInfoModel>();
            }
        }

        public async Task<Stream?> DownloadFileAsync(string fileName)
        {
            try
            {
                var shareClient = _shareServiceClient.GetShareClient(_azureSettings.FileShareName);
                var directoryClient = shareClient.GetRootDirectoryClient();
                var fileClient = directoryClient.GetFileClient(fileName);

                var response = await fileClient.DownloadAsync();
                return response.Value.Content;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            try
            {
                var shareClient = _shareServiceClient.GetShareClient(_azureSettings.FileShareName);
                var directoryClient = shareClient.GetRootDirectoryClient();
                var fileClient = directoryClient.GetFileClient(fileName);

                var response = await fileClient.DeleteIfExistsAsync();
                return response.Value;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> FileExistsAsync(string fileName)
        {
            try
            {
                var shareClient = _shareServiceClient.GetShareClient(_azureSettings.FileShareName);
                var directoryClient = shareClient.GetRootDirectoryClient();
                var fileClient = directoryClient.GetFileClient(fileName);

                var response = await fileClient.ExistsAsync();
                return response.Value;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<FileInfoModel?> GetFileInfoAsync(string fileName)
        {
            try
            {
                var shareClient = _shareServiceClient.GetShareClient(_azureSettings.FileShareName);
                var directoryClient = shareClient.GetRootDirectoryClient();
                var fileClient = directoryClient.GetFileClient(fileName);

                var properties = await fileClient.GetPropertiesAsync();

                var description = "";
                var contractType = "General";

                if (properties.Value.Metadata.ContainsKey("Description"))
                {
                    description = properties.Value.Metadata["Description"];
                }

                if (properties.Value.Metadata.ContainsKey("ContractType"))
                {
                    contractType = properties.Value.Metadata["ContractType"];
                }

                return new FileInfoModel
                {
                    FileName = fileName,
                    ContentType = properties.Value.ContentType,
                    Size = properties.Value.ContentLength,
                    LastModified = properties.Value.LastModified.DateTime,
                    DownloadUrl = fileClient.Uri.ToString(),
                    Description = description,
                    ContractType = contractType
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
//-----------------------End Of File----------------//