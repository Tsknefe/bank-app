using BankApi.Application.Auth.Dtos;
using BankApi.Domain.Entities;
using BankApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly BankaDbContext _context;

    public CustomersController(BankaDbContext context)
    {
        _context = context;
    }



    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName))
            return BadRequest("FirstName ve LastName zorunludur.");

        if (string.IsNullOrWhiteSpace(req.IdentityNumber))
            return BadRequest("IdentityNumber zorunludur.");

        var identity = req.IdentityNumber.Trim();

        var identityExists = await _context.Customers
            .AnyAsync(x => x.IdentityNumber == identity, ct);

        if (identityExists)
            return Conflict("Bu IdentityNumber zaten kayıtlı.");

        var customer = new Customer
        {
            FirstName = req.FirstName.Trim(),
            LastName = req.LastName.Trim(),
            IdentityNumber = identity,
            DateOfBirth = req.DateOfBirth
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var customer = await _context.Customers
            .Include(x => x.Accounts)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (customer is null) return NotFound();
        return Ok(customer);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var q = _context.Customers.AsQueryable();
        if (!includeInactive)
        {
            q = q.Where(x => x.IsActive);
        }
        var customers = await q.OrderByDescending(x => x.Id).ToListAsync(ct);

        return Ok(customers);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCustomerRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName))
            return BadRequest("FirstName ve LastName zorunludur.");

        var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (customer is null) return NotFound();

        customer.FirstName = req.FirstName.Trim();
        customer.LastName = req.LastName.Trim();
        customer.DateOfBirth = req.DateOfBirth;

        await _context.SaveChangesAsync(ct);
        return Ok(customer);
    }
    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate([FromRoute] Guid id, CancellationToken ct)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (customer is null) return NotFound();

        customer.IsActive = false;
        await _context.SaveChangesAsync(ct);

        return Ok(new { customer.Id, customer.IsActive, message = "Customer Deactivated" });
    }
    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate([FromRoute] Guid id, CancellationToken ct)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (customer is null) return NotFound();

        customer.IsActive = true;
        await _context.SaveChangesAsync(ct);
        return Ok(new { customer.Id, customer.IsActive, message = "Customer Activated" });

    }
}
