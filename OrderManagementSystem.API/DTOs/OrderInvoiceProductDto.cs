namespace OrderManagementSystem.API.DTOs
{
    public class OrderInvoiceProductDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal Amount { get; set; }
    }
}
