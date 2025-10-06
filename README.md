# CLDV6212_POE_st10439398_v2
# ABC Retail Cloud System - CLDV6212 POE Part 1 and 2

## Project Overview
A comprehensive retail management web application demonstrating the integration of all four Azure Storage services. Built with ASP.NET Core MVC for managing customers, products, orders, inventory, and documents in a cloud-based retail environment.

## Student Information
- **Student Number:** st10439398
- **Module:** CLDV6212 - Cloud Development
- **Assignment:** Portfolio of Evidence Part 1

## Live Application **Deployed URL:** https://abcretail-st10439398-gccda9cgeye9djgw.southafricanorth-01.azurewebsites.net/


## Technology Stack
- **Framework:** ASP.NET Core MVC (.NET 6+)
- **Cloud Platform:** Microsoft Azure
- **Storage Services:** Azure Tables, Blob Storage, Queue Storage, File Storage
- **Hosting:** Azure App Service
- **IDE:** Microsoft Visual Studio
- **Version Control:** Git/GitHub

## Azure Storage Services Implementation

###  Azure Table Storage
- **Customers Table:** Customer profiles and contact information
- **Products Table:** Product catalog with pricing and inventory
- **Orders Table:** Order management and transaction history
- **Features:** Complete CRUD operations, proper partitioning, decimal price handling

###  Azure Blob Storage  
- **Container:** `productimages`
- **Purpose:** Product image storage and display
- **Features:** Image upload, public access for web display, file management

### Azure Queue Storage
- **Order Processing Queue:** `orderprocessing` - Manages order workflow
- **Inventory Management Queue:** `inventorymanagement` - Tracks stock changes
- **Features:** Asynchronous processing, message queuing, workflow automation

### Azure Files Storage
- **File Share:** `contracts`
- **Purpose:** Business document and contract storage
- **Features:** File upload/download, metadata management, document categorization

## Key Features

### Customer Management
- Customer registration and profile management
- Complete CRUD operations
- Contact information tracking

### Product Catalog
- Product creation with image upload
- Inventory tracking with stock levels
- South African Rand (R) pricing
- Category-based organization

### Order Processing
- Order creation linking customers and products
- Queue-based processing workflow
- Automatic inventory updates
- Status tracking (Pending → Processing → Completed)

### Inventory Management
- Real-time inventory tracking
- Queue-based stock adjustments
- Low stock alerts and restocking
- Comprehensive audit trail

### File Management
- Contract and document upload
- File categorization and metadata
- Download and deletion capabilities


## Project Structure
```
CLDV6212_POE_st10439398/
├── Controllers/
│   ├── CustomerController.cs
│   ├── ProductController.cs
│   ├── OrderController.cs
│   ├── InventoryController.cs
│   └── FileController.cs
├── Models/
│   ├── Customer.cs
│   ├── Product.cs
│   ├── OrderMessage.cs
│   ├── InventoryMessage.cs
│   └── FileUploadModel.cs
├── Services/
│   ├── Interfaces/
│   └── Implementations/
│       ├── TableService.cs
│       ├── BlobService.cs
│       ├── QueueService.cs
│       └── FileService.cs
├── Views/
│   ├── Customer/
│   ├── Product/
│   ├── Order/
│   ├── Inventory/
│   └── File/
└── wwwroot/
```

## Installation and Setup

### Prerequisites
- Visual Studio 2022 or later
- .NET 6+ SDK
- Azure subscription with available credit
- Git

### Local Development Setup
1. **Clone the repository:**
   ```bash
   git clone [YOUR-REPOSITORY-URL]
   cd CLDV6212_POE_st10439398
   ```



3. **Run the application:**
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```

## Azure Storage Configuration

### Required Storage Services:
- **4 Tables:** Customers, Products, Orders, (additional if needed)
- **1 Blob Container:** productimages (public access)
- **2 Queues:** orderprocessing, inventorymanagement  
- **1 File Share:** contracts

### Test Data Requirements:
- 5+ customer records
- 5+ product records with images
- 5+ order records
- 5+ inventory adjustments
- 5+ uploaded contract files

## Usage Instructions

### Creating Customers
1. Navigate to Customers → Add New
2. Fill in customer details
3. Save to Azure Table Storage

### Managing Products
1. Go to Products → Add New Product
2. Upload product image (stored in Blob)
3. Set pricing in South African Rands
4. Manage inventory levels

### Processing Orders
1. Create order linking customer and product
2. Order queued for processing
3. Inventory automatically reduced
4. Status tracked through completion

### Inventory Management
1. Monitor stock levels
2. Process restocking through queue
3. Handle manual adjustments
4. View inventory change audit trail

### File Management
1. Upload contracts and documents
2. Store in Azure Files with metadata
3. Download and manage business documents

## API Endpoints
The application uses MVC pattern with the following main routes:
- `/Customer` - Customer management
- `/Product` - Product catalog
- `/Order` - Order processing
- `/Inventory` - Inventory management
- `/File` - File operations

## Error Handling
- Comprehensive try-catch blocks
- User-friendly error messages
- Logging for debugging
- Graceful degradation

## Security Features
- Secure connection string storage
- Input validation on all forms
- Proper error handling
- Azure service authentication

## Testing
- Manual testing of all CRUD operations

## Screenshots and Documentation
For complete project documentation including screenshots of Azure services and application functionality, refer to the POE Part 1 submission document.
