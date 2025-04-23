using System.Threading.Tasks;
using Xunit;
using System.ComponentModel.DataAnnotations;
using OrderManagementSystem.API.Models;
using OrderManagementSystem.API.Services;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.Tests.TestHelpers;

namespace OrderManagementSystem.Tests
{
    public class ProductServiceTests
    {
        private static AutoMapper.IMapper GetTestMapper()
        {
            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(OrderManagementSystem.API.Program));
            });
            return config.CreateMapper();
        }

        [Fact]
        public async Task ApplyDiscount_InvalidPercentage_ThrowsValidationException()
        {
            // Arrange
            var options = DbContextTestHelper.GetTestDbOptions();
            using var context = new OrderManagementSystem.API.Data.OrderManagementContext(options);
            var service = new ProductService(context, GetTestMapper()); // null context for now, TDD-first
            var product = new Product { Name = "Test", Price = 10m };
            var discount = new ProductService.DiscountDto { Percentage = -5, QuantityThreshold = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => service.ApplyDiscountAsync(product, discount));
        }

        [Fact]
        public async Task ApplyDiscount_NegativeQuantityThreshold_ThrowsValidationException()
        {
            var options = DbContextTestHelper.GetTestDbOptions();
            using var context = new OrderManagementSystem.API.Data.OrderManagementContext(options);
            var service = new ProductService(context, GetTestMapper());
            var product = new Product { Name = "Test", Price = 10m };
            var discount = new ProductService.DiscountDto { Percentage = 10, QuantityThreshold = -1 };
            await Assert.ThrowsAsync<ValidationException>(() => service.ApplyDiscountAsync(product, discount));
        }

        [Fact]
        public async Task ApplyDiscount_BothZero_RemovesDiscount()
        {
            var options = DbContextTestHelper.GetTestDbOptions();
            using var context = new OrderManagementSystem.API.Data.OrderManagementContext(options);
            var service = new ProductService(context, GetTestMapper());
            var product = new Product { Name = "Test", Price = 10m, DiscountPercentage = 5, DiscountQuantityThreshold = 10 };
            var discount = new ProductService.DiscountDto { Percentage = 0, QuantityThreshold = 0 };
            await service.ApplyDiscountAsync(product, discount);
            Assert.Null(product.DiscountPercentage);
            Assert.Null(product.DiscountQuantityThreshold);
        }

        [Fact]
        public async Task ApplyDiscount_PercentagePositive_QuantityThresholdLessThanOne_ThrowsValidationException()
        {
            var options = DbContextTestHelper.GetTestDbOptions();
            using var context = new OrderManagementSystem.API.Data.OrderManagementContext(options);
            var service = new ProductService(context, GetTestMapper());
            var product = new Product { Name = "Test", Price = 10m };
            var discount = new ProductService.DiscountDto { Percentage = 10, QuantityThreshold = 0 };
            await Assert.ThrowsAsync<ValidationException>(() => service.ApplyDiscountAsync(product, discount));
        }

        [Fact]
        public async Task ApplyDiscount_ValidDiscount_UpdatesProductFields()
        {
            var options = DbContextTestHelper.GetTestDbOptions();
            using var context = new OrderManagementSystem.API.Data.OrderManagementContext(options);
            var service = new ProductService(context, GetTestMapper());
            var product = new Product { Name = "Test", Price = 10m };
            var discount = new ProductService.DiscountDto { Percentage = 15, QuantityThreshold = 10 };
            await service.ApplyDiscountAsync(product, discount);
            Assert.Equal(15, product.DiscountPercentage);
            Assert.Equal(10, product.DiscountQuantityThreshold);
        }

        [Fact]
        public async Task ApplyDiscount_NullProduct_ThrowsArgumentNullException()
        {
            var options = DbContextTestHelper.GetTestDbOptions();
            using var context = new OrderManagementSystem.API.Data.OrderManagementContext(options);
            var service = new ProductService(context, GetTestMapper());
            var discount = new ProductService.DiscountDto { Percentage = 10, QuantityThreshold = 10 };
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.ApplyDiscountAsync(null, discount));
        }

        // --- Product Creation TDD ---
        [Fact]
        public async Task CreateProduct_NullName_ThrowsValidationException()
        {
            var options = DbContextTestHelper.GetTestDbOptions();
            using var context = new OrderManagementSystem.API.Data.OrderManagementContext(options);
            var service = new ProductService(context, GetTestMapper());
            var dto = new ProductCreateDto { Name = null, Price = 10m };
            await Assert.ThrowsAsync<ValidationException>(() => service.CreateProductAsync(dto));
        }

        [Fact]
        public async Task CreateProduct_EmptyName_ThrowsValidationException()
        {
            var options = DbContextTestHelper.GetTestDbOptions();
            using var context = new OrderManagementSystem.API.Data.OrderManagementContext(options);
            var service = new ProductService(context, GetTestMapper());
            var dto = new ProductCreateDto { Name = "", Price = 10m };
            await Assert.ThrowsAsync<ValidationException>(() => service.CreateProductAsync(dto));
        }

        [Fact]
        public async Task CreateProduct_WhitespaceName_ThrowsValidationException()
        {
            var options = DbContextTestHelper.GetTestDbOptions();
            using var context = new OrderManagementSystem.API.Data.OrderManagementContext(options);
            var service = new ProductService(context, GetTestMapper());
            var dto = new ProductCreateDto { Name = "   ", Price = 10m };
            await Assert.ThrowsAsync<ValidationException>(() => service.CreateProductAsync(dto));
        }

        [Fact]
        public async Task CreateProduct_NonPositivePrice_ThrowsValidationException()
        {
            var options = DbContextTestHelper.GetTestDbOptions();
            using var context = new OrderManagementSystem.API.Data.OrderManagementContext(options);
            var service = new ProductService(context, GetTestMapper());
            var dto = new ProductCreateDto { Name = "Valid Name", Price = 0m };
            await Assert.ThrowsAsync<ValidationException>(() => service.CreateProductAsync(dto));
        }


        [Fact]
        public async Task CreateProduct_ValidProduct_Succeeds()
        {
            // Arrange
            var options = DbContextTestHelper.GetTestDbOptions();
            using var context = new OrderManagementSystem.API.Data.OrderManagementContext(options);
            // Clean up before test
            context.Products.RemoveRange(context.Products);
            await context.SaveChangesAsync();
            var service = new ProductService(context, GetTestMapper());
            var dto = new ProductCreateDto { Name = "Valid Name", Price = 100m };
            // Act
            var result = await service.CreateProductAsync(dto);
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Valid Name", result.Name);
            Assert.Equal(100m, result.Price);
        }

        [Fact]
        public async Task CreateProduct_NegativePrice_ThrowsValidationException()
        {
            var options = DbContextTestHelper.GetTestDbOptions();
            using var context = new OrderManagementSystem.API.Data.OrderManagementContext(options);
            var service = new ProductService(context, GetTestMapper());
            var dto = new ProductCreateDto { Name = "Valid Name", Price = -10m };
            await Assert.ThrowsAsync<ValidationException>(() => service.CreateProductAsync(dto));
        }

        [Fact]
        public async Task CreateProduct_MaxNameLength_Succeeds()
        {
            // Arrange
            var options = DbContextTestHelper.GetTestDbOptions();
            using var context = new OrderManagementSystem.API.Data.OrderManagementContext(options);
            context.Products.RemoveRange(context.Products);
            await context.SaveChangesAsync();
            var service = new ProductService(context, GetTestMapper());
            var longName = new string('A', 256); // Adjust if your max is different
            var dto = new ProductCreateDto { Name = longName, Price = 10m };
            // Act
            var result = await service.CreateProductAsync(dto);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(longName, result.Name);
        }

        [Fact]
        public async Task CreateProduct_ExceedingNameLength_ThrowsValidationException()
        {
            var options = DbContextTestHelper.GetTestDbOptions();
            using var context = new OrderManagementSystem.API.Data.OrderManagementContext(options);
            var service = new ProductService(context, GetTestMapper());
            var tooLongName = new string('B', 257); // Adjust if your max is different
            var dto = new ProductCreateDto { Name = tooLongName, Price = 10m };
            await Assert.ThrowsAsync<ValidationException>(() => service.CreateProductAsync(dto));
        }

        // This test assumes you want to trim whitespace; adjust as needed
        [Fact]
        public async Task CreateProduct_NameWithLeadingTrailingWhitespace_TrimmedOrRejected()
        {
            // Arrange
            var options = DbContextTestHelper.GetTestDbOptions();
            using var context = new OrderManagementSystem.API.Data.OrderManagementContext(options);
            context.Products.RemoveRange(context.Products);
            await context.SaveChangesAsync();
            var service = new ProductService(context, GetTestMapper());
            var dto = new ProductCreateDto { Name = "  TrimMe  ", Price = 10m };
            // Act
            var result = await service.CreateProductAsync(dto);
            // Assert
            Assert.NotNull(result);
            Assert.Equal("TrimMe", result.Name); // Or adjust if you want to reject instead
        }

        [Fact]
        public async Task CreateProduct_UnicodeName_Succeeds()
        {
            // Arrange
            var options = DbContextTestHelper.GetTestDbOptions();
            using var context = new OrderManagementSystem.API.Data.OrderManagementContext(options);
            context.Products.RemoveRange(context.Products);
            await context.SaveChangesAsync();
            var service = new ProductService(context, GetTestMapper());
            var dto = new ProductCreateDto { Name = "商品名-测试", Price = 10m };
            // Act
            var result = await service.CreateProductAsync(dto);
            // Assert
            Assert.NotNull(result);
            Assert.Equal("商品名-测试", result.Name);
        }

        [Fact]
        public async Task CreateProduct_DuplicateName_ThrowsValidationException()
        {
            var options = DbContextTestHelper.GetTestDbOptions();
            using var context = new OrderManagementSystem.API.Data.OrderManagementContext(options);
            var service = new ProductService(context, GetTestMapper());
            var dto1 = new ProductCreateDto { Name = "Duplicate", Price = 10m };
            var dto2 = new ProductCreateDto { Name = "Duplicate", Price = 20m };
            await service.CreateProductAsync(dto1);
            await Assert.ThrowsAsync<ValidationException>(() => service.CreateProductAsync(dto2));
        }
    }
}
