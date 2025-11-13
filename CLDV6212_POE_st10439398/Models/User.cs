//-----------------------Start Of File----------------//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLDV6212_POE_st10439398.Models
{

    /// User entity for Azure SQL Database authentication
    /// Stores login credentials and user role information

    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Role")]
        public string Role { get; set; } = "Customer"; // "Customer" or "Admin"

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Login")]
        public DateTime? LastLoginDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [StringLength(100)]
        [Display(Name = "City")]
        public string? City { get; set; }

        // Navigation properties
        public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        // Computed properties
        [Display(Name = "Full Name")]
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "Is Admin")]
        [NotMapped]
        public bool IsAdmin => Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;

        [Display(Name = "Is Customer")]
        [NotMapped]
        public bool IsCustomer => Role?.Equals("Customer", StringComparison.OrdinalIgnoreCase) == true;
    }
}
//-----------------------End Of File----------------//