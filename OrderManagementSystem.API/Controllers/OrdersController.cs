using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.API.Data;
using OrderManagementSystem.API.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderManagementContext _context;

        public OrdersController(OrderManagementContext context)
        {
            _context = context;
        }

        public class CreateOrderRequest
        {
            [Required]
            [MinLength(1, ErrorMessage = "At least one item is required.")]
            public List<OrderItemDto> Items { get; set; } = new();
        }
        public class OrderItemDto
        {
            [Required]
            public int ProductId { get; set; }
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
            public int Quantity { get; set; }
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

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid || request.Items == null || request.Items.Count == 0)
                return BadRequest(ModelState);

            // Additional validation: ensure all items have both ProductId and Quantity set
            foreach (var item in request.Items)
            {
                // ProductId is required (should not be 0 or default)
                // Quantity is required and must be >= 1
                if (item == null || item.ProductId == 0 || item.Quantity < 1)
                {
                    // Add model errors for clarity (optional)
                    if (item == null)
                        ModelState.AddModelError("Items", "Order item cannot be null.");
                    else
                    {
                        if (item.ProductId == 0)
                            ModelState.AddModelError("Items.ProductId", "ProductId is required and must be greater than 0.");
                        if (item.Quantity < 1)
                            ModelState.AddModelError("Items.Quantity", "Quantity must be at least 1.");
                    }
                    return BadRequest(ModelState);
                }
            }

            // Validate all product IDs
            var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();
            if (products.Count != productIds.Count)
                return NotFound("One or more products not found.");

            // Map items
            var order = new Order();
            foreach (var item in request.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);
                order.Items.Add(new OrderItem { Product = product, ProductId = product.Id, Quantity = item.Quantity });
            }
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var response = new OrderResponse
            {
                Id = order.Id,
                Items = order.Items.Select(i => new OrderItemResponse { ProductId = i.ProductId, Quantity = i.Quantity }).ToList()
            };
            return CreatedAtAction(nameof(CreateOrder), new { id = order.Id }, response);
        }

        [HttpGet]
public async Task<ActionResult<PagedResult<OrderResponse>>> GetOrders(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
{
    if (page < 1 || pageSize < 1 || pageSize > 100)
        return BadRequest("Invalid pagination parameters.");

    var query = _context.Orders.Include(o => o.Items).AsQueryable();
    var totalCount = await query.CountAsync();
    var orders = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    var response = orders.Select(order => new OrderResponse
    {
        Id = order.Id,
        Items = order.Items.Select(i => new OrderItemResponse { ProductId = i.ProductId, Quantity = i.Quantity }).ToList()
    }).ToList();
    var result = new PagedResult<OrderResponse>
    {
        Items = response,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    };
    return Ok(result);
}
        [HttpGet("{id}/invoice")]
        public async Task<IActionResult> GetOrderInvoice(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
                return NotFound();

            var invoiceProducts = new List<InvoiceProductDto>();
            decimal total = 0m;
            foreach (var item in order.Items)
            {
                var product = item.Product;
                decimal discountPercent = 0m;
                var discountPct = product.DiscountPercentage ?? 0m;
                var discountQtyThreshold = product.DiscountQuantityThreshold ?? int.MaxValue;
                if (discountPct > 0 && item.Quantity >= discountQtyThreshold)
                {
                    discountPercent = discountPct;
                }
                var lineAmount = product.Price * item.Quantity * (1 - discountPercent / 100);
                invoiceProducts.Add(new InvoiceProductDto
                {
                    ProductName = product.Name,
                    Quantity = item.Quantity,
                    DiscountPercent = discountPercent,
                    Amount = lineAmount
                });
                total += lineAmount;
            }
            var invoice = new InvoiceResponseDto
            {
                Products = invoiceProducts,
                TotalAmount = total
            };
            return Ok(invoice);
        }

        public class InvoiceResponseDto
        {
            public List<InvoiceProductDto> Products { get; set; } = new();
            public decimal TotalAmount { get; set; }
        }
        public class InvoiceProductDto
        {
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal DiscountPercent { get; set; }
            public decimal Amount { get; set; }
        }
        [HttpGet("/api/reports/discounted-products")]
        public async Task<IActionResult> GetDiscountedProductReport()
        {
            // Get all products with a discount
            var discountedProducts = await _context.Products
                .Where(p => p.DiscountPercentage != null && p.DiscountPercentage > 0 && p.DiscountQuantityThreshold != null)
                .ToListAsync();

            var report = new List<DiscountedProductReportItem>();
            foreach (var product in discountedProducts)
            {
                // Find all order items where this product was ordered with quantity >= threshold
                var orderItems = await _context.OrderItems
                    .Where(oi => oi.ProductId == product.Id && oi.Quantity >= product.DiscountQuantityThreshold)
                    .ToListAsync();
                if (orderItems.Count == 0)
                    continue;
                var orderIdsWithDiscount = orderItems.Select(oi => oi.OrderId).Distinct().Count();
                var totalAmount = orderItems.Sum(oi => oi.Quantity * product.Price * (1 - (product.DiscountPercentage ?? 0) / 100));
                report.Add(new DiscountedProductReportItem
                {
                    ProductName = product.Name,
                    DiscountPercent = product.DiscountPercentage ?? 0,
                    NumberOfOrders = orderIdsWithDiscount,
                    TotalAmount = totalAmount
                });
            }
            return Ok(report);
        }

        public class DiscountedProductReportItem
        {
            public string ProductName { get; set; }
            public decimal DiscountPercent { get; set; }
            public int NumberOfOrders { get; set; }
            public decimal TotalAmount { get; set; }
        }
    }
}
