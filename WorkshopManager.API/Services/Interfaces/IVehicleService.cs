using WorkshopManager.API.Models;

namespace WorkshopManager.API.Services.Interfaces;

public interface IVehicleService
{
    Task<IEnumerable<Vehicle>> GetAllVehiclesAsync();
    Task<Vehicle?> GetVehicleByIdAsync(int id);
    Task<Vehicle?> GetVehicleByVinAsync(string vin);
    Task<Vehicle> CreateVehicleAsync(Vehicle vehicle);
    Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle);
    Task DeleteVehicleAsync(int id);
    Task<string> UploadVehiclePhotoAsync(int vehicleId, Stream photoStream, string fileName);
    Task<IEnumerable<ServiceOrder>> GetVehicleServiceHistoryAsync(int vehicleId);
} 