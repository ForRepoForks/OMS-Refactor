using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using OrderManagementSystem.API.Models;
using Xunit;

namespace OrderManagementSystem.Tests
{
    public class ProductApiTests : IClassFixture<WebApplicationFactory<OrderManagementSystem.API.Program>>
    {
        private async Task CleanupDatabaseAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderManagementSystem.API.Data.OrderManagementContext>();
            db.OrderItems.RemoveRange(db.OrderItems);
            db.Orders.RemoveRange(db.Orders);
            db.Products.RemoveRange(db.Products);
            await db.SaveChangesAsync();
        }

        [Theory]
        [InlineData(null, 10)]
        [InlineData("", 10)]
        [InlineData("Valid Name", 0)]
        [InlineData("Valid Name", -5)]
        public async Task CreateProduct_InvalidInput_ReturnsBadRequest(string name, decimal price)
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();
            var payload = new { Name = name, Price = price };
            var response = await client.PostAsJsonAsync("/api/products", payload);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(-1, 10)]
        [InlineData(101, 10)]
        [InlineData(10, 0)]
        [InlineData(10, -5)]
        public async Task ApplyDiscount_InvalidInput_ReturnsBadRequest(decimal percentage, int quantityThreshold)
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();
            var newProduct = new Product { Name = "Valid Product", Price = 100m };
            var createResponse = await client.PostAsJsonAsync("/api/products", newProduct);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<Product>();
            Assert.NotNull(created);

            var discount = new { Percentage = percentage, QuantityThreshold = quantityThreshold };
            var discountResponse = await client.PutAsJsonAsync($"/api/products/{created.Id}/discount", discount);
            Assert.Equal(HttpStatusCode.BadRequest, discountResponse.StatusCode);
        }

        private readonly WebApplicationFactory<OrderManagementSystem.API.Program> _factory;

        public ProductApiTests(WebApplicationFactory<OrderManagementSystem.API.Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreateProduct_ReturnsCreatedProduct()
        {
            await CleanupDatabaseAsync();
            // Arrange
            var client = _factory.CreateClient();
            var newProduct = new Product { Name = new string('A', 256), Price = 123.45m };

            // Act
            var response = await client.PostAsJsonAsync("/api/products", newProduct);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<Product>();
            Assert.NotNull(created);
            Assert.Equal(newProduct.Name, created.Name);
            Assert.Equal(newProduct.Price, created.Price);
            Assert.True(created.Id > 0);
        }

        [Fact]
        public async Task CreateProduct_WithMaxPrice_ReturnsCreatedProduct()
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();
            var newProduct = new Product { Name = "MaxPrice", Price = decimal.MaxValue };
            var response = await client.PostAsJsonAsync("/api/products", newProduct);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<Product>();
            Assert.NotNull(created);
            Assert.Equal(decimal.MaxValue, created.Price);
        }

        [Fact]
        public async Task ApplyDiscountToNonExistentProduct_ReturnsNotFound()
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();
            var discount = new { Percentage = 10, QuantityThreshold = 2 };
            var response = await client.PutAsJsonAsync($"/api/products/99999/discount", discount);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task RemoveDiscountFromProduct_SetsDiscountToNull()
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();
            var newProduct = new Product { Name = "Discounted Product", Price = 100m };
            var createResponse = await client.PostAsJsonAsync("/api/products", newProduct);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<Product>();
            Assert.NotNull(created);

            var discount = new { Percentage = 15, QuantityThreshold = 10 };
            var discountResponse = await client.PutAsJsonAsync($"/api/products/{created.Id}/discount", discount);
            discountResponse.EnsureSuccessStatusCode();
            var updated = await discountResponse.Content.ReadFromJsonAsync<Product>();
            Assert.NotNull(updated);
            Assert.Equal(15, updated.DiscountPercentage);
            Assert.Equal(10, updated.DiscountQuantityThreshold);

            // Remove discount
            var removeDiscount = new { Percentage = 0, QuantityThreshold = 0 };
            var removeResponse = await client.PutAsJsonAsync($"/api/products/{created.Id}/discount", removeDiscount);
            removeResponse.EnsureSuccessStatusCode();
            var removed = await removeResponse.Content.ReadFromJsonAsync<Product>();
            Assert.NotNull(removed);
            Assert.True(removed.DiscountPercentage == 0 || removed.DiscountPercentage == null);
            Assert.True(removed.DiscountQuantityThreshold == 0 || removed.DiscountQuantityThreshold == null);
        }

        [Fact]
        public async Task CreateProduct_ResponseSchema_IsValid()
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();
            var newProduct = new Product { Name = "SchemaTest", Price = 10m };
            var response = await client.PostAsJsonAsync("/api/products", newProduct);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"name\"", json.ToLower());
            Assert.Contains("\"price\"", json.ToLower());
            Assert.Contains("\"id\"", json.ToLower());
        }

        [Fact]
        public async Task GetProducts_ReturnsListIncludingCreatedProduct()
        {
            await CleanupDatabaseAsync();
            // Arrange
            var client = _factory.CreateClient();
            var newProduct = new Product { Name = "List Test Product", Price = 55.55m };
            var createResponse = await client.PostAsJsonAsync("/api/products", newProduct);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<Product>();

            // Act
            var response = await client.GetAsync("/api/products");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var paged = await response.Content.ReadFromJsonAsync<PagedResult<Product>>();
            Assert.NotNull(paged);
            Assert.NotNull(paged.Items);
            Assert.Contains(paged.Items, p => p.Id == created.Id && p.Name == newProduct.Name && p.Price == newProduct.Price);
        }

        [Fact]
        public async Task GetProducts_SearchByName_ReturnsMatchingProducts()
        {
            await CleanupDatabaseAsync();
            // Arrange
            var client = _factory.CreateClient();
            var productsToCreate = new[]
            {
                new Product { Name = "Apple", Price = 1.00m },
                new Product { Name = "Banana", Price = 2.00m },
                new Product { Name = "Green Apple", Price = 1.50m },
                new Product { Name = "Pineapple", Price = 3.00m }
            };
            foreach (var p in productsToCreate)
            {
                var resp = await client.PostAsJsonAsync("/api/products", p);
                Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
                var created = await resp.Content.ReadFromJsonAsync<Product>();
                Assert.NotNull(created);
            }

            // Act
            var response = await client.GetAsync("/api/products?name=Apple");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var paged = await response.Content.ReadFromJsonAsync<PagedResult<Product>>();
            Assert.NotNull(paged);
            Assert.NotNull(paged.Items);
            Assert.All(paged.Items, p => Assert.True(p.Name.IndexOf("Apple", System.StringComparison.OrdinalIgnoreCase) >= 0));
            Assert.Contains(paged.Items, p => p.Name == "Apple");
            Assert.Contains(paged.Items, p => p.Name == "Green Apple");
            Assert.Contains(paged.Items, p => p.Name == "Pineapple");
            Assert.DoesNotContain(paged.Items, p => p.Name == "Banana");
        }

        [Fact]
        public async Task ApplyDiscountToProduct_StoresDiscountCorrectly()
        {
            await CleanupDatabaseAsync();
            // Arrange
            var client = _factory.CreateClient();
            var newProduct = new Product { Name = "Discounted Product", Price = 100m };
            var createResponse = await client.PostAsJsonAsync("/api/products", newProduct);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<Product>();
            Assert.NotNull(created);

            var discount = new { Percentage = 15, QuantityThreshold = 10 };

            // Act
            var discountResponse = await client.PutAsJsonAsync($"/api/products/{created.Id}/discount", discount);
            discountResponse.EnsureSuccessStatusCode();
            var updated = await discountResponse.Content.ReadFromJsonAsync<Product>();

            // Assert
            Assert.NotNull(updated);
            Assert.Equal(created.Id, updated.Id);
            Assert.Equal(discount.Percentage, updated.DiscountPercentage);
            Assert.Equal(discount.QuantityThreshold, updated.DiscountQuantityThreshold);
        }
    }
}
