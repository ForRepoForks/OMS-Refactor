using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrderManagementSystem.API.Models;

namespace OrderManagementSystem.API.Services
{
    public class ProductService
    {
        private readonly object _context;
        public ProductService(object context)
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
    }
}
