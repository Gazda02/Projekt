using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WorkshopManager.API.Services;

namespace WorkshopManager.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;

        public FileUploadController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(
            IFormFile file,
            [FromQuery] int? serviceOrderId = null,
            [FromQuery] int? vehicleId = null)
        {
            try
            {
                var fileUpload = await _fileStorageService.SaveFileAsync(file, serviceOrderId, vehicleId);
                return Ok(fileUpload);
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{fileId}")]
        public async Task<IActionResult> GetFile(int fileId)
        {
            try
            {
                var fileInfo = await _fileStorageService.GetFileInfoAsync(fileId);
                if (fileInfo == null)
                {
                    return NotFound();
                }

                var fileBytes = await _fileStorageService.GetFileAsync(fileId);
                return File(fileBytes, fileInfo.ContentType, fileInfo.FileName);
            }
            catch (System.IO.FileNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{fileId}")]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(fileId);
                return NoContent();
            }
            catch (System.IO.FileNotFoundException)
            {
                return NotFound();
            }
        }
    }
} 