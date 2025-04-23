using System.Threading.Tasks;
using OrderManagementSystem.API.DTOs;

namespace OrderManagementSystem.API.Services
{
    public interface IOrderService
    {
        Task<int> CreateOrderAsync(OrderCreateRequestDto dto);

        Task<(OrderService.OrderResult? Result, string? Error)> CreateOrderAsync(List<OrderItemDto> items);
        Task<PagedResult<OrderResponseDto>> GetOrdersAsync(int page, int pageSize);
    }
}
