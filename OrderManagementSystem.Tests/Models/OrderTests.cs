using System.Collections.Generic;
using Xunit;
using OrderManagementSystem.API.Models;

namespace OrderManagementSystem.Tests.Models
{
    public class OrderTests
    {
        [Fact]
        public void Order_CanBeCreated_WithProductListAndQuantities()
        {
            // Arrange
            var product1 = new Product { Name = "Product 1", Price = 10m };
            var product2 = new Product { Name = "Product 2", Price = 20m };
            var items = new List<(Product product, int quantity)>
            {
                (product1, 2),
                (product2, 5)
            };

            // Act
            var order = new Order(items);

            // Assert
            Assert.Equal(2, order.Items.Count);
            Assert.Contains(order.Items, i => i.Product == product1 && i.Quantity == 2);
            Assert.Contains(order.Items, i => i.Product == product2 && i.Quantity == 5);
        }

        [Fact]
        public void Order_ThrowsException_IfProductListIsNullOrEmpty()
        {
            // Arrange, Act & Assert
            Assert.Throws<System.ArgumentException>(() => new Order(null));
            Assert.Throws<System.ArgumentException>(() => new Order(new List<(Product, int)>()));
        }

        [Fact]
        public void Order_CanBeCreated_WithDiscountThresholdEdgeCases()
        {
            var discounted = new Product { Name = "Discounted", Price = 100m, DiscountPercentage = 20m, DiscountQuantityThreshold = 2 };
            var normal = new Product { Name = "Normal", Price = 50m };

            // Below threshold
            var below = new List<(Product, int)> { (discounted, 1) };
            var orderBelow = new Order(below);
            Assert.Single(orderBelow.Items);
            Assert.Equal(1, orderBelow.Items[0].Quantity);

            // At threshold
            var at = new List<(Product, int)> { (discounted, 2) };
            var orderAt = new Order(at);
            Assert.Single(orderAt.Items);
            Assert.Equal(2, orderAt.Items[0].Quantity);

            // Above threshold
            var above = new List<(Product, int)> { (discounted, 3) };
            var orderAbove = new Order(above);
            Assert.Single(orderAbove.Items);
            Assert.Equal(3, orderAbove.Items[0].Quantity);
        }

        [Fact]
        public void Order_CanBeCreated_WithMixedDiscountedAndNonDiscountedProducts()
        {
            var discounted = new Product { Name = "Discounted", Price = 100m, DiscountPercentage = 10m, DiscountQuantityThreshold = 2 };
            var normal = new Product { Name = "Normal", Price = 50m };
            var items = new List<(Product, int)>
            {
                (discounted, 3),
                (normal, 5)
            };
            var order = new Order(items);
            Assert.Equal(2, order.Items.Count);
            Assert.Contains(order.Items, i => i.Product == discounted && i.Quantity == 3);
            Assert.Contains(order.Items, i => i.Product == normal && i.Quantity == 5);
        }
    }
}
