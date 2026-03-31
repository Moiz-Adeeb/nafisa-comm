using Persistence.Context;

namespace Application.Interfaces;

public interface IOrderTimeoutBackgroundService
{
    Task CancelExpiredOrders(ApplicationDbContext context, CancellationToken stoppingToken);
}