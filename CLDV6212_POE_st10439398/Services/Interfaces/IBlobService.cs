//-----------------------Start Of File----------------//
using CLDV6212_POE_st10439398.Models;

namespace CLDV6212_POE_st10439398.Services.Interfaces
{
    public interface IBlobService
    {
        Task<string> UploadImageAsync(IFormFile file, string fileName);
        Task<IEnumerable<BlobInfoModel>> GetAllBlobsAsync();
        Task<bool> DeleteBlobAsync(string blobName);
        Task<Stream> DownloadBlobAsync(string blobName);
        Task<bool> BlobExistsAsync(string blobName);
        Task<string> GetBlobUrlAsync(string blobName);
    }
}
//-----------------------End Of File----------------//