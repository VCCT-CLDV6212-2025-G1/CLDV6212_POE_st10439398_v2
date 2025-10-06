//-----------------------Start of File-----------------------//
using System.ComponentModel.DataAnnotations;

namespace CLDV6212_POE_st10439398.Models.ViewModels
{
    public class CustomerViewModel
    {
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public string FullName => $"{FirstName} {LastName}";
    }

    public class CustomerListViewModel
    {
        public IEnumerable<Customer> Customers { get; set; } = new List<Customer>();
        public int TotalCustomers { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public string SortBy { get; set; } = "Name";
        public bool IsDescending { get; set; } = false;
    }
}
//-----------------------End Of File----------------//