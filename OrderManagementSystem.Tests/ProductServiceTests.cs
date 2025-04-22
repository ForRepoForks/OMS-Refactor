using System.Threading.Tasks;
using Xunit;
using System.ComponentModel.DataAnnotations;
using OrderManagementSystem.API.Models;
using OrderManagementSystem.API.Services;

namespace OrderManagementSystem.Tests
{
    public class ProductServiceTests
    {
        [Fact]
        public async Task ApplyDiscount_InvalidPercentage_ThrowsValidationException()
        {
            // Arrange
            var service = new ProductService(null); // null context for now, TDD-first
            var product = new Product { Name = "Test", Price = 10m };
            var discount = new ProductService.DiscountDto { Percentage = -5, QuantityThreshold = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => service.ApplyDiscountAsync(product, discount));
        }

        [Fact]
        public async Task ApplyDiscount_NegativeQuantityThreshold_ThrowsValidationException()
        {
            var service = new ProductService(null);
            var product = new Product { Name = "Test", Price = 10m };
            var discount = new ProductService.DiscountDto { Percentage = 10, QuantityThreshold = -1 };
            await Assert.ThrowsAsync<ValidationException>(() => service.ApplyDiscountAsync(product, discount));
        }

        [Fact]
        public async Task ApplyDiscount_BothZero_RemovesDiscount()
        {
            var service = new ProductService(null);
            var product = new Product { Name = "Test", Price = 10m, DiscountPercentage = 5, DiscountQuantityThreshold = 10 };
            var discount = new ProductService.DiscountDto { Percentage = 0, QuantityThreshold = 0 };
            await service.ApplyDiscountAsync(product, discount);
            Assert.Null(product.DiscountPercentage);
            Assert.Null(product.DiscountQuantityThreshold);
        }

        [Fact]
        public async Task ApplyDiscount_PercentagePositive_QuantityThresholdLessThanOne_ThrowsValidationException()
        {
            var service = new ProductService(null);
            var product = new Product { Name = "Test", Price = 10m };
            var discount = new ProductService.DiscountDto { Percentage = 10, QuantityThreshold = 0 };
            await Assert.ThrowsAsync<ValidationException>(() => service.ApplyDiscountAsync(product, discount));
        }

        [Fact]
        public async Task ApplyDiscount_ValidDiscount_UpdatesProductFields()
        {
            var service = new ProductService(null);
            var product = new Product { Name = "Test", Price = 10m };
            var discount = new ProductService.DiscountDto { Percentage = 15, QuantityThreshold = 10 };
            await service.ApplyDiscountAsync(product, discount);
            Assert.Equal(15, product.DiscountPercentage);
            Assert.Equal(10, product.DiscountQuantityThreshold);
        }

        [Fact]
        public async Task ApplyDiscount_NullProduct_ThrowsArgumentNullException()
        {
            var service = new ProductService(null);
            var discount = new ProductService.DiscountDto { Percentage = 10, QuantityThreshold = 10 };
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.ApplyDiscountAsync(null, discount));
        }
    }
}
