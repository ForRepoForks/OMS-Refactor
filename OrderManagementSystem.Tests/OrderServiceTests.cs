using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using OrderManagementSystem.API.Data;
using OrderManagementSystem.API.DTOs;
using OrderManagementSystem.API.Models;
using Xunit;

namespace OrderManagementSystem.Tests
{
    public class OrderServiceTests : IClassFixture<WebApplicationFactory<OrderManagementSystem.API.Program>>
    {
        private readonly WebApplicationFactory<OrderManagementSystem.API.Program> _factory;

        public OrderServiceTests(WebApplicationFactory<OrderManagementSystem.API.Program> factory)
        {
            _factory = factory;
        }

        private async Task CleanupDatabaseAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderManagementContext>();
            db.OrderItems.RemoveRange(db.OrderItems);
            db.Orders.RemoveRange(db.Orders);
            db.Products.RemoveRange(db.Products);
            await db.SaveChangesAsync();
        }

        [Fact]
        public async Task CreateOrder_ValidInput_CreatesOrder()
        {
            await CleanupDatabaseAsync();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderManagementContext>();
            // Add products
            var product1 = new Product { Name = "OrderServiceTestProduct1", Price = 10m };
            var product2 = new Product { Name = "OrderServiceTestProduct2", Price = 20m };
            db.Products.AddRange(product1, product2);
            await db.SaveChangesAsync();
            // Prepare DTO
            var dto = new OrderCreateRequestDto
            {
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto { ProductId = product1.Id, Quantity = 2 },
                    new OrderItemDto { ProductId = product2.Id, Quantity = 3 }
                }
            };
            var service = scope.ServiceProvider.GetRequiredService<OrderManagementSystem.API.Services.IOrderService>();

            // Act
            var orderId = await service.CreateOrderAsync(dto);

            // Assert
            var createdOrder = db.Orders
                .Where(o => o.Id == orderId)
                .Select(o => new { o.Id, Items = o.Items.Select(i => new { i.ProductId, i.Quantity }).ToList() })
                .FirstOrDefault();
            Assert.NotNull(createdOrder);
            Assert.Equal(2, createdOrder.Items.Count);
            Assert.Contains(createdOrder.Items, i => i.ProductId == product1.Id && i.Quantity == 2);
            Assert.Contains(createdOrder.Items, i => i.ProductId == product2.Id && i.Quantity == 3);
        }
    }
}
