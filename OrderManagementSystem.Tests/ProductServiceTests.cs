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

        // --- Product Creation TDD ---
        [Fact]
        public async Task CreateProduct_NullName_ThrowsValidationException()
        {
            var service = new ProductService(null);
            var dto = new ProductCreateDto { Name = null, Price = 10m };
            await Assert.ThrowsAsync<ValidationException>(() => service.CreateProductAsync(dto));
        }

        [Fact]
        public async Task CreateProduct_EmptyName_ThrowsValidationException()
        {
            var service = new ProductService(null);
            var dto = new ProductCreateDto { Name = "", Price = 10m };
            await Assert.ThrowsAsync<ValidationException>(() => service.CreateProductAsync(dto));
        }

        [Fact]
        public async Task CreateProduct_WhitespaceName_ThrowsValidationException()
        {
            var service = new ProductService(null);
            var dto = new ProductCreateDto { Name = "   ", Price = 10m };
            await Assert.ThrowsAsync<ValidationException>(() => service.CreateProductAsync(dto));
        }

        [Fact]
        public async Task CreateProduct_NonPositivePrice_ThrowsValidationException()
        {
            var service = new ProductService(null);
            var dto = new ProductCreateDto { Name = "Valid Name", Price = 0m };
            await Assert.ThrowsAsync<ValidationException>(() => service.CreateProductAsync(dto));
        }

        [Fact]
        public async Task CreateProduct_ValidProduct_Succeeds()
        {
            var service = new ProductService(null);
            var dto = new ProductCreateDto { Name = "Valid Name", Price = 100m };
            var result = await service.CreateProductAsync(dto);
            Assert.NotNull(result);
            Assert.Equal("Valid Name", result.Name);
            Assert.Equal(100m, result.Price);
        }

        [Fact]
        public async Task CreateProduct_NegativePrice_ThrowsValidationException()
        {
            var service = new ProductService(null);
            var dto = new ProductCreateDto { Name = "Valid Name", Price = -10m };
            await Assert.ThrowsAsync<ValidationException>(() => service.CreateProductAsync(dto));
        }

        [Fact]
        public async Task CreateProduct_MaxNameLength_Succeeds()
        {
            var service = new ProductService(null);
            var longName = new string('A', 256); // Adjust if your max is different
            var dto = new ProductCreateDto { Name = longName, Price = 10m };
            var result = await service.CreateProductAsync(dto);
            Assert.NotNull(result);
            Assert.Equal(longName, result.Name);
        }

        [Fact]
        public async Task CreateProduct_ExceedingNameLength_ThrowsValidationException()
        {
            var service = new ProductService(null);
            var tooLongName = new string('B', 257); // Adjust if your max is different
            var dto = new ProductCreateDto { Name = tooLongName, Price = 10m };
            await Assert.ThrowsAsync<ValidationException>(() => service.CreateProductAsync(dto));
        }

        // This test assumes you want to trim whitespace; adjust as needed
        [Fact]
        public async Task CreateProduct_NameWithLeadingTrailingWhitespace_TrimmedOrRejected()
        {
            var service = new ProductService(null);
            var dto = new ProductCreateDto { Name = "  TrimMe  ", Price = 10m };
            var result = await service.CreateProductAsync(dto);
            Assert.NotNull(result);
            Assert.Equal("TrimMe", result.Name); // Or adjust if you want to reject instead
        }

        [Fact]
        public async Task CreateProduct_UnicodeName_Succeeds()
        {
            var service = new ProductService(null);
            var dto = new ProductCreateDto { Name = "商品名-测试", Price = 10m };
            var result = await service.CreateProductAsync(dto);
            Assert.NotNull(result);
            Assert.Equal("商品名-测试", result.Name);
        }

        // TODO: Enable this duplicate name test when the following are implemented:
        // 1. Enforce unique product names in the database (add a unique index via EF Core migration)
        //    and/or add a check in ProductService.CreateProductAsync to throw a ValidationException
        //    if a product with the same name already exists.
        // 2. Use a real or EF Core in-memory OrderManagementContext for this test, so that products
        //    are persisted and uniqueness can be checked across calls.
        // 3. After adding the unique index, generate and apply the migration to update the schema.
        // 4. TDD steps: (a) Write/enable this failing test, (b) implement uniqueness check, (c) make test pass.
        //
        // [Fact]
        // public async Task CreateProduct_DuplicateName_ThrowsValidationException()
        // {
        //     // Use a real or in-memory context here
        //     var service = new ProductService(/* real or mock context */);
        //     var product1 = new Product { Name = "Duplicate", Price = 10m };
        //     var product2 = new Product { Name = "Duplicate", Price = 20m };
        //     await service.CreateProductAsync(product1);
        //     await Assert.ThrowsAsync<ValidationException>(() => service.CreateProductAsync(product2));
        // }

    }
}
