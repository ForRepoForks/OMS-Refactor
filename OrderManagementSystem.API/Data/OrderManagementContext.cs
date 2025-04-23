using Microsoft.EntityFrameworkCore;

namespace OrderManagementSystem.API.Data
{
    public class OrderManagementContext : DbContext
    {
        public OrderManagementContext(DbContextOptions<OrderManagementContext> options)
            : base(options)
        {
        }

        public DbSet<OrderManagementSystem.API.Models.Product> Products { get; set; } = null!;
        public DbSet<OrderManagementSystem.API.Models.Order> Orders { get; set; } = null!;
        public DbSet<OrderManagementSystem.API.Models.OrderItem> OrderItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<OrderManagementSystem.API.Models.Product>()
                .HasIndex(p => p.Name)
                .IsUnique();
        }
    }
}
