using Application.Exceptions;
using Application.Extensions;
using Application.Services.Companies.Models;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Companies.Queries;

public class GetCompanyByIdRequestModel : IRequest<GetCompanyByIdResponseModel>
{
    public string Id { get; set; }
}

public class GetCompanyByIdRequestModelValidator : AbstractValidator<GetCompanyByIdRequestModel>
{
    public GetCompanyByIdRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
    }
}

public class GetCompanyByIdRequestHandler
    : IRequestHandler<GetCompanyByIdRequestModel, GetCompanyByIdResponseModel>
{
    private readonly ApplicationDbContext _context;

    public GetCompanyByIdRequestHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetCompanyByIdResponseModel> Handle(
        GetCompanyByIdRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var data = await _context.Companies.GetByWithSelectAsync(
            p => p.Id == request.Id,
            CompanySelector.Selector,
            cancellationToken: cancellationToken
        );

        if (data == null)
        {
            throw new NotFoundException(nameof(Company));
        }

        return new GetCompanyByIdResponseModel { Data = data };
    }
}

public class GetCompanyByIdResponseModel
{
    public CompanyDto Data { get; set; }
}
