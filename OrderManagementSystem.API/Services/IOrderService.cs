using System.Threading.Tasks;
using OrderManagementSystem.API.DTOs;

namespace OrderManagementSystem.API.Services
{
    public interface IOrderService
    {
        Task<int> CreateOrderAsync(OrderCreateRequestDto dto);

        // New method for controller extraction
        Task<(OrderService.OrderResult? Result, string? Error)> CreateOrderAsync(List<OrderItemDto> items);
    }
}
