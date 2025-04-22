using Microsoft.EntityFrameworkCore;

namespace OrderManagementSystem.API.Data
{
    public class OrderManagementContext(DbContextOptions<OrderManagementContext> options) : DbContext(options)
    {
        public DbSet<OrderManagementSystem.API.Models.Product> Products { get; set; } = null!;
        public DbSet<OrderManagementSystem.API.Models.Order> Orders { get; set; } = null!;
        public DbSet<OrderManagementSystem.API.Models.OrderItem> OrderItems { get; set; } = null!;
    }
}
