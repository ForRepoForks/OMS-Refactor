using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.API.Data;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderManagementSystem.API.Services;
using OrderManagementSystem.API.DTOs;

namespace OrderManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrderReportingService _orderReportingService;

        public OrdersController(IOrderService orderService, IOrderReportingService orderReportingService)
        {
            _orderService = orderService;
            _orderReportingService = orderReportingService;
        }


        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequestDto request)
        {
            if (!ModelState.IsValid || request.Items == null || request.Items.Count == 0)
                return BadRequest(ModelState);

            var (result, error) = await _orderService.CreateOrderAsync(request.Items);
            if (error != null)
            {
                if (error.Contains("not found", System.StringComparison.OrdinalIgnoreCase))
                    return NotFound(error);
                ModelState.AddModelError("Order", error);
                return BadRequest(ModelState);
            }
            var response = new OrderResponseDto
            {
                Id = result.Id,
                Items = result.Items.Select(i => new OrderItemResponseDto { ProductId = i.ProductId, Quantity = i.Quantity }).ToList()
            };
            return CreatedAtAction(nameof(CreateOrder), new { id = response.Id }, response);
        }

        [HttpGet]
        public async Task<ActionResult<OrderManagementSystem.API.DTOs.PagedResult<OrderResponseDto>>> GetOrders(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _orderService.GetOrdersAsync(page, pageSize);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{id}/invoice")]
        public async Task<IActionResult> GetOrderInvoice(int id)
        {
            var invoice = await _orderReportingService.GetOrderInvoiceAsync(id);
            if (invoice == null)
                return NotFound();
            return Ok(invoice);
        }

        [HttpGet("/api/reports/discounted-products")]
        public async Task<IActionResult> GetDiscountedProductReport()
        {
            var report = await _orderReportingService.GetDiscountedProductReportAsync();
            return Ok(report);
        }


    }
}
