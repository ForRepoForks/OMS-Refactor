using System.Collections.Generic;
using System.Threading.Tasks;
using OrderManagementSystem.API.DTOs;

namespace OrderManagementSystem.API.Services
{
    public interface IOrderReportingService
    {
        Task<InvoiceResponseDto?> GetOrderInvoiceAsync(int orderId);
        Task<List<DiscountedProductReportItem>> GetDiscountedProductReportAsync();
    }
}
