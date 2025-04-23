using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrderManagementSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace OrderManagementSystem.API.Services
{
    public class ProductService : IProductService
    {
        private readonly AutoMapper.IMapper _mapper;

        private readonly OrderManagementSystem.API.Data.OrderManagementContext _context;
        public ProductService(OrderManagementSystem.API.Data.OrderManagementContext context, AutoMapper.IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public class DiscountDto
        {
            public decimal Percentage { get; set; }
            public int QuantityThreshold { get; set; }
        }

        public Task ApplyDiscountAsync(Product product, DiscountDto discount)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            if (discount.Percentage < 0 || discount.Percentage > 100)
                throw new System.ComponentModel.DataAnnotations.ValidationException("Discount percentage must be between 0 and 100.");
            if (discount.QuantityThreshold < 0)
                throw new System.ComponentModel.DataAnnotations.ValidationException("Quantity threshold cannot be negative.");
            if (discount.Percentage == 0 && discount.QuantityThreshold == 0)
            {
                product.DiscountPercentage = null;
                product.DiscountQuantityThreshold = null;
                return Task.CompletedTask;
            }
            if (discount.Percentage > 0 && discount.QuantityThreshold < 1)
                throw new System.ComponentModel.DataAnnotations.ValidationException("Quantity threshold must be greater than 0 unless removing discount.");
            product.DiscountPercentage = discount.Percentage;
            product.DiscountQuantityThreshold = discount.QuantityThreshold;
            return Task.CompletedTask;
        }

        public async Task<Product> CreateProductAsync(ProductCreateDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Name validation
            var name = dto.Name;
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Product name is required.");
            name = name.Trim();
            const int maxNameLength = 256; // Adjust if your model is different
            if (name.Length > maxNameLength)
                throw new ValidationException($"Product name must be at most {maxNameLength} characters.");

            // Price validation
            if (dto.Price <= 0)
                throw new ValidationException("Product price must be positive.");

            // Construct Product and catch model-level exceptions
            try
            {
                var product = new Product { Name = name, Price = dto.Price };
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return product;
            }
            catch (ArgumentException ex)
            {
                throw new ValidationException(ex.Message, ex);
            }
        }
        public async Task<OrderManagementSystem.API.DTOs.PagedResult<DTOs.ProductResponseDto>> GetProductsAsync(string? name, int page, int pageSize)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                throw new ValidationException("Invalid pagination parameters.");

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
            return new OrderManagementSystem.API.DTOs.PagedResult<DTOs.ProductResponseDto>
            {
                Items = dtoList,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<DTOs.ProductResponseDto?> ApplyDiscountAsync(int id, DiscountDto discount)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return null;
            await ApplyDiscountAsync(product, discount);
            await _context.SaveChangesAsync();
            return _mapper.Map<DTOs.ProductResponseDto>(product);
        }
    }
}
