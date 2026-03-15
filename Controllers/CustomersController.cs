using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreHub.API.Data;
using StoreHub.API.Models;
using StoreHub.API.DTOs;

namespace StoreHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerResponseDto>>> GetCustomers()
    {
        var customers = await _context.Customers.ToListAsync();

        var response = customers.Select(c => new CustomerResponseDto(
            c.Id,
            c.FirstName,
            c.LastName,
            c.Email,
            $"{c.FirstName} {c.LastName}" // FullName oluşturuluyor
        ));

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerResponseDto>> GetCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
            return NotFound(new { message = "Müşteri bulunamadı." });

        var response = new CustomerResponseDto(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            $"{customer.FirstName} {customer.LastName}"
        );

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponseDto>> CreateCustomer(CustomerCreateDto dto)
    {
        var customer = new Customer
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var response = new CustomerResponseDto(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            $"{customer.FirstName} {customer.LastName}"
        );

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, CustomerUpdateDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new { message = "ID eşleşmiyor." });

        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return NotFound(new { message = "Müşteri bulunamadı." });

        customer.FirstName = dto.FirstName;
        customer.LastName = dto.LastName;
        customer.Email = dto.Email;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return NotFound();

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Müşteri silindi." });
    }
}
