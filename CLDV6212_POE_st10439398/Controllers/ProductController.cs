//-----------------------Start of File-----------------------//
using Microsoft.AspNetCore.Mvc;
using CLDV6212_POE_st10439398.Models;
using CLDV6212_POE_st10439398.Services.Interfaces;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;

namespace CLDV6212_POE_st10439398.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ITableService _tableService;
        private readonly IBlobService _blobService;
        private readonly IFunctionService _functionService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ITableService tableService, IBlobService blobService, IFunctionService functionService, ILogger<ProductController> logger)
        {
            _tableService = tableService;
            _blobService = blobService;
            _functionService = functionService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _tableService.GetAllProductsAsync();
            return View(products);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var product = await _tableService.GetProductAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            var priceParam = Request.Form["Price"].ToString();
            if (!string.IsNullOrEmpty(priceParam) && decimal.TryParse(priceParam, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsedPrice) && parsedPrice > 0)
            {
                product.Price = parsedPrice;
            }
            else
            {
                ModelState.AddModelError("Price", "Valid price required.");
            }

            if (ModelState.IsValid)
            {
                product.RowKey = Guid.NewGuid().ToString();
                product.PartitionKey = "Product";
                product.CreatedDate = DateTime.UtcNow;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = $"{product.ProductId}_{Path.GetFileName(imageFile.FileName)}";
                    await _functionService.UploadProductImageAsync(imageFile, fileName);
                    var imageUrl = await _blobService.UploadImageAsync(imageFile, fileName);
                    if (!string.IsNullOrEmpty(imageUrl)) product.ImageUrl = imageUrl;
                }

                if (await _tableService.AddProductAsync(product))
                {
                    TempData["SuccessMessage"] = "Product created!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = "Failed to create product.";
            }
            return View(product);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var product = await _tableService.GetProductAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Product product, IFormFile? imageFile)
        {
            if (id != product.RowKey) return NotFound();

            var priceParam = Request.Form["Price"].ToString();
            if (!string.IsNullOrEmpty(priceParam) && decimal.TryParse(priceParam, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsedPrice) && parsedPrice > 0)
            {
                product.Price = parsedPrice;
            }

            if (ModelState.IsValid)
            {
                var existing = await _tableService.GetProductAsync(id);
                if (existing == null) return NotFound();

                existing.Name = product.Name;
                existing.Description = product.Description;
                existing.Price = product.Price;
                existing.Category = product.Category;
                existing.StockQuantity = product.StockQuantity;
                existing.IsAvailable = product.IsAvailable;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = $"{existing.ProductId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Path.GetFileName(imageFile.FileName)}";
                    await _functionService.UploadProductImageAsync(imageFile, fileName);
                    var imageUrl = await _blobService.UploadImageAsync(imageFile, fileName);
                    if (!string.IsNullOrEmpty(imageUrl)) existing.ImageUrl = imageUrl;
                }

                if (await _tableService.UpdateProductAsync(existing))
                {
                    TempData["SuccessMessage"] = "Product updated!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = "Failed to update.";
            }
            return View(product);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var product = await _tableService.GetProductAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var success = await _tableService.DeleteProductAsync(id);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Deleted!" : "Failed.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Images()
        {
            return View(await _blobService.GetAllBlobsAsync());
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(string blobName)
        {
            var success = await _blobService.DeleteBlobAsync(blobName);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Image deleted!" : "Failed.";
            return RedirectToAction(nameof(Images));
        }
    }
}
//-----------------------End Of File----------------//