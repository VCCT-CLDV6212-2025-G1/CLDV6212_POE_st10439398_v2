//-----------------------Start of File-----------------------//
namespace CLDV6212_POE_st10439398.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int CustomerCount { get; set; }
        public int ProductCount { get; set; }
        public int QueueLength { get; set; }
        public int FileCount { get; set; }
        public int BlobCount { get; set; }

        public IEnumerable<Customer> RecentCustomers { get; set; } = new List<Customer>();
        public IEnumerable<Product> RecentProducts { get; set; } = new List<Product>();
        public IEnumerable<OrderMessage> RecentOrders { get; set; } = new List<OrderMessage>();
        public IEnumerable<FileInfoModel> RecentFiles { get; set; } = new List<FileInfoModel>();

        public bool HasAzureConnectionError { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class ServiceStatusViewModel
    {
        public string ServiceName { get; set; } = string.Empty;
        public bool IsOperational { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public string LastUpdated { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string BadgeColor { get; set; } = "primary";
    }
}
//-----------------------End Of File----------------//