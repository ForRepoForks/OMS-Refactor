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
        private readonly Services.ProductService _productService;

        public ProductsController(OrderManagementContext context)
        {
            _context = context;
            _productService = new Services.ProductService(_context);
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
        public async Task<ActionResult<Product>> ApplyDiscount(int id, [FromBody] Services.ProductService.DiscountDto discount)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();
            try
            {
                await _productService.ApplyDiscountAsync(product, discount);
                await _context.SaveChangesAsync();
                return Ok(product);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                ModelState.AddModelError("Discount", ex.Message);
                return BadRequest(ModelState);
            }
            catch (System.ArgumentNullException ex)
            {
                ModelState.AddModelError("Discount", ex.Message);
                return BadRequest(ModelState);
            }
        }


    }
}
