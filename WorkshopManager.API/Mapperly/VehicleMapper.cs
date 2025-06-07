using Riok.Mapperly.Abstractions;
using WorkshopManager.API.Models;
using WorkshopManager.API.DTOs;

namespace WorkshopManager.API.Mapperly;

[Mapper]
public partial class VehicleMapper
{
    public partial Vehicle Map(VehicleDto vehicleDto);
    public partial VehicleDto Map(Vehicle vehicle);
}