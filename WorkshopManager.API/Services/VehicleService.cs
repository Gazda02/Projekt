using Microsoft.EntityFrameworkCore;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;
using WorkshopManager.API.Services.Interfaces;

namespace WorkshopManager.API.Services;

public class VehicleService : IVehicleService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<VehicleService> _logger;

    public VehicleService(ApplicationDbContext context, ILogger<VehicleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
    {
        try
        {
            return await _context.Vehicles
                .Include(v => v.Customer)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all vehicles");
            throw;
        }
    }

    public async Task<Vehicle?> GetVehicleByIdAsync(int id)
    {
        try
        {
            return await _context.Vehicles
                .Include(v => v.Customer)
                .Include(v => v.ServiceOrders)
                .FirstOrDefaultAsync(v => v.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting vehicle by ID: {Id}", id);
            throw;
        }
    }

    public async Task<Vehicle?> GetVehicleByVinAsync(string vin)
    {
        try
        {
            return await _context.Vehicles
                .Include(v => v.Customer)
                .FirstOrDefaultAsync(v => v.VIN == vin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting vehicle by VIN: {VIN}", vin);
            throw;
        }
    }

    public async Task<Vehicle> CreateVehicleAsync(Vehicle vehicle)
    {
        try
        {
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating vehicle");
            throw;
        }
    }

    public async Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle)
    {
        try
        {
            _context.Entry(vehicle).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return vehicle;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating vehicle with ID: {Id}", vehicle.Id);
            throw;
        }
    }

    public async Task DeleteVehicleAsync(int id)
    {
        try
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting vehicle with ID: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ServiceOrder>> GetVehicleServiceHistoryAsync(int vehicleId)
    {
        try
        {
            return await _context.ServiceOrders
                .Include(so => so.Tasks)
                .Include(so => so.Comments)
                .Where(so => so.VehicleId == vehicleId)
                .OrderByDescending(so => so.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting service history for vehicle ID: {Id}", vehicleId);
            throw;
        }
    }

    public async Task<string> UploadVehiclePhotoAsync(int vehicleId, Stream photoStream, string fileName)
    {
        try
        {
            // In a real application, implement file upload logic here
            // For now, return a dummy URL
            return $"https://storage.example.com/vehicles/{vehicleId}/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while uploading photo for vehicle ID: {Id}", vehicleId);
            throw;
        }
    }
} 