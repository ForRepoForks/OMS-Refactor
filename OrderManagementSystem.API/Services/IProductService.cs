using System.Threading.Tasks;
using OrderManagementSystem.API.Models;

namespace OrderManagementSystem.API.Services
{
    public interface IProductService
    {
        Task<Product> CreateProductAsync(ProductCreateDto dto);
        Task<PagedResult<DTOs.ProductResponseDto>> GetProductsAsync(string? name, int page, int pageSize);
        Task<DTOs.ProductResponseDto?> ApplyDiscountAsync(int id, ProductService.DiscountDto discount);
    }
}
