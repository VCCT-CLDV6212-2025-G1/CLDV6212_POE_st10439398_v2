//----------Start of file-----------//
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Storage.Queues;
using CLDV6212_POE_st10439398.Services.Implementations;
using CLDV6212_POE_st10439398.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

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

// Register HttpClient and FunctionService
builder.Services.AddHttpClient<IFunctionService, FunctionService>();

//  SQL-BASED AUTHENTICATION & CART SERVICES 
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// AUTHENTICATION CONFIGURATION
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.Cookie.Name = "ABCRetail.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;

        // CRITICAL: Configure logout behavior
        options.Events.OnSigningOut = async context =>
        {
            // Delete the authentication cookie
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        };
    });

// Add Authorization services with role-based policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
});

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

// Add Session BEFORE Authentication
app.UseSession();

app.UseAuthentication();
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