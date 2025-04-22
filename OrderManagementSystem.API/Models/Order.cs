using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OrderManagementSystem.API.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public List<OrderItem> Items { get; set; } = new();

        public Order() { }

        public Order(IEnumerable<(Product product, int quantity)> items)
        {
            if (items == null || !items.Any())
                throw new ArgumentException("Order must have at least one product.", nameof(items));
            Items = items.Select(i => new OrderItem { Product = i.product, Quantity = i.quantity }).ToList();
        }
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; }

        public OrderItem() { }
        public OrderItem(Product product, int quantity)
        {
            Product = product ?? throw new ArgumentNullException(nameof(product));
            Quantity = quantity;
        }
    }
}
