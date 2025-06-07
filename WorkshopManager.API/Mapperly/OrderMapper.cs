using Riok.Mapperly.Abstractions;
using WorkshopManager.API.DTOs;
using WorkshopManager.API.Models;

namespace WorkshopManager.API.Mapperly;

[Mapper]
public partial class OrderMapper
{
    public partial OrderDto Map(ServiceOrder serviceOrder);
    public partial ServiceOrder Map(OrderDto orderDto);
}