//-----------------------Start Of File----------------//
using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace CLDV6212_POE_st10439398.Models
{
    public class Product : ITableEntity
    {
        public Product()
        {
            PartitionKey = "Product";
            RowKey = Guid.NewGuid().ToString();
            Timestamp = DateTimeOffset.UtcNow;
            PriceStorage = "0.00"; // Initialize the storage property
        }

        [Required]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        // This is what Azure Table Storage will store
        public string PriceStorage { get; set; } = "0.00";

      
        [Required]
        [Display(Name = "Price")]
        [System.Text.Json.Serialization.JsonIgnore] // Ignore in JSON serialization
        public decimal Price
        {
            get
            {
                // More robust parsing with detailed logging
                if (string.IsNullOrWhiteSpace(PriceStorage))
                {
                    Console.WriteLine($"WARNING: PriceStorage is null or empty for product {Name}");
                    return 0m;
                }

                
                if (decimal.TryParse(PriceStorage, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result))
                {
                    Console.WriteLine($"Successfully parsed price '{PriceStorage}' as {result} for product {Name}");
                    return result;
                }

                // Try parsing with current culture
                if (decimal.TryParse(PriceStorage, NumberStyles.Number, CultureInfo.CurrentCulture, out result))
                {
                    Console.WriteLine($"Successfully parsed price '{PriceStorage}' as {result} with current culture for product {Name}");
                    return result;
                }

                // Try parsing as a simple number (remove any commas, spaces)
                var cleanPrice = PriceStorage.Replace(",", "").Replace(" ", "").Replace("R", "");
                if (decimal.TryParse(cleanPrice, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
                {
                    Console.WriteLine($"Successfully parsed cleaned price '{cleanPrice}' (from '{PriceStorage}') as {result} for product {Name}");
                    return result;
                }

                Console.WriteLine($"ERROR: Could not parse price '{PriceStorage}' for product {Name}. Returning 0.");
                return 0m;
            }
            set
            {
                PriceStorage = value.ToString("F2", CultureInfo.InvariantCulture);
                Console.WriteLine($"Setting price for product {Name}: {value} -> PriceStorage: '{PriceStorage}'");
            }
        }

        [Required]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; } = 0;

        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; } = string.Empty;

        [Display(Name = "Is Available")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;


        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        [Display(Name = "Product ID")]
        public string ProductId => RowKey;

        public string PriceInRand => $"R {Price:N2}";


        public string GetPriceDebugInfo()
        {
            return $"PriceStorage: '{PriceStorage}', Price: {Price}, PriceInRand: '{PriceInRand}'";
        }
    }
}
//-----------------------End Of File----------------//