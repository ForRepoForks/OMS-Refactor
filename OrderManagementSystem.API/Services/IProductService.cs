using System.Threading.Tasks;
using OrderManagementSystem.API.Models;

namespace OrderManagementSystem.API.Services
{
    public interface IProductService
    {
        Task<Product> CreateProductAsync(ProductCreateDto dto);
        Task ApplyDiscountAsync(Product product, ProductService.DiscountDto discount);
    }
}
