using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.API.Data;
using OrderManagementSystem.API.Models;
using System.ComponentModel.DataAnnotations;

namespace OrderManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly OrderManagementContext _context;

        public ProductsController(OrderManagementContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(product.Name))
            {
                return BadRequest(ModelState);
            }
            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("Price", ex.Message);
                return BadRequest(ModelState);
            }
            return CreatedAtAction(nameof(CreateProduct), new { id = product.Id }, product);
        }

        [HttpGet]
public async Task<ActionResult<PagedResult<Product>>> GetProducts(
    [FromQuery] string? name,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
{
    if (page < 1 || pageSize < 1 || pageSize > 100)
        return BadRequest("Invalid pagination parameters.");

    var query = _context.Products.AsQueryable();
    if (!string.IsNullOrWhiteSpace(name))
    {
        query = query.Where(p => p.Name.ToLower().Contains(name.ToLower()));
    }
    var totalCount = await query.CountAsync();
    var products = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    var result = new PagedResult<Product>
    {
        Items = products,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    };
    return Ok(result);
}

        [HttpPut("{id}/discount")]
        public async Task<ActionResult<Product>> ApplyDiscount(int id, [FromBody] DiscountDto discount)
        {
            // Allow both fields to be zero to indicate removal
            if ((discount.Percentage < 0 || discount.Percentage > 100) || (discount.QuantityThreshold < 0))
            {
                ModelState.AddModelError("Discount", "Discount percentage must be between 0 and 100 and quantity threshold cannot be negative.");
                return BadRequest(ModelState);
            }
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            if (discount.Percentage == 0 && discount.QuantityThreshold == 0)
            {
                product.DiscountPercentage = null;
                product.DiscountQuantityThreshold = null;
            }
            else
            {
                if (discount.QuantityThreshold < 1)
                {
                    ModelState.AddModelError("Discount", "Quantity threshold must be greater than 0 unless removing discount.");
                    return BadRequest(ModelState);
                }
                product.DiscountPercentage = discount.Percentage;
                product.DiscountQuantityThreshold = discount.QuantityThreshold;
            }
            await _context.SaveChangesAsync();
            return Ok(product);
        }

        public class DiscountDto
        {
            public decimal Percentage { get; set; }
            public int QuantityThreshold { get; set; }
        }
    }
}
