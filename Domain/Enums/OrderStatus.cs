namespace Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    InTransit = 4,
    Delivered = 5,
    Completed = 6,
    Cancelled = 7,
    Refunded = 8,
    Failed = 9
}