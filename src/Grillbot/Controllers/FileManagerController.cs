using Grillbot.FileSystem;
using Grillbot.FileSystem.Entities;
using Grillbot.Models.FileManager;
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
        private FileExtensionContentTypeProvider ContentTypeProvider { get; }
        private IFileSystemRepository Repository { get; }

        public FileManagerController(FileExtensionContentTypeProvider contentTypeProvider, IFileSystemRepository repository)
        {
            ContentTypeProvider = contentTypeProvider;
            Repository = repository;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var files = Repository.GetAllFiles();
            return View(new FileManagerViewModel(files));
        }

        [HttpGet("Download")]
        public async Task<IActionResult> DownloadAsync(string directory, string filename)
        {
            FileSystemEntity entity = null;
            switch(directory)
            {
                case "AuditLogs":
                    entity = Repository.AuditLogs.GetFileByFilename(filename);
                    break;
            }

            if (entity == null)
                return NotFound();

            var contentType = ContentTypeProvider.TryGetContentType(entity.Filename, out string type) ? type : "application/octet-stream";
            return File(entity.Content, contentType);
        }

        [HttpGet("Delete")]
        public async Task<IActionResult> DeleteAsync(string filename, string directory)
        {
            switch(directory)
            {
                case "AuditLogs":
                    Repository.AuditLogs.RemoveFile(filename);
                    break;
            }

            return RedirectToAction("Index");
        }
    }
}
