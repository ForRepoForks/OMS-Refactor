namespace OrderManagementSystem.API.DTOs
{
    public class OrderInvoiceProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal Amount { get; set; }
    }
}
