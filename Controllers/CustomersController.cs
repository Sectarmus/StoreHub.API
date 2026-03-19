using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreHub.API.Data;
using StoreHub.API.Models;
using StoreHub.API.DTOs;
using AutoMapper;

namespace StoreHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper; // AutoMapper asistanımız

    public CustomersController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerResponseDto>>> GetCustomers()
    {
        var customers = await _context.Customers.ToListAsync();

        // Sihir başlıyor: Liste halinde Customers objelerini -> CustomerResponseDto listesine (IEnumerable) çeviriyor.
        var response = _mapper.Map<IEnumerable<CustomerResponseDto>>(customers);

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerResponseDto>> GetCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
            return NotFound(new { message = "Müşteri bulunamadı." });

        var response = _mapper.Map<CustomerResponseDto>(customer);

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponseDto>> CreateCustomer(CustomerCreateDto dto)
    {
        // DTO'dan (girdi) -> Model'e (veritabanı) doğru çeviri yapar (Boş formatı senin verdiğin kuralla doldurur)
        var customer = _mapper.Map<Customer>(dto);

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var response = _mapper.Map<CustomerResponseDto>(customer);

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

        // Dıştaki DTO'dan gelen GÜNCEL verileri, veritabanından bulduğumuz Customer Varlığının İÇİNE DÖKER!
        // Tek tek customer.FirstName = dto.FirstName ameleliğinden kurtulduk!
        _mapper.Map(dto, customer);

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
