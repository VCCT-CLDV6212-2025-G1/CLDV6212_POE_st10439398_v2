//-----------------------Start Of File----------------//
using System.ComponentModel.DataAnnotations;

namespace CLDV6212_POE_st10439398.Models.ViewModels
{

    /// ViewModel for user login

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; } = false;

        public string? ReturnUrl { get; set; }
    }


    /// ViewModel for user registration

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(255)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [StringLength(100)]
        [Display(Name = "City")]
        public string? City { get; set; }
    }


    /// ViewModel for cart display

    public class CartViewModel
    {
        public Cart Cart { get; set; } = new Cart();
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
    }


    /// ViewModel for checkout

    public class CheckoutViewModel
    {
        public Cart Cart { get; set; } = new Cart();
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();

        [Required]
        [Display(Name = "Shipping Address")]
        [StringLength(500)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Display(Name = "Special Instructions")]
        [StringLength(1000)]
        public string? SpecialInstructions { get; set; }

        public decimal TotalAmount { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }


    /// ViewModel for customer dashboard

    public class CustomerDashboardViewModel
    {
        public User User { get; set; } = new User();
        public List<Product> AvailableProducts { get; set; } = new List<Product>();
        public Cart? CurrentCart { get; set; }
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public int CartItemCount { get; set; }
    }


    /// ViewModel for admin dashboard

    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalAdmins { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessedOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<User> RecentUsers { get; set; } = new List<User>();
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
    }


    /// ViewModel for order management (admin)

    public class OrderManagementViewModel
    {
        public List<Order> Orders { get; set; } = new List<Order>();
        public string StatusFilter { get; set; } = "All";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
    }
}
//-----------------------End Of File----------------//