//-----------------------Start of File-----------------------//
using System.ComponentModel.DataAnnotations;

namespace CLDV6212_POE_st10439398.Models.ViewModels
{
    public class ProductViewModel
    {
        public string ProductId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [Display(Name = "Stock Quantity")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int StockQuantity { get; set; } = 0;

        [Display(Name = "Product Image")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Current Image URL")]
        public string ImageUrl { get; set; } = string.Empty;

        [Display(Name = "Is Available")]
        public bool IsAvailable { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }

    public class ProductListViewModel
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public int TotalProducts { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public string CategoryFilter { get; set; } = string.Empty;
        public string SortBy { get; set; } = "Name";
        public bool IsDescending { get; set; } = false;
        public List<string> Categories { get; set; } = new List<string>();
    }
}
//-----------------------End Of File----------------//