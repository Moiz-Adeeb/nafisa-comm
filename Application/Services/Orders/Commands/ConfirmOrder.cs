using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.Emails.Commands;
using Application.Services.Orders.Models;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Orders.Commands;

public class ConfirmOrderRequestModel : IRequest<ConfirmOrderResponseModel>
{
    public string OrderId { get; set; }
    public bool Confirmation { get; set; }
}

public class ConfirmOrderRequestModelValidator : AbstractValidator<ConfirmOrderRequestModel>
{
    public ConfirmOrderRequestModelValidator()
    {
        RuleFor(x => x.OrderId).Required();
        RuleFor(x => x.Confirmation).NotNull();
    }
}

public class ConfirmOrderRequestHandler : IRequestHandler<ConfirmOrderRequestModel, ConfirmOrderResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _backgroundTaskQueueService;


    public ConfirmOrderRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService backgroundTaskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _backgroundTaskQueueService = backgroundTaskQueueService;
    }

    public async Task<ConfirmOrderResponseModel> Handle(
        ConfirmOrderRequestModel request,
        CancellationToken cancellationToken
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var userId = _sessionService.GetUserId();
            var order = await _context.Order  
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => 
                    o.Id == request.OrderId &&
                    o.UserId == userId &&
                    o.IsDeleted == false,   
                    cancellationToken
                );
            
            var user = await _context.Users.FindAsync(userId, cancellationToken);

            if (order == null) throw new NotFoundException(nameof(request.OrderId));
            if (order.Status != OrderStatus.Pending) throw new BadRequestException($"Order does not need to be confirmed");

            if (request.Confirmation == true)
            {
                var emailBody =
                    $@"<div style=""font-family: Arial, sans-serif;"">
                        <h2>Order Confirmed!</h2>
                        <p>Hello {{user.Name ?? ""Customer""}},</p>
                        <p>Your order <strong>{{order.OrderNumber}}</strong> has been confirmed and is now being processed.</p>
                        <p>Total Amount: <strong>{{order.TotalAmount:C}}</strong></p>
                        <p>Thank you for shopping with us!</p>
                    </div>";
                
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _backgroundTaskQueueService.QueueBackgroundWorkItem(
                    new SendEmailRequestModel()
                    {
                        UserId = user.Id,
                        IsCheckForEmailAllow = true,
                        Subject = $"Order Confirmed - {order.OrderNumber}",
                        Body = emailBody,
                    }
                );
                order.Status = OrderStatus.Confirmed;
            }
            else
            {
                order.Status = OrderStatus.Cancelled;
                foreach (var item in order.OrderItems)
                {
                    if (item.Product != null)
                    {
                        item.Product.Stock += item.Quantity; 
                        item.Product.SoldQuantity -= item.Quantity; 
                    }
                }
                order.IsDeleted = true;
                
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            

            return new ConfirmOrderResponseModel() { Data = request.Confirmation };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

public class ConfirmOrderResponseModel
{
    public bool Data { get; set; }
}
