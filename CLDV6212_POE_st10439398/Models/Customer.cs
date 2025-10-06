//-----------------------Start Of File----------------//
using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace CLDV6212_POE_st10439398.Models
{
    public class Customer : ITableEntity
    {
        public Customer()
        {
            PartitionKey = "Customer";
            RowKey = Guid.NewGuid().ToString();
            Timestamp = DateTimeOffset.UtcNow;
        }

        public Customer(string customerId, string firstName, string lastName, string email, string phone)
        {
            PartitionKey = "Customer";
            RowKey = customerId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Phone = phone;
            CreatedDate = DateTime.UtcNow;
            Timestamp = DateTimeOffset.UtcNow;
        }

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

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

      
        public string PartitionKey { get; set; } = "Customer";
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        [Display(Name = "Customer ID")]
        public string CustomerId => RowKey;

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";
    }
}
//-----------------------End Of File----------------//