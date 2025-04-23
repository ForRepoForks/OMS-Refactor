using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.API.Data;
using OrderManagementSystem.API.DTOs;

namespace OrderManagementSystem.API.Services
{
    public class OrderReportingService : IOrderReportingService
    {
        private readonly OrderManagementContext _context;
        public OrderReportingService(OrderManagementContext context)
        {
            _context = context;
        }

        public async Task<InvoiceResponseDto?> GetOrderInvoiceAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                return null;

            var invoiceProducts = new List<InvoiceProductDto>();
            decimal total = 0m;
            foreach (var item in order.Items)
            {
                var product = item.Product;
                decimal discountPercent = 0m;
                var discountPct = product.DiscountPercentage ?? 0m;
                if (product.DiscountQuantityThreshold != null && item.Quantity >= product.DiscountQuantityThreshold)
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
            return new InvoiceResponseDto
            {
                Products = invoiceProducts,
                TotalAmount = total
            };
        }

        public async Task<List<DiscountedProductReportItem>> GetDiscountedProductReportAsync()
        {
            var discountedProducts = await _context.Products
                .Where(p => p.DiscountPercentage != null && p.DiscountPercentage > 0 && p.DiscountQuantityThreshold != null)
                .ToListAsync();

            var report = new List<DiscountedProductReportItem>();
            foreach (var product in discountedProducts)
            {
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
            return report;
        }
    }
}
