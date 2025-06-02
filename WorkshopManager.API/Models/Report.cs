using System;

namespace WorkshopManager.API.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }  // e.g., "ServiceOrder", "VehicleHistory", "Financial"
        public DateTime GeneratedDate { get; set; }
        public string FilePath { get; set; }
        public string Format { get; set; }  // e.g., "PDF", "Excel"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? VehicleId { get; set; }
        public int? ServiceOrderId { get; set; }
        public int? CustomerId { get; set; }

        // Navigation properties
        public Vehicle Vehicle { get; set; }
        public ServiceOrder ServiceOrder { get; set; }
        public Customer Customer { get; set; }
    }
} 