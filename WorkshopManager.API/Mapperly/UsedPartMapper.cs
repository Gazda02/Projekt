using Riok.Mapperly.Abstractions;
using WorkshopManager.API.DTOs;
using WorkshopManager.API.Models;

namespace WorkshopManager.API.Mapperly;

[Mapper]
public partial class UsedPartMapper
{
    public partial UsedPart Map(UsedPartDto usedPartDto);
    public partial UsedPartDto Map(UsedPart usedPart);
}