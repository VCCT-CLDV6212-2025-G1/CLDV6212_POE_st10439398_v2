//-----------------------Start Of File----------------//
using System.ComponentModel.DataAnnotations;

namespace CLDV6212_POE_st10439398.Models
{
    public class FileUploadModel
    {
        [Required]
        [Display(Name = "Select File")]
        public IFormFile File { get; set; } = null!;

        [Display(Name = "File Description")]
        public string? Description { get; set; }

        [Display(Name = "Contract Type")]
        public string ContractType { get; set; } = "General";
    }

    public class FileInfoModel
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ContractType { get; set; } = string.Empty;
    }

    public class ProductImageUploadModel
    {
        public string ProductId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Product Image")]
        public IFormFile ImageFile { get; set; } = null!;

        [Display(Name = "Alt Text")]
        public string? AltText { get; set; }
    }

    public class BlobInfoModel
    {
        public string BlobName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public string Url { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
//-----------------------End Of File----------------//