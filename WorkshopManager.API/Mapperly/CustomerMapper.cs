using Riok.Mapperly.Abstractions;
using WorkshopManager.API.DTOs;
using WorkshopManager.API.Models;

namespace WorkshopManager.API.Mapperly;

[Mapper]
public partial class CustomerMapper
{
    public partial CustomerDto Map(Customer customer);
    public partial Customer Map(CustomerDto customerDto);
}