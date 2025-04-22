using System.Collections.Generic;

namespace OrderManagementSystem.API.DTOs
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public List<OrderItemResponseDto> Items { get; set; } = new();
    }
}
