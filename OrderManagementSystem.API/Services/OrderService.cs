using System.Threading.Tasks;
using OrderManagementSystem.API.Data;
using OrderManagementSystem.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace OrderManagementSystem.API.Services
{
    public class OrderService : IOrderService
    {
        public class OrderResult
        {
            public int Id { get; set; }
            public List<OrderItemResult> Items { get; set; } = new();
        }
        public class OrderItemResult
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
        private readonly OrderManagementContext _context;
        public OrderService(OrderManagementContext context)
        {
            _context = context;
        }

        public async Task<int> CreateOrderAsync(OrderCreateRequestDto dto)
        {
            // Minimal implementation for TDD: assumes valid input and products exist
            var products = await _context.Products
                .Where(p => dto.Items.Select(i => i.ProductId).Contains(p.Id))
                .ToListAsync();

            var order = new Models.Order();
            foreach (var item in dto.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);
                order.Items.Add(new Models.OrderItem { Product = product, ProductId = product.Id, Quantity = item.Quantity });
            }
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order.Id;
        }
        // New method for controller extraction
        public async Task<(OrderResult? Result, string? Error)> CreateOrderAsync(List<OrderItemDto> items)
        {
            if (items == null || items.Count == 0)
                return (null, "At least one item is required.");
            foreach (var item in items)
            {
                if (item == null)
                    return (null, "Order item cannot be null.");
                if (item.ProductId == 0)
                    return (null, "ProductId is required and must be greater than 0.");
                if (item.Quantity < 1)
                    return (null, "Quantity must be at least 1.");
            }
            var productIds = items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();
            if (products.Count != productIds.Count)
                return (null, "One or more products not found.");
            var order = new Models.Order();
            foreach (var item in items)
            {
                var product = products.First(p => p.Id == item.ProductId);
                order.Items.Add(new Models.OrderItem { Product = product, ProductId = product.Id, Quantity = item.Quantity });
            }
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            var result = new OrderResult
            {
                Id = order.Id,
                Items = order.Items.Select(i => new OrderItemResult { ProductId = i.ProductId, Quantity = i.Quantity }).ToList()
            };
            return (result, null);
        }
        public async Task<PagedResult<OrderResponseDto>> GetOrdersAsync(int page, int pageSize)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                throw new ArgumentException("Invalid pagination parameters.");

            var query = _context.Orders.Include(o => o.Items).AsQueryable();
            var totalCount = await query.CountAsync();
            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<OrderResponseDto>
            {
                Items = orders.Select(o => new OrderResponseDto
                {
                    Id = o.Id,
                    Items = o.Items.Select(i => new OrderItemResponseDto
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    }).ToList()
                }).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
            return result;
        }
    }
}
