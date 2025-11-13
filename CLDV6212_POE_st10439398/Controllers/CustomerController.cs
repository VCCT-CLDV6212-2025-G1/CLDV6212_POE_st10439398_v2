//-----------------------Start of file------------------------//
using Microsoft.AspNetCore.Mvc;
using CLDV6212_POE_st10439398.Models;
using CLDV6212_POE_st10439398.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace CLDV6212_POE_st10439398.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CustomerController : Controller
    {
        private readonly ITableService _tableService;
        private readonly IFunctionService _functionService;

        public CustomerController(ITableService tableService, IFunctionService functionService)
        {
            _tableService = tableService;
            _functionService = functionService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _tableService.GetAllCustomersAsync());
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var customer = await _tableService.GetCustomerAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        public IActionResult Create() => View();
        
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.RowKey = Guid.NewGuid().ToString();
                customer.PartitionKey = "Customer";
                customer.CreatedDate = DateTime.UtcNow;

                if (await _functionService.StoreCustomerToTableAsync(customer))
                {
                    TempData["SuccessMessage"] = "Customer created via Azure Function!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = "Function call failed.";
            }
            return View(customer);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var customer = await _tableService.GetCustomerAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Customer customer)
        {
            if (id != customer.RowKey) return NotFound();

            if (ModelState.IsValid)
            {
                var existing = await _tableService.GetCustomerAsync(id);
                if (existing == null) return NotFound();

                existing.FirstName = customer.FirstName;
                existing.LastName = customer.LastName;
                existing.Email = customer.Email;
                existing.Phone = customer.Phone;
                existing.Address = customer.Address;
                existing.City = customer.City;

                if (await _tableService.UpdateCustomerAsync(existing))
                {
                    TempData["SuccessMessage"] = "Updated!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = "Failed to update.";
            }
            return View(customer);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var customer = await _tableService.GetCustomerAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var success = await _tableService.DeleteCustomerAsync(id);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Deleted!" : "Failed.";
            return RedirectToAction(nameof(Index));
        }
    }
}
//-----------------------End of File----------------//