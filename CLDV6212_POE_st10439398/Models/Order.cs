//-----------------------Start Of File----------------//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLDV6212_POE_st10439398.Models
{

    /// Order entity stored in SQL Database
    /// Represents a completed customer order

    [Table("Orders")]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending"; // Pending, Processing, PROCESSED, Completed, Cancelled

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        [StringLength(500)]
        [Display(Name = "Shipping Address")]
        public string? ShippingAddress { get; set; }

        [StringLength(1000)]
        [Display(Name = "Special Instructions")]
        public string? SpecialInstructions { get; set; }

        [Display(Name = "Processed Date")]
        public DateTime? ProcessedDate { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Computed properties
        [NotMapped]
        public string TotalAmountFormatted => $"R {TotalAmount:N2}";

        [NotMapped]
        public int TotalItems => OrderItems?.Sum(oi => oi.Quantity) ?? 0;

        [NotMapped]
        public string StatusBadgeClass => Status switch
        {
            "Pending" => "warning",
            "Processing" => "info",
            "PROCESSED" => "success",
            "Completed" => "success",
            "Cancelled" => "danger",
            _ => "secondary"
        };
    }


    /// Individual item in an order

    [Table("OrderItems")]
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderItemId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Product ID")]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Unit Price")]
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        // Computed properties
        [NotMapped]
        [Display(Name = "Line Total")]
        [DataType(DataType.Currency)]
        public decimal LineTotal => Quantity * UnitPrice;

        [NotMapped]
        public string PriceFormatted => $"R {UnitPrice:N2}";

        [NotMapped]
        public string LineTotalFormatted => $"R {LineTotal:N2}";
    }
}
//-----------------------End Of File----------------//