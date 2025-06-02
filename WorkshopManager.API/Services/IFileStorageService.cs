using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using WorkshopManager.API.Models;

namespace WorkshopManager.API.Services
{
    public interface IFileStorageService
    {
        Task<FileUpload> SaveFileAsync(IFormFile file, int? serviceOrderId = null, int? vehicleId = null);
        Task<byte[]> GetFileAsync(int fileId);
        Task DeleteFileAsync(int fileId);
        Task<FileUpload> GetFileInfoAsync(int fileId);
    }
} 