using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrderManagementSystem.API.DTOs
{
    public class OrderCreateRequestDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required.")]
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
