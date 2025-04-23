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
        private readonly Services.IProductService _productService;
        private readonly AutoMapper.IMapper _mapper;

        public ProductsController(Services.IProductService productService, AutoMapper.IMapper mapper)
        {
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
            try
            {
                var result = await _productService.GetProductsAsync(name, page, pageSize);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/discount")]
        public async Task<ActionResult<DTOs.ProductResponseDto>> ApplyDiscount(int id, [FromBody] Services.ProductService.DiscountDto discount)
        {
            try
            {
                var responseDto = await _productService.ApplyDiscountAsync(id, discount);
                if (responseDto == null)
                    return NotFound();
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
