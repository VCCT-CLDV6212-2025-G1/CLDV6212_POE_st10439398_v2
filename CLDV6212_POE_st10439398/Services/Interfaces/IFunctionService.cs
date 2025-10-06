using CLDV6212_POE_st10439398.Models;

namespace CLDV6212_POE_st10439398.Services.Interfaces
{
    public interface IFunctionService
    {
        Task<bool> StoreCustomerToTableAsync(Customer customer);
        Task<bool> UploadProductImageAsync(IFormFile file, string fileName);
        Task<bool> UploadContractFileAsync(IFormFile file, string fileName, string description, string contractType);
        Task<string> GetFunctionHealthAsync();
    }
}