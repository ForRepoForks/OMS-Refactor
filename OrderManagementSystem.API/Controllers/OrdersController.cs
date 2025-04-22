using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.API.Data;
using OrderManagementSystem.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderManagementSystem.API.Services;
using OrderManagementSystem.API.DTOs;

namespace OrderManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly OrderManagementContext _context;

        public OrdersController(IOrderService orderService, OrderManagementContext context)
        {
            _orderService = orderService;
            _context = context;
        }


        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequestDto request)
        {
            if (!ModelState.IsValid || request.Items == null || request.Items.Count == 0)
                return BadRequest(ModelState);

            var (result, error) = await _orderService.CreateOrderAsync(request.Items);
            if (error != null)
            {
                if (error.Contains("not found", System.StringComparison.OrdinalIgnoreCase))
                    return NotFound(error);
                ModelState.AddModelError("Order", error);
                return BadRequest(ModelState);
            }
            var response = new OrderResponseDto
            {
                Id = result.Id,
                Items = result.Items.Select(i => new OrderItemResponseDto { ProductId = i.ProductId, Quantity = i.Quantity }).ToList()
            };
            return CreatedAtAction(nameof(CreateOrder), new { id = response.Id }, response);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<OrderResponseDto>>> GetOrders(
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
            public string ProductName { get; set; } = string.Empty;
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
                    .Where(oi => oi.ProductId == product.Id && product.DiscountQuantityThreshold != null && oi.Quantity >= product.DiscountQuantityThreshold.Value)
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
            public string ProductName { get; set; } = string.Empty;
            public decimal DiscountPercent { get; set; }
            public int NumberOfOrders { get; set; }
            public decimal TotalAmount { get; set; }
        }
    }
}
