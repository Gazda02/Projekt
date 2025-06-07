using Microsoft.AspNetCore.Mvc;
using WorkshopManager.API.Mapperly;
using WorkshopManager.API.Models;
using WorkshopManager.API.Services.Interfaces;

namespace WorkshopManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;
    private readonly CustomerMapper _mapper;

    public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger, CustomerMapper mapper)
    {
        _customerService = customerService;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
    {
        try
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers.Select(c => _mapper.Map(c)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all customers");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetCustomer(int id)
    {
        try
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map(customer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting customer with ID: {Id}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
    {
        try
        {
            var createdCustomer = await _customerService.CreateCustomerAsync(customer);
            return CreatedAtAction(nameof(GetCustomer), new { id = createdCustomer.Id }, createdCustomer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating customer");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, Customer customer)
    {
        if (id != customer.Id)
        {
            return BadRequest();
        }

        try
        {
            await _customerService.UpdateCustomerAsync(customer);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating customer with ID: {Id}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        try
        {
            await _customerService.DeleteCustomerAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting customer with ID: {Id}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
} 