using Application.Extensions;
using Common.Requests;
using Common.Response;
using FluentValidation;
using MediatR;

public abstract class GetCursorPagedRequest<T> : IRequest<T>
{
    // The timestamp of the last item from the previous page
    public DateTimeOffset? CursorTime { get; set; }

    // The unique ID of the last item from the previous page (to handle identical timestamps)
    public string? CursorId { get; set; }

    // How many items to fetch
    public int Limit { get; set; } = 20;
}
