//----------Start of file-----------//
using CLDV6212_POE_st10439398.Services.Interfaces;
using CLDV6212_POE_st10439398.Services.Implementations;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Files.Shares;
using Azure.Data.Tables;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Get Azure Storage connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("AzureStorage");

// Validate connection string exists
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Azure Storage connection string is not configured. Please check your appsettings.json file.");
}

// Register Azure Storage clients with connection string
builder.Services.AddSingleton(serviceProvider => new BlobServiceClient(connectionString));
builder.Services.AddSingleton(serviceProvider => new QueueServiceClient(connectionString));
builder.Services.AddSingleton(serviceProvider => new ShareServiceClient(connectionString));
builder.Services.AddSingleton(serviceProvider => new TableServiceClient(connectionString));

// Register Azure configuration settings
builder.Services.Configure<AzureSettings>(
    builder.Configuration.GetSection("AzureSettings"));

// Register custom service interfaces with their implementations
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<IBlobService, BlobService>();
builder.Services.AddScoped<IQueueService, QueueService>();
builder.Services.AddScoped<IFileService, FileService>();

// Register HttpClient and FunctionService - ADD THIS LINE
builder.Services.AddHttpClient<IFunctionService, FunctionService>();

// Add logging services
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialize tables on startup
using (var scope = app.Services.CreateScope())
{
    var tableService = scope.ServiceProvider.GetRequiredService<ITableService>();
    await tableService.CreateTablesIfNotExistsAsync();
}

app.Run();

// Configuration classes for Azure settings
public class AzureSettings
{
    public string StorageAccountName { get; set; } = string.Empty;
    public string StorageAccountKey { get; set; } = string.Empty;
    public string BlobContainerName { get; set; } = string.Empty;
    public TableNames TableName { get; set; } = new TableNames();
    public string QueueName { get; set; } = string.Empty;
    public string InventoryQueueName { get; set; } = string.Empty;
    public string FileShareName { get; set; } = string.Empty;
}

public class TableNames
{
    public string Customers { get; set; } = string.Empty;
    public string Products { get; set; } = string.Empty;
    public string Orders { get; set; } = string.Empty;
}
//------------End of file------------//