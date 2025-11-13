//-----------------------Start Of File----------------//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLDV6212_POE_st10439398.Models
{

    /// Shopping cart for a user

    [Table("Carts")]
    public class Cart
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CartId { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        // Computed properties
        [NotMapped]
        public decimal TotalAmount => CartItems?.Sum(ci => ci.LineTotal) ?? 0;

        [NotMapped]
        public int TotalItems => CartItems?.Sum(ci => ci.Quantity) ?? 0;
    }


    /// Individual item in shopping cart

    [Table("CartItems")]
    public class CartItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CartItemId { get; set; }

        [Required]
        public int CartId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Product ID")]
        public string ProductId { get; set; } = string.Empty; // References Product in Azure Table Storage

        [Required]
        [StringLength(255)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Unit Price")]
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; } = null!;

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