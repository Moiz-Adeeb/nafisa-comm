using Application.Extensions;
using Common.Requests;
using Common.Response;
using FluentValidation;
using MediatR;

namespace Application.Shared
{
    public abstract class GetPagedRequest<T> : PageRequestModel, IRequest<T> { }

    public abstract class GetPagedRequest : PageRequestModel, IRequest<ResponseViewModel> { }

    public abstract class GetUserPagedRequest : GetPagedRequest
    {
        public string UserId { get; set; }
    }

    public abstract class GetUserPagedRequest<T> : GetPagedRequest<T>
    {
        public string UserId { get; set; }
    }

    public class PageRequestValidator<T> : AbstractValidator<T>
        where T : PageRequestModel
    {
        public PageRequestValidator()
        {
            RuleFor(p => p.Page).Required();
            RuleFor(p => p.PageSize).MustBeOneOf(-1, 5, 10, 25, 50, 100, 200, 1000);
        }
    }

    public class UserPageRequestValidator<T> : AbstractValidator<T>
        where T : GetUserPagedRequest
    {
        public UserPageRequestValidator()
        {
            RuleFor(p => p.Page).Required();
            RuleFor(p => p.PageSize).MustBeOneOf(-1, 5, 10, 25, 50, 100, 200, 1000);
            RuleFor(p => p.UserId).Required();
        }
    }

    public class DeleteIdRequest : IRequest<ResponseViewModel>
    {
        public int Id { get; set; }
    }

    public class DeleteIdValidator<T> : AbstractValidator<T>
        where T : DeleteIdRequest
    {
        public DeleteIdValidator()
        {
            RuleFor(p => p.Id).Required();
        }
    }
}
