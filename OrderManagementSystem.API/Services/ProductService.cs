using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrderManagementSystem.API.Models;

namespace OrderManagementSystem.API.Services
{
    public class ProductService : IProductService
    {
        private readonly OrderManagementSystem.API.Data.OrderManagementContext _context;
        public ProductService(OrderManagementSystem.API.Data.OrderManagementContext context)
        {
            _context = context;
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

        public Task<Product> CreateProductAsync(ProductCreateDto dto)
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
                return Task.FromResult(product);
            }
            catch (ArgumentException ex)
            {
                throw new ValidationException(ex.Message, ex);
            }
        }
    }
}
