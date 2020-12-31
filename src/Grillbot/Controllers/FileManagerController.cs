using Grillbot.Models.FileManager;
using Grillbot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Threading.Tasks;

namespace Grillbot.Controllers
{
    [Authorize]
    [Route("Admin/Files")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class FileManagerController : Controller
    {
        private FileManagerService FileService { get; }
        private FileExtensionContentTypeProvider ContentTypeProvider { get; }

        public FileManagerController(FileManagerService fileService, FileExtensionContentTypeProvider contentTypeProvider)
        {
            FileService = fileService;
            ContentTypeProvider = contentTypeProvider;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var files = await FileService.GetFilesAsync();
            return View(new FileManagerViewModel(files));
        }

        [HttpGet("Download")]
        public async Task<IActionResult> DownloadAsync(string filename)
        {
            var file = await FileService.GetFileAsync(filename);

            if (file == null)
                return NotFound();

            var contentType = ContentTypeProvider.TryGetContentType(file.Filename, out string type) ? type : "application/octet-stream";
            return File(file.Content, contentType);
        }

        [HttpGet("Delete")]
        public async Task<IActionResult> DeleteAsync(string filename)
        {
            await FileService.DeleteFileAsync(filename);
            return RedirectToAction("Index");
        }
    }
}
