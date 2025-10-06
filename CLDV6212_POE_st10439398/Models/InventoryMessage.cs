//-----------------------Start Of File----------------//
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace CLDV6212_POE_st10439398.Models
{
    public class InventoryMessage
    {
        public InventoryMessage()
        {
            MessageId = Guid.NewGuid().ToString();
            Timestamp = DateTime.UtcNow;
        }

        [Display(Name = "Message ID")]
        public string MessageId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Product ID")]
        public string ProductId { get; set; } = string.Empty;

        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Display(Name = "Operation Type")]
        public string OperationType { get; set; } = string.Empty; 

        [Display(Name = "Quantity Change")]
        public int QuantityChange { get; set; } 

        [Display(Name = "Previous Stock")]
        public int PreviousStock { get; set; }

        [Display(Name = "New Stock")]
        public int NewStock { get; set; }

        [Display(Name = "Reason")]
        public string Reason { get; set; } = string.Empty;

        [Display(Name = "Timestamp")]
        public DateTime Timestamp { get; set; }

        [Display(Name = "User/System")]
        public string ProcessedBy { get; set; } = "System";

        // Convert to JSON for queue storage
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = false
            });
        }

        // Create from JSON
        public static InventoryMessage FromJson(string json)
        {
            return JsonSerializer.Deserialize<InventoryMessage>(json) ?? new InventoryMessage();
        }
    }

    public class InventoryAdjustmentModel
    {
        [Required]
        [Display(Name = "Product")]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Adjustment Type")]
        public string AdjustmentType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Quantity Change")]
        [Range(-9999, 9999, ErrorMessage = "Quantity change must be between -9999 and 9999")]
        public int QuantityChange { get; set; }

        [Display(Name = "Reason")]
        public string Reason { get; set; } = string.Empty;

        // For dropdown
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
//-----------------------End Of File----------------//