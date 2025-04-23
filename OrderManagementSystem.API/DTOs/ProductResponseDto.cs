using System;

namespace OrderManagementSystem.API.DTOs
{
    public class ProductResponseDto
    {
        public int DiscountQuantityThreshold { get; set; } = 0;
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountPercent { get; set; }
    }
}
