namespace OrderManagementSystem.API.DTOs
{
    public class DiscountedProductReportItem
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; }
        public int NumberOfOrders { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
