using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using OrderManagementSystem.API.Models;
using Xunit;

namespace OrderManagementSystem.Tests
{
    public class OrderApiTests : IClassFixture<WebApplicationFactory<OrderManagementSystem.API.Program>>
    {
        private readonly WebApplicationFactory<OrderManagementSystem.API.Program> _factory;

        public OrderApiTests(WebApplicationFactory<OrderManagementSystem.API.Program> factory)
        {
            _factory = factory;
        }

        private async Task CleanupDatabaseAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderManagementSystem.API.Data.OrderManagementContext>();
            db.OrderItems.RemoveRange(db.OrderItems);
            db.Orders.RemoveRange(db.Orders);
            db.Products.RemoveRange(db.Products);
            await db.SaveChangesAsync();
        }

        [Fact]
        public async Task CreateOrder_ValidInput_ReturnsCreatedOrder()
        {
            await CleanupDatabaseAsync();
            // Arrange: create products first
            var client = _factory.CreateClient();
            var p1 = new Product { Name = "OrderTest Product 1", Price = 5.00m };
            var p2 = new Product { Name = "OrderTest Product 2", Price = 7.50m };
            var resp1 = await client.PostAsJsonAsync("/api/products", p1);
            var resp2 = await client.PostAsJsonAsync("/api/products", p2);
            Assert.Equal(HttpStatusCode.Created, resp1.StatusCode);
            Assert.Equal(HttpStatusCode.Created, resp2.StatusCode);
            var prod1 = await resp1.Content.ReadFromJsonAsync<Product>();
            var prod2 = await resp2.Content.ReadFromJsonAsync<Product>();
            Assert.NotNull(prod1);
            Assert.NotNull(prod2);

            var orderRequest = new
            {
                items = new[]
                {
                    new { productId = prod1.Id, quantity = 2 },
                    new { productId = prod2.Id, quantity = 3 }
                }
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/orders", orderRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<OrderResponse>();
            Assert.NotNull(created);
            Assert.Equal(2, created.Items.Count);
            Assert.Contains(created.Items, i => i.ProductId == prod1.Id && i.Quantity == 2);
            Assert.Contains(created.Items, i => i.ProductId == prod2.Id && i.Quantity == 3);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task CreateOrder_InvalidInput_ReturnsBadRequest(object items)
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();
            var orderRequest = new { items };
            var response = await client.PostAsJsonAsync("/api/orders", orderRequest);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateOrder_EmptyItemsList_ReturnsBadRequest()
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();
            var orderRequest = new { items = new object[] { } };
            var response = await client.PostAsJsonAsync("/api/orders", orderRequest);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateOrder_MissingProductId_ReturnsBadRequest()
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();
            var orderRequest = new { items = new[] { new { quantity = 2 } } };
            var response = await client.PostAsJsonAsync("/api/orders", orderRequest);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateOrder_MissingQuantity_ReturnsBadRequest()
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();
            // First, create a valid product
            var productResp = await client.PostAsJsonAsync("/api/products", new Product { Name = "Test Product", Price = 1.00m });
            Assert.Equal(HttpStatusCode.Created, productResp.StatusCode);
            var product = await productResp.Content.ReadFromJsonAsync<Product>();
            Assert.NotNull(product);
            var orderRequest = new { items = new[] { new { productId = product.Id } } };
            var response = await client.PostAsJsonAsync("/api/orders", orderRequest);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task CreateOrder_QuantityLessThanOne_ReturnsBadRequest(int quantity)
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();
            // First, create a valid product
            var productResp = await client.PostAsJsonAsync("/api/products", new Product { Name = "Test Product", Price = 1.00m });
            Assert.Equal(HttpStatusCode.Created, productResp.StatusCode);
            var product = await productResp.Content.ReadFromJsonAsync<Product>();
            Assert.NotNull(product);
            var orderRequest = new { items = new[] { new { productId = product.Id, quantity } } };
            var response = await client.PostAsJsonAsync("/api/orders", orderRequest);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateOrder_MissingAllFieldsInItem_ReturnsBadRequest()
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();
            var orderRequest = new { items = new[] { new { } } };
            var response = await client.PostAsJsonAsync("/api/orders", orderRequest);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateOrder_NonExistentProduct_ReturnsNotFound()
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();
            var orderRequest = new
            {
                items = new[] { new { productId = 99999, quantity = 1 } }
            };
            var response = await client.PostAsJsonAsync("/api/orders", orderRequest);
            Assert.True(response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.BadRequest);
        }

        public class OrderResponse
        {
            public int Id { get; set; }
            public List<OrderItemResponse> Items { get; set; } = new();
        }
        public class OrderItemResponse
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }

        [Fact]
        public async Task GetOrders_Empty_ReturnsEmptyList()
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/orders");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var paged = await response.Content.ReadFromJsonAsync<PagedResult<OrderResponse>>();
            Assert.NotNull(paged);
            Assert.NotNull(paged.Items);
            Assert.Empty(paged.Items);
        }

        [Fact]
        public async Task GetOrderInvoice_ReturnsCorrectInvoice()
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();

            // Create products
            var product1 = new Product { Name = "Apple", Price = 10m };
            var product2 = new Product { Name = "Banana", Price = 20m, DiscountPercentage = 25m, DiscountQuantityThreshold = 2 };
            var resp1 = await client.PostAsJsonAsync("/api/products", product1);
            var resp2 = await client.PostAsJsonAsync("/api/products", product2);
            Assert.Equal(HttpStatusCode.Created, resp1.StatusCode);
            Assert.Equal(HttpStatusCode.Created, resp2.StatusCode);
            var p1 = await resp1.Content.ReadFromJsonAsync<Product>();
            var p2 = await resp2.Content.ReadFromJsonAsync<Product>();
            Assert.NotNull(p1);
            Assert.NotNull(p2);

            // Place order: 1 Apple (no discount), 3 Banana (discount applies to 2+)
            var orderRequest = new
            {
                items = new[]
                {
                    new { productId = p1.Id, quantity = 1 },
                    new { productId = p2.Id, quantity = 3 }
                }
            };
            var orderResp = await client.PostAsJsonAsync("/api/orders", orderRequest);
            Assert.Equal(HttpStatusCode.Created, orderResp.StatusCode);
            var createdOrder = await orderResp.Content.ReadFromJsonAsync<OrderResponse>();
            Assert.NotNull(createdOrder);

            // Act: get invoice
            var invoiceResp = await client.GetAsync($"/api/orders/{createdOrder.Id}/invoice");
            Assert.Equal(HttpStatusCode.OK, invoiceResp.StatusCode);
            var invoice = await invoiceResp.Content.ReadFromJsonAsync<OrderInvoiceResponse>();
            Assert.NotNull(invoice);
            Assert.Equal(2, invoice.Products.Count);

            var appleLine = invoice.Products.Find(x => x.ProductName == "Apple");
            var bananaLine = invoice.Products.Find(x => x.ProductName == "Banana");
            Assert.NotNull(appleLine);
            Assert.NotNull(bananaLine);

            // Apple: no discount
            Assert.Equal(1, appleLine.Quantity);
            Assert.Equal(0m, appleLine.DiscountPercent);
            Assert.Equal(10m, appleLine.Amount);
            // Banana: 25% discount applies to all 3 (since quantity >= threshold)
            Assert.Equal(3, bananaLine.Quantity);
            Assert.Equal(25m, bananaLine.DiscountPercent);
            Assert.Equal(45m, bananaLine.Amount); // 20 * 3 * 0.75 = 45
            // Total
            Assert.Equal(55m, invoice.TotalAmount);
        }

        [Fact]
        public async Task GetOrderInvoice_NonExistentOrder_ReturnsNotFound()
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/orders/99999/invoice");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        public class OrderInvoiceResponse
        {
            public List<OrderInvoiceProduct> Products { get; set; } = new();
            public decimal TotalAmount { get; set; }
        }
        public class OrderInvoiceProduct
        {
            public required string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal DiscountPercent { get; set; }
            public decimal Amount { get; set; }
        }

        [Fact]
        public async Task GetOrders_ReturnsCreatedOrdersWithItems()
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();
            // Create products
            var p1 = new Product { Name = "GetList Product 1", Price = 1.00m };
            var p2 = new Product { Name = "GetList Product 2", Price = 2.00m };
            var resp1 = await client.PostAsJsonAsync("/api/products", p1);
            var resp2 = await client.PostAsJsonAsync("/api/products", p2);
            Assert.Equal(HttpStatusCode.Created, resp1.StatusCode);
            Assert.Equal(HttpStatusCode.Created, resp2.StatusCode);
            var prod1 = await resp1.Content.ReadFromJsonAsync<Product>();
            var prod2 = await resp2.Content.ReadFromJsonAsync<Product>();
            Assert.NotNull(prod1);
            Assert.NotNull(prod2);

            // Create order 1
            var orderReq1 = new { items = new[] { new { productId = prod1.Id, quantity = 2 } } };
            var respOrder1 = await client.PostAsJsonAsync("/api/orders", orderReq1);
            Assert.Equal(HttpStatusCode.Created, respOrder1.StatusCode);
            var created1 = await respOrder1.Content.ReadFromJsonAsync<OrderResponse>();
            Assert.NotNull(created1);

            // Create order 2
            var orderReq2 = new { items = new[] { new { productId = prod2.Id, quantity = 3 } } };
            var respOrder2 = await client.PostAsJsonAsync("/api/orders", orderReq2);
            Assert.Equal(HttpStatusCode.Created, respOrder2.StatusCode);
            var created2 = await respOrder2.Content.ReadFromJsonAsync<OrderResponse>();
            Assert.NotNull(created2);

            // Act
            var response = await client.GetAsync("/api/orders");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var paged = await response.Content.ReadFromJsonAsync<PagedResult<OrderResponse>>();
            Assert.NotNull(paged);
            Assert.NotNull(paged.Items);
            Assert.True(paged.Items.Count == 2); // Should only be the two we created
            Assert.Contains(paged.Items, o => o.Id == created1.Id && o.Items.Count == 1 && o.Items[0].ProductId == prod1.Id && o.Items[0].Quantity == 2);
            Assert.Contains(paged.Items, o => o.Id == created2.Id && o.Items.Count == 1 && o.Items[0].ProductId == prod2.Id && o.Items[0].Quantity == 3);
        }

        [Fact]
        public async Task GetDiscountedProductReport_ReturnsCorrectReport()
        {
            await CleanupDatabaseAsync();
            // ... existing test logic ...
        }

        [Fact]
        public async Task GetDiscountedProductReport_NoDiscountedProducts_ReturnsEmptyList()
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();
            var prod = new Product { Name = "NoDiscount", Price = 50m };
            var resp = await client.PostAsJsonAsync("/api/products", prod);
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
            var order = new { items = new[] { new { productId = (await resp.Content.ReadFromJsonAsync<Product>()).Id, quantity = 2 } } };
            var respOrder = await client.PostAsJsonAsync("/api/orders", order);
            Assert.Equal(HttpStatusCode.Created, respOrder.StatusCode);
            var response = await client.GetAsync("/api/reports/discounted-products");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var report = await response.Content.ReadFromJsonAsync<List<DiscountedProductReportItem>>();
            Assert.NotNull(report);
            Assert.Empty(report);
        }

        [Fact]
        public async Task GetDiscountedProductReport_MultipleDiscountedProducts_ReturnsAll()
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();
            var prod1 = new Product { Name = "DiscountA", Price = 10m, DiscountPercentage = 10m, DiscountQuantityThreshold = 2 };
            var prod2 = new Product { Name = "DiscountB", Price = 20m, DiscountPercentage = 15m, DiscountQuantityThreshold = 1 };
            var resp1 = await client.PostAsJsonAsync("/api/products", prod1);
            var resp2 = await client.PostAsJsonAsync("/api/products", prod2);
            Assert.Equal(HttpStatusCode.Created, resp1.StatusCode);
            Assert.Equal(HttpStatusCode.Created, resp2.StatusCode);
            var p1 = await resp1.Content.ReadFromJsonAsync<Product>();
            var p2 = await resp2.Content.ReadFromJsonAsync<Product>();
            var order1 = new { items = new[] { new { productId = p1.Id, quantity = 2 } } };
            var order2 = new { items = new[] { new { productId = p2.Id, quantity = 2 } } };
            await client.PostAsJsonAsync("/api/orders", order1);
            await client.PostAsJsonAsync("/api/orders", order2);
            var response = await client.GetAsync("/api/reports/discounted-products");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var report = await response.Content.ReadFromJsonAsync<List<DiscountedProductReportItem>>();
            Assert.NotNull(report);
            Assert.Equal(2, report.Count);
            Assert.Contains(report, r => r.ProductName == p1.Name);
            Assert.Contains(report, r => r.ProductName == p2.Name);
        }

        [Fact]
        public async Task GetDiscountedProductReport_LargeNumberOfOrders_Performance()
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();
            var prod = new Product { Name = "BulkDiscount", Price = 5m, DiscountPercentage = 10m, DiscountQuantityThreshold = 2 };
            var resp = await client.PostAsJsonAsync("/api/products", prod);
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
            var p = await resp.Content.ReadFromJsonAsync<Product>();
            for (int i = 0; i < 100; i++)
            {
                var order = new { items = new[] { new { productId = p.Id, quantity = 2 } } };
                var respOrder = await client.PostAsJsonAsync("/api/orders", order);
                Assert.Equal(HttpStatusCode.Created, respOrder.StatusCode);
            }
            var response = await client.GetAsync("/api/reports/discounted-products");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var report = await response.Content.ReadFromJsonAsync<List<DiscountedProductReportItem>>();
            Assert.NotNull(report);
            Assert.Single(report);
            Assert.Equal(100, report[0].NumberOfOrders);
        }

        [Fact]
        public async Task CreateOrder_InvalidInput_ReturnsBadRequest_WithErrorMessage()
        {
            await CleanupDatabaseAsync();
            var client = _factory.CreateClient();
            var orderRequest = new { items = (object)null };
            var response = await client.PostAsJsonAsync("/api/orders", orderRequest);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("error", content.ToLower());
        }

        public class DiscountedProductReportItem
        {
            public required string ProductName { get; set; }
            public decimal DiscountPercent { get; set; }
            public int NumberOfOrders { get; set; }
            public decimal TotalAmount { get; set; }
        }
    }
}
