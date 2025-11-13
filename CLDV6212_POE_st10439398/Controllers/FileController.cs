//-----------------------Start of File-----------//
using Microsoft.AspNetCore.Mvc;
using CLDV6212_POE_st10439398.Models;
using CLDV6212_POE_st10439398.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace CLDV6212_POE_st10439398.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FileController : Controller
    {
        private readonly IFileService _fileService;
        private readonly IFunctionService _functionService;

        public FileController(IFileService fileService, IFunctionService functionService)
        {
            _fileService = fileService;
            _functionService = functionService;
        }

        // GET: File
        public async Task<IActionResult> Index()
        {
            var files = await _fileService.GetAllFilesAsync();
            return View(files);
        }

        // GET: File/Upload
        public IActionResult Upload()
        {
            return View();
        }

        // POST: File/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(FileUploadModel model)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
            }

            if (ModelState.IsValid && model.File != null)
            {
                var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{model.File.FileName}";

                // Call Azure Function to upload file
                var functionSuccess = await _functionService.UploadContractFileAsync(
                    model.File,
                    fileName,
                    model.Description ?? "",
                    model.ContractType);

                if (functionSuccess)
                {
                    TempData["SuccessMessage"] = "File uploaded successfully via Azure Function!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Azure Function call failed. Please try again.";
                }
            }
            else if (model.File == null)
            {
                ModelState.AddModelError("File", "Please select a file to upload.");
            }

            return View(model);
        }

        // GET: File/Download/filename
        public async Task<IActionResult> Download(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound();
            }

            var fileStream = await _fileService.DownloadFileAsync(fileName);
            if (fileStream == null)
            {
                TempData["ErrorMessage"] = "File not found or could not be downloaded.";
                return RedirectToAction(nameof(Index));
            }

            var fileInfo = await _fileService.GetFileInfoAsync(fileName);
            var contentType = fileInfo?.ContentType ?? "application/octet-stream";

            return File(fileStream, contentType, fileName);
        }

        // GET: File/Details/filename
        public async Task<IActionResult> Details(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound();
            }

            var fileInfo = await _fileService.GetFileInfoAsync(fileName);
            if (fileInfo == null)
            {
                return NotFound();
            }

            return View(fileInfo);
        }

        // GET: File/Delete/filename
        public async Task<IActionResult> Delete(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound();
            }

            var fileInfo = await _fileService.GetFileInfoAsync(fileName);
            if (fileInfo == null)
            {
                return NotFound();
            }

            return View(fileInfo);
        }

        // POST: File/Delete/filename
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string fileName)
        {
            var success = await _fileService.DeleteFileAsync(fileName);
            if (success)
            {
                TempData["SuccessMessage"] = "File deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete file. Please try again.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: File/CheckExists
        [HttpPost]
        public async Task<IActionResult> CheckExists(string fileName)
        {
            var exists = await _fileService.FileExistsAsync(fileName);
            return Json(new { exists });
        }
    }
}
//-----------------------End Of File----------------//