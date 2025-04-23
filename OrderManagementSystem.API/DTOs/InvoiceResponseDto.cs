using System.Collections.Generic;

namespace OrderManagementSystem.API.DTOs
{
    public class InvoiceResponseDto
    {
        public List<InvoiceProductDto> Products { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }
}
