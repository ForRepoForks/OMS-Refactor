
using Microsoft.EntityFrameworkCore;

namespace OrderManagementSystem.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Services.AddDbContext<OrderManagementSystem.API.Data.OrderManagementContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<OrderManagementSystem.API.Services.IProductService, OrderManagementSystem.API.Services.ProductService>(provider =>
            {
                var context = provider.GetRequiredService<OrderManagementSystem.API.Data.OrderManagementContext>();
                var mapper = provider.GetRequiredService<AutoMapper.IMapper>();
                return new OrderManagementSystem.API.Services.ProductService(context, mapper);
            });
            builder.Services.AddScoped<OrderManagementSystem.API.Services.IOrderService, OrderManagementSystem.API.Services.OrderService>();
            builder.Services.AddScoped<OrderManagementSystem.API.Services.IOrderReportingService, OrderManagementSystem.API.Services.OrderReportingService>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers();
            builder.Services.AddAutoMapper(typeof(Program));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Register ArgumentExceptionMiddleware before controllers
            app.UseMiddleware<OrderManagementSystem.API.ArgumentExceptionMiddleware>();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
