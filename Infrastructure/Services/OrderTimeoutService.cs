using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence.Context;

namespace Infrastructure.Services;

public class OrderTimeoutBackgroundService : BackgroundService, IOrderTimeoutBackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderTimeoutBackgroundService> _logger;

    public OrderTimeoutBackgroundService(
        IServiceProvider serviceProvider, 
        ILogger<OrderTimeoutBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Order Timeout Background Service is running...");
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);


            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    await CancelExpiredOrders(context, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cancelling expired orders.");
            }

            // Run every 1 hour (adjust frequency as needed)
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    public async Task CancelExpiredOrders(ApplicationDbContext context, CancellationToken stoppingToken)
    {
        var expirationTime = DateTimeOffset.UtcNow.AddHours(-24);

        // 1. Find orders that are still Pending and older than 24 hours
        var expiredOrders = await context.Order
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.Product)
            .Where(o => o.Status == OrderStatus.Pending && o.OrderDate <= expirationTime && !o.IsDeleted)
            .ToListAsync(stoppingToken);

        if (!expiredOrders.Any()) return;

        using var transaction = await context.Database.BeginTransactionAsync(stoppingToken);
        try
        {
            foreach (var order in expiredOrders)
            {
                order.Status = OrderStatus.Cancelled;
                
                // 2. Restore Stock
                foreach (var item in order.OrderItems)
                {
                    if (item.Product != null)
                    {
                        item.Product.Stock += item.Quantity;
                        item.Product.SoldQuantity -= item.Quantity;
                    }
                }
                
                _logger.LogInformation($"Order {order.OrderNumber} cancelled due to timeout.");
            }

            await context.SaveChangesAsync(stoppingToken);
            await transaction.CommitAsync(stoppingToken);
        }
        catch
        {
            await transaction.RollbackAsync(stoppingToken);
            throw;
        }
    }
}