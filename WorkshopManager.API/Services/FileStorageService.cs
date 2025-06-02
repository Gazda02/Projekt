using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;

namespace WorkshopManager.API.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _uploadDirectory;
        private readonly ApplicationDbContext _context;

        public FileStorageService(IConfiguration configuration, ApplicationDbContext context)
        {
            _uploadDirectory = configuration.GetValue<string>("FileStorage:UploadDirectory") 
                ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            _context = context;

            if (!Directory.Exists(_uploadDirectory))
            {
                Directory.CreateDirectory(_uploadDirectory);
            }
        }

        public async Task<FileUpload> SaveFileAsync(IFormFile file, int? serviceOrderId = null, int? vehicleId = null)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null");
            }

            // Create a unique filename
            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            string filePath = Path.Combine(_uploadDirectory, uniqueFileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create and save file metadata
            var fileUpload = new FileUpload
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                FilePath = uniqueFileName,
                FileSize = file.Length,
                UploadDate = DateTime.UtcNow,
                ServiceOrderId = serviceOrderId,
                VehicleId = vehicleId
            };

            _context.FileUploads.Add(fileUpload);
            await _context.SaveChangesAsync();

            return fileUpload;
        }

        public async Task<byte[]> GetFileAsync(int fileId)
        {
            var fileInfo = await GetFileInfoAsync(fileId);
            if (fileInfo == null)
            {
                throw new FileNotFoundException("File not found");
            }

            string filePath = Path.Combine(_uploadDirectory, fileInfo.FilePath);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Physical file not found");
            }

            return await File.ReadAllBytesAsync(filePath);
        }

        public async Task DeleteFileAsync(int fileId)
        {
            var fileInfo = await GetFileInfoAsync(fileId);
            if (fileInfo == null)
            {
                throw new FileNotFoundException("File not found");
            }

            string filePath = Path.Combine(_uploadDirectory, fileInfo.FilePath);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            _context.FileUploads.Remove(fileInfo);
            await _context.SaveChangesAsync();
        }

        public async Task<FileUpload> GetFileInfoAsync(int fileId)
        {
            return await _context.FileUploads.FindAsync(fileId);
        }
    }
} 