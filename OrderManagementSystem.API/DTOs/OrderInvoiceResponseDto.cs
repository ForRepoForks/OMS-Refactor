using System.Collections.Generic;

namespace OrderManagementSystem.API.DTOs
{
    public class OrderInvoiceResponseDto
    {
        public List<OrderInvoiceProductDto> Products { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }
}
