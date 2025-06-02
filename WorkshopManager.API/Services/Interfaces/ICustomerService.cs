using WorkshopManager.API.Models;

namespace WorkshopManager.API.Services.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<Customer>> GetAllCustomersAsync();
    Task<Customer?> GetCustomerByIdAsync(int id);
    Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm);
    Task<Customer> CreateCustomerAsync(Customer customer);
    Task<Customer> UpdateCustomerAsync(Customer customer);
    Task DeleteCustomerAsync(int id);
    Task<IEnumerable<Vehicle>> GetCustomerVehiclesAsync(int customerId);
} 