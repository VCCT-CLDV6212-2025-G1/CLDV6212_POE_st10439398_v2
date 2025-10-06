//-----------------------Start Of File----------------//
using CLDV6212_POE_st10439398.Models;

namespace CLDV6212_POE_st10439398.Services.Interfaces
{
    public interface IFileService
    {
        Task<bool> UploadFileAsync(IFormFile file, string fileName, string description = "", string contractType = "");
        Task<IEnumerable<FileInfoModel>> GetAllFilesAsync();
        Task<Stream?> DownloadFileAsync(string fileName);
        Task<bool> DeleteFileAsync(string fileName);
        Task<bool> FileExistsAsync(string fileName);
        Task<FileInfoModel?> GetFileInfoAsync(string fileName);
    }
}
//-----------------------End Of File----------------//