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
        public async Task CreateOrder_EmptyItems_ReturnsError()
        {
            await CleanupDatabaseAsync();
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<OrderManagementSystem.API.Services.IOrderService>();
            var (result, error) = await service.CreateOrderAsync(new List<OrderItemDto>());
            Assert.Null(result);
            Assert.Equal("At least one item is required.", error);
        }

        [Fact]
        public async Task CreateOrder_NullItem_ReturnsError()
        {
            await CleanupDatabaseAsync();
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<OrderManagementSystem.API.Services.IOrderService>();
            var items = new List<OrderItemDto> { null };
            var (result, error) = await service.CreateOrderAsync(items);
            Assert.Null(result);
            Assert.Equal("Order item cannot be null.", error);
        }

        [Fact]
        public async Task CreateOrder_ZeroQuantity_ReturnsError()
        {
            await CleanupDatabaseAsync();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderManagementContext>();
            var product = new Product { Name = "ZeroQuantityProduct", Price = 10m };
            db.Products.Add(product);
            await db.SaveChangesAsync();
            var service = scope.ServiceProvider.GetRequiredService<OrderManagementSystem.API.Services.IOrderService>();
            var items = new List<OrderItemDto> { new OrderItemDto { ProductId = product.Id, Quantity = 0 } };
            var (result, error) = await service.CreateOrderAsync(items);
            Assert.Null(result);
            Assert.Equal("Quantity must be at least 1.", error);
        }

        [Fact]
        public async Task CreateOrder_MissingProductId_ReturnsError()
        {
            await CleanupDatabaseAsync();
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<OrderManagementSystem.API.Services.IOrderService>();
            var items = new List<OrderItemDto> { new OrderItemDto { ProductId = 0, Quantity = 1 } };
            var (result, error) = await service.CreateOrderAsync(items);
            Assert.Null(result);
            Assert.Equal("ProductId is required and must be greater than 0.", error);
        }

        [Fact]
        public async Task CreateOrder_NonExistentProduct_ReturnsError()
        {
            await CleanupDatabaseAsync();
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<OrderManagementSystem.API.Services.IOrderService>();
            var items = new List<OrderItemDto> { new OrderItemDto { ProductId = 9999, Quantity = 1 } };
            var (result, error) = await service.CreateOrderAsync(items);
            Assert.Null(result);
            Assert.Equal("One or more products not found.", error);
        }
    }
}
