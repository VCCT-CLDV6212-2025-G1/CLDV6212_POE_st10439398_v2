# CLDV6212_POE_st10439398 - ABC Retail Cloud System
# Complete Portfolio of Evidence: Parts 1, 2, and 3

## Project Overview
A comprehensive cloud-based retail management system demonstrating enterprise-grade integration of Azure Storage Services, Azure SQL Database, and serverless computing. Built with ASP.NET Core MVC for managing customers, products, orders, inventory, and documents with secure user authentication and role-based authorization.

## Student Information
- **Student Number:** st10439398
- **Module:** CLDV6212 - Cloud Development
- **Assignment:** Portfolio of Evidence Parts 1, 2, and 3A
- **Institution:** The Independent Institute of Education (Pty) Ltd

## Live Application
**Deployed URL:** https://abcretail-st10439398-gccda9cgeye9djgw.southafricanorth-01.azurewebsites.net/

## Technology Stack

### Core Technologies
- **Framework:** ASP.NET Core MVC (.NET 8.0)
- **Cloud Platform:** Microsoft Azure
- **Database:** Azure SQL Database (DTU-based, Standard S0)
- **Storage Services:** Azure Tables, Blob Storage, Queue Storage, File Storage
- **Serverless Computing:** Azure Functions (Consumption Plan)
- **Hosting:** Azure App Service (PaaS)
- **IDE:** Microsoft Visual Studio 2022
- **Version Control:** Git/GitHub

### Authentication & Security
- **Authentication:** Cookie-based authentication with ASP.NET Core Identity
- **Password Hashing:** BCrypt.Net (WorkFactor: 11)
- **Authorization:** Role-based access control (Admin/Customer)
- **Security:** HTTPS enforcement, SQL injection prevention, secure cookie policies

## Azure Services Implementation

### 🗄️ Azure SQL Database (Part 3A)
**Purpose:** User authentication, shopping cart, and order management

#### Database Schema:
- **Users Table:** User authentication with encrypted passwords, roles (Admin/Customer), profile information
- **Carts Table:** Shopping cart management linked to user accounts
- **CartItems Table:** Individual cart items with product references and quantities
- **Orders Table:** Completed orders with status tracking (Pending → Processing → PROCESSED → Completed)
- **OrderItems Table:** Line items for each order with pricing snapshots

#### Features:
- ✅ Secure user authentication with BCrypt password hashing
- ✅ Role-based authorization (Admin/Customer)
- ✅ Complete shopping cart system
- ✅ Order processing with status updates
- ✅ Transaction management for data integrity
- ✅ Customer and admin login portals

### 📊 Azure Table Storage (Parts 1 & 2)
**Purpose:** NoSQL storage for product catalog and customer profiles

#### Tables:
- **Customers Table:** Customer profiles and contact information
- **Products Table:** Product catalog with pricing and inventory
- **Orders Table:** Order management and transaction history

#### Features:
- Complete CRUD operations
- Proper partition key strategy
- Decimal price handling for South African Rands
- Fast key-value lookups

### 🖼️ Azure Blob Storage (Parts 1 & 2)
- **Container:** `productimages`
- **Purpose:** Product image storage and display
- **Features:** Image upload, public access, CDN-ready, metadata management

### 📬 Azure Queue Storage (Parts 1 & 2)
#### Queues:
- **Order Processing Queue:** `orderprocessing` - Manages order workflow
- **Inventory Management Queue:** `inventorymanagement` - Tracks stock changes

#### Features:
- Asynchronous order processing
- Automatic inventory updates
- Message retry logic
- Visibility timeout handling

### 📁 Azure Files Storage (Parts 1 & 2)
- **File Share:** `contracts`
- **Purpose:** Business document and contract storage
- **Features:** Hierarchical directory structure, metadata, SMB compatibility

### ⚡ Azure Functions (Part 2)
**Deployed Functions App:** HTTP-triggered serverless functions

#### Functions:
- **StoreToTableFunction:** Stores customer data to Azure Tables
- **WriteToBlobFunction:** Uploads product images to Blob Storage
- **WriteToFilesFunction:** Stores contracts in Azure Files
- **QueueOperationsFunction:** Processes order and inventory queues

## Key Features

### Part 3A: User Authentication & Shopping Cart System

#### Customer Portal
- **Registration & Login:** Secure account creation with email and password
- **Shopping Dashboard:** Browse available products with images and pricing
- **Shopping Cart:**
  - Add products to cart with quantity selection
  - Update quantities or remove items
  - View cart total in South African Rands
  - Linked to authenticated user account
- **Checkout Process:**
  - Enter shipping address and special instructions
  - Create order from cart items
  - Automatic cart clearing after order placement
- **Order History:**
  - View all past orders with status
  - Order details with line items
  - Track order progression

#### Admin Portal
- **Dashboard:** Overview of system statistics and metrics
- **Customer Management:** View and manage all customer accounts
- **Product Management:** CRUD operations for product catalog
- **Order Management:**
  - View all orders across customers
  - Update order status to PROCESSED
  - Process orders from queue
  - SQL-based order reporting
- **Inventory Control:** Stock management and adjustments
- **File Management:** Contract and document administration

### Parts 1 & 2: Core Retail Management

#### Customer Management
- Customer registration and profile management
- Complete CRUD operations via Azure Tables
- Contact information tracking

#### Product Catalog
- Product creation with image upload
- Real-time inventory tracking
- South African Rand (R) pricing
- Category-based organization

#### Order Processing
- Queue-based asynchronous order processing
- Automatic inventory updates
- Status tracking workflow
- Azure Functions integration

#### Inventory Management
- Real-time stock level monitoring
- Queue-based inventory adjustments
- Low stock alerts and restocking
- Comprehensive audit trail

#### File Management
- Contract and document upload to Azure Files
- File categorization with metadata
- Download and deletion capabilities

## Project Structure

```
CLDV6212_POE_st10439398/
├── Controllers/
│   ├── AccountController.cs          # Authentication & registration
│   ├── CustomerController.cs         # Customer CRUD (Admin)
│   ├── ProductController.cs          # Product management (Admin)
│   ├── OrderController.cs            # Order processing (Admin)
│   ├── InventoryController.cs        # Inventory management (Admin)
│   ├── FileController.cs             # File operations (Admin)
│   ├── HomeController.cs             # Dashboard & routing
│   └── CustomerAreaController.cs     # Customer shopping portal
│
├── Models/
│   ├── User.cs                       # SQL Database entity
│   ├── Cart.cs                       # Shopping cart (SQL)
│   ├── CartItem.cs                   # Cart items (SQL)
│   ├── Order.cs                      # Order entity (SQL)
│   ├── OrderItem.cs                  # Order line items (SQL)
│   ├── Customer.cs                   # Azure Table entity
│   ├── Product.cs                    # Azure Table entity
│   ├── OrderMessage.cs               # Queue message entity
│   ├── InventoryMessage.cs           # Queue message entity
│   └── FileUploadModel.cs            # File upload DTO
│
├── Models/ViewModels/
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   ├── CartViewModel.cs
│   ├── CheckoutViewModel.cs
│   ├── CustomerDashboardViewModel.cs
│   └── OrderViewModel.cs
│
├── Services/Interfaces/
│   ├── IAuthService.cs               # Authentication interface
│   ├── ICartService.cs               # Shopping cart interface
│   ├── IOrderService.cs              # Order management interface
│   ├── ITableService.cs              # Azure Tables interface
│   ├── IBlobService.cs               # Blob Storage interface
│   ├── IQueueService.cs              # Queue Storage interface
│   ├── IFileService.cs               # File Storage interface
│   └── IFunctionService.cs           # Azure Functions interface
│
├── Services/Implementations/
│   ├── AuthService.cs                # User authentication & BCrypt
│   ├── CartService.cs                # Cart operations (SQL)
│   ├── OrderService.cs               # Order processing (SQL)
│   ├── TableService.cs               # Azure Table operations
│   ├── BlobService.cs                # Blob Storage operations
│   ├── QueueService.cs               # Queue operations
│   ├── FileService.cs                # File Storage operations
│   └── FunctionService.cs            # Function HTTP calls
│
├── Views/
│   ├── Account/
│   │   ├── Login.cshtml
│   │   ├── Register.cshtml
│   │   └── AccessDenied.cshtml
│   ├── CustomerArea/
│   │   ├── Dashboard.cshtml
│   │   ├── Cart.cshtml
│   │   ├── Checkout.cshtml
│   │   ├── Orders.cshtml
│   │   └── OrderDetails.cshtml
│   ├── Customer/                     # Admin views
│   ├── Product/                      # Admin views
│   ├── Order/                        # Admin views
│   ├── Inventory/                    # Admin views
│   └── File/                         # Admin views
│
└── Program.cs                        # Dependency injection & configuration

CLDV6212_POE_Functions/
├── Program.cs
├── QueueOperationsFunction.cs
├── StoreToTableFunction.cs
├── WriteToBlobFunction.cs
└── WriteToFilesFunction.cs
```

## Installation and Setup

### Prerequisites
- Visual Studio 2022 or later
- .NET 8.0 SDK
- Azure subscription with available credit
- Git
- SQL Server Management Studio (optional, for database management)

### Local Development Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/yourusername/CLDV6212_POE_st10439398.git
   cd CLDV6212_POE_st10439398
   ```

2. **Configure Azure Services:**
   Create an `appsettings.json` with:
   ```json
   {
     "ConnectionStrings": {
       "AzureStorage": "YOUR_STORAGE_CONNECTION_STRING",
       "SqlConnection": "YOUR_SQL_CONNECTION_STRING"
     },
     "AzureSettings": {
       "StorageAccountName": "YOUR_ACCOUNT_NAME",
       "BlobContainerName": "productimages",
       "QueueName": "orderprocessing",
       "InventoryQueueName": "inventorymanagement",
       "FileShareName": "contracts",
       "TableName": {
         "Customers": "Customers",
         "Products": "Products",
         "Orders": "Orders"
       }
     },
     "AzureFunctions": {
       "BaseUrl": "YOUR_FUNCTION_APP_URL",
       "FunctionKey": "YOUR_FUNCTION_KEY"
     }
   }
   ```

3. **Set up Azure SQL Database:**
   Run the following SQL scripts to create required tables:
   ```sql
   -- Users Table
   CREATE TABLE Users (
       UserId INT PRIMARY KEY IDENTITY(1,1),
       Email NVARCHAR(255) UNIQUE NOT NULL,
       PasswordHash NVARCHAR(500) NOT NULL,
       FirstName NVARCHAR(100) NOT NULL,
       LastName NVARCHAR(100) NOT NULL,
       Phone NVARCHAR(20),
       Role NVARCHAR(20) NOT NULL DEFAULT 'Customer',
       IsActive BIT NOT NULL DEFAULT 1,
       CreatedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
       LastLoginDate DATETIME,
       Address NVARCHAR(500),
       City NVARCHAR(100)
   );

   -- Carts Table
   CREATE TABLE Carts (
       CartId INT PRIMARY KEY IDENTITY(1,1),
       UserId INT NOT NULL,
       CreatedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
       LastModifiedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
       FOREIGN KEY (UserId) REFERENCES Users(UserId)
   );

   -- CartItems Table
   CREATE TABLE CartItems (
       CartItemId INT PRIMARY KEY IDENTITY(1,1),
       CartId INT NOT NULL,
       ProductId NVARCHAR(100) NOT NULL,
       ProductName NVARCHAR(255) NOT NULL,
       Quantity INT NOT NULL DEFAULT 1,
       UnitPrice DECIMAL(18,2) NOT NULL,
       AddedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
       FOREIGN KEY (CartId) REFERENCES Carts(CartId) ON DELETE CASCADE
   );

   -- Orders Table
   CREATE TABLE Orders (
       OrderId INT PRIMARY KEY IDENTITY(1,1),
       UserId INT NOT NULL,
       OrderDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
       Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
       TotalAmount DECIMAL(18,2) NOT NULL,
       ShippingAddress NVARCHAR(500),
       SpecialInstructions NVARCHAR(1000),
       ProcessedDate DATETIME,
       FOREIGN KEY (UserId) REFERENCES Users(UserId)
   );

   -- OrderItems Table
   CREATE TABLE OrderItems (
       OrderItemId INT PRIMARY KEY IDENTITY(1,1),
       OrderId INT NOT NULL,
       ProductId NVARCHAR(100) NOT NULL,
       ProductName NVARCHAR(255) NOT NULL,
       Quantity INT NOT NULL,
       UnitPrice DECIMAL(18,2) NOT NULL,
       FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE
   );
   ```

4. **Run the application:**
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```

5. **Access the application:**
   - Navigate to `https://localhost:7001` (or your configured port)
   - Register a new account or use default credentials


### Test Data Requirements:
- ✅ 5+ customer records (Azure Tables)
- ✅ 5+ product records with images (Azure Tables + Blob)
- ✅ 5+ user accounts (SQL Database)
- ✅ 5+ orders (SQL Database)
- ✅ 5+ order records in queue (Azure Queue)
- ✅ 5+ inventory adjustments (Azure Queue)
- ✅ 5+ uploaded contract files (Azure Files)

## Usage Instructions

### For Customers:

#### 1. Registration and Login
1. Navigate to the registration page
2. Fill in personal details (name, email, password)
3. Submit to create account
4. Login with email and password

#### 2. Shopping
1. Browse products on the dashboard
2. Click "Add to Cart" on desired products
3. Adjust quantities as needed
4. View cart summary

#### 3. Checkout
1. Navigate to Cart
2. Review items and total
3. Click "Proceed to Checkout"
4. Enter shipping address
5. Add special instructions (optional)
6. Place order

#### 4. Order Tracking
1. Go to "My Orders"
2. View order history with statuses
3. Click order for detailed view
4. Track order progression

### For Administrators:

#### 1. Login
- Use admin credentials to access admin portal
- Dashboard shows system overview

#### 2. Customer Management
1. Navigate to Customers
2. View all registered customers
3. Create new customer records
4. Edit or delete customer information

#### 3. Product Management
1. Go to Products
2. Add new products with images
3. Update pricing and inventory
4. Manage product availability

#### 4. Order Processing
1. View all customer orders
2. Process orders from queue
3. Update order status to PROCESSED
4. Monitor order completion

#### 5. Inventory Management
1. Monitor stock levels
2. Process restocking requests
3. Adjust inventory via queue
4. View inventory change history

#### 6. File Management
1. Upload business documents
2. Categorize files by type
3. Download or delete files

## API Endpoints

### Authentication Routes
- `GET /Account/Login` - Login page
- `POST /Account/Login` - Process login
- `GET /Account/Register` - Registration page
- `POST /Account/Register` - Create account
- `POST /Account/Logout` - End session

### Customer Portal Routes
- `GET /CustomerArea/Dashboard` - Shopping dashboard
- `POST /CustomerArea/AddToCart` - Add item to cart
- `GET /CustomerArea/Cart` - View cart
- `POST /CustomerArea/UpdateCartItem` - Update quantity
- `GET /CustomerArea/Checkout` - Checkout page
- `POST /CustomerArea/PlaceOrder` - Process order
- `GET /CustomerArea/Orders` - Order history
- `GET /CustomerArea/OrderDetails/{id}` - Order details

### Admin Routes
- `GET /Customer` - Customer management
- `GET /Product` - Product catalog
- `GET /Order` - Order processing
- `GET /Order/SqlOrders` - SQL-based orders
- `POST /Order/UpdateStatus` - Update order status
- `GET /Inventory` - Inventory management
- `GET /File` - File operations

## Security Features

### Authentication & Authorization
- ✅ BCrypt password hashing (WorkFactor: 11)
- ✅ Cookie-based authentication with secure policies
- ✅ Role-based authorization (Admin/Customer)
- ✅ HTTPS enforcement in production
- ✅ Anti-forgery tokens on all forms
- ✅ Secure session management

### Data Protection
- ✅ SQL injection prevention via parameterized queries
- ✅ Input validation on all user inputs
- ✅ XSS protection with Razor encoding
- ✅ Connection string encryption
- ✅ Azure-managed service authentication

### Session Management
- ✅ 2-hour session timeout
- ✅ Sliding expiration enabled
- ✅ Secure cookie policies
- ✅ HttpOnly cookies
- ✅ SameSite=Lax protection

## Error Handling
- Comprehensive try-catch blocks in all services
- User-friendly error messages via TempData
- Detailed logging for debugging
- Graceful degradation on service failures
- Transaction rollback on database errors

## Performance Optimizations
- Async/await patterns throughout
- Connection pooling for SQL Database
- Efficient Azure SDK usage
- Minimal round-trips to storage
- Proper disposal of resources

## Testing

### Manual Testing Performed
- ✅ User registration and login flows
- ✅ Role-based authorization (Admin/Customer)
- ✅ Shopping cart operations (add, update, remove)
- ✅ Checkout and order placement
- ✅ Order status updates by admin
- ✅ Product CRUD operations
- ✅ Customer CRUD operations
- ✅ Inventory adjustments
- ✅ File upload and download
- ✅ Queue message processing
- ✅ Azure Functions integration

### Test Accounts
```
Admin Account:
Email: admin@abcretail.com
Password: Admin123!

Customer Account:
Email: morgan@example.com
Password: Morgan@2005
```


## Screenshots and Documentation
For complete project documentation including screenshots of:
- Azure services configuration
- Database schema
- Application functionality
- Deployment process

Refer to the POE submission documents.

## Deployment

### Azure App Service Deployment
1. Publish from Visual Studio
2. Configure connection strings in Azure Portal
3. Enable HTTPS only
4. Set up custom domain (optional)
5. Configure CI/CD pipeline (optional)

### Azure Functions Deployment
1. Publish Functions project
2. Configure application settings
3. Set AzureWebJobsStorage connection string
4. Test HTTP triggers

## Troubleshooting

### Common Issues:

**Issue:** Can't connect to Azure SQL Database
**Solution:** Add your IP to firewall rules in Azure Portal

**Issue:** Images not displaying
**Solution:** Verify Blob container has public access enabled

**Issue:** Orders not processing
**Solution:** Check Queue Storage connection string and message format

**Issue:** Function calls failing
**Solution:** Verify Function App is running and key is correct

## License
This project is submitted as part of academic coursework for CLDV6212.

## Contact
- **Student:** st10439398
- **Institution:** The Independent Institute of Education
- **Module:** CLDV6212 - Cloud Development

## Acknowledgments
- The Independent Institute of Education
- Microsoft Azure Documentation
- ASP.NET Core Documentation
- Course instructors and lecturers

---

**Last Updated:** November 2025
**Version:** 3.0 (Parts 1, 2, and 3A Complete)
**Status:** ✅ Fully Functional and Deployed
