using System;

namespace WorkshopManager.API.Models
{
    public class FileUpload
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public int? ServiceOrderId { get; set; }
        public int? VehicleId { get; set; }

        // Navigation properties
        public ServiceOrder ServiceOrder { get; set; }
        public Vehicle Vehicle { get; set; }
    }
} 