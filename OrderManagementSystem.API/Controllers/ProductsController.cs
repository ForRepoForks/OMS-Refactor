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
        private readonly Services.IProductService _productService;
        private readonly AutoMapper.IMapper _mapper;

        public ProductsController(OrderManagementContext context, Services.IProductService productService, AutoMapper.IMapper mapper)
        {
            _context = context;
            _productService = productService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<DTOs.ProductResponseDto>> CreateProduct([FromBody] Services.ProductCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var product = await _productService.CreateProductAsync(dto);
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                var responseDto = _mapper.Map<DTOs.ProductResponseDto>(product);
                return CreatedAtAction(nameof(CreateProduct), new { id = product.Id }, responseDto);
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("Product", ex.Message);
                return BadRequest(ModelState);
            }
            catch (ArgumentNullException ex)
            {
                ModelState.AddModelError("Product", ex.Message);
                return BadRequest(ModelState);
            }
        }

        [HttpGet]
        public async Task<ActionResult<Models.PagedResult<DTOs.ProductResponseDto>>> GetProducts(
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
            var dtoList = _mapper.Map<List<DTOs.ProductResponseDto>>(products);
            var result = new Models.PagedResult<DTOs.ProductResponseDto>
            {
                Items = dtoList,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
            return Ok(result);
        }

        [HttpPut("{id}/discount")]
        public async Task<ActionResult<DTOs.ProductResponseDto>> ApplyDiscount(int id, [FromBody] Services.ProductService.DiscountDto discount)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();
            try
            {
                await _productService.ApplyDiscountAsync(product, discount);
                await _context.SaveChangesAsync();
                var responseDto = _mapper.Map<DTOs.ProductResponseDto>(product);
                return Ok(responseDto);
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
