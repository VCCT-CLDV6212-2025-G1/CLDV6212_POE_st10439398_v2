//-----------------------Start Of File----------------//
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Azure.Data.Tables;
using Azure;
using System.Globalization;

namespace CLDV6212_POE_st10439398.Models
{
    public class OrderMessage : ITableEntity
    {
        public OrderMessage()
        {
            OrderId = Guid.NewGuid().ToString();
            OrderDate = DateTime.UtcNow;
            Status = "Pending";
            PartitionKey = "Order"; // Static partition for all orders
            RowKey = OrderId;
            UnitPriceStorage = "0.00"; // Initialize storage
        }

        // ITableEntity properties
        public string PartitionKey { get; set; } = "Order";
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        [Display(Name = "Order ID")]
        public string OrderId
        {
            get => RowKey;
            set
            {
                RowKey = value;
                if (string.IsNullOrEmpty(RowKey))
                {
                    RowKey = Guid.NewGuid().ToString();
                }
            }
        }

        [Required]
        [Display(Name = "Customer ID")]
        public string CustomerId { get; set; } = string.Empty;

        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Product ID")]
        public string ProductId { get; set; } = string.Empty;

        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Quantity")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;

        // Storage for Unit Price - this gets saved to Azure
        public string UnitPriceStorage { get; set; } = "0.00";

        [Display(Name = "Unit Price")]
        [DataType(DataType.Currency)]
        [System.Text.Json.Serialization.JsonIgnore]
        public decimal UnitPrice
        {
            get
            {
                if (decimal.TryParse(UnitPriceStorage, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result))
                    return result;
                Console.WriteLine($"Warning: Could not parse UnitPriceStorage '{UnitPriceStorage}' for order {OrderId}");
                return 0m;
            }
            set
            {
                UnitPriceStorage = value.ToString("F2", CultureInfo.InvariantCulture);
                Console.WriteLine($"Setting UnitPrice for order {OrderId}: {value} -> UnitPriceStorage: '{UnitPriceStorage}'");
            }
        }

        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount => UnitPrice * Quantity;

        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Special Instructions")]
        public string? SpecialInstructions { get; set; }

        // Convert to JSON for queue storage
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = false
            });
        }

        // Create from JSON
        public static OrderMessage FromJson(string json)
        {
            var order = JsonSerializer.Deserialize<OrderMessage>(json) ?? new OrderMessage();
            // Ensure table entity properties are set correctly
            order.PartitionKey = "Order";
            order.RowKey = order.OrderId;
            return order;
        }
    }

    public class OrderViewModel
    {
        [Display(Name = "Customer")]
        public string CustomerId { get; set; } = string.Empty;

        [Display(Name = "Product")]
        public string ProductId { get; set; } = string.Empty;

        [Display(Name = "Quantity")]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Special Instructions")]
        public string? SpecialInstructions { get; set; }

        // For dropdowns
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
//-----------------------End Of File----------------//