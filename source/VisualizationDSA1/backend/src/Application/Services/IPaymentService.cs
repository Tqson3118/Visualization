using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VisualizationDSA.Application.DTOs;

namespace VisualizationDSA.Application.Services
{
    public interface IPaymentService
    {
        Task<OrderDto> CreateOrderAsync(Guid userId);
        Task<OrderDto> GetOrderStatusAsync(Guid orderId, Guid userId);
        Task<bool> ProcessSePayWebhookAsync(SePayWebhookPayload payload);
        Task<IEnumerable<OrderDto>> GetUserOrdersAsync(Guid userId);
    }
}
