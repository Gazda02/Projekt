using Riok.Mapperly.Abstractions;
using WorkshopManager.API.DTOs;
using WorkshopManager.API.Models;

namespace WorkshopManager.API.Mapperly;

[Mapper]
public partial class PartMapper
{
    public partial PartDto Map(Part part);
    public partial Part Map(PartDto partDto);
}