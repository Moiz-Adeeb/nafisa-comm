using System.Globalization;
using Application.Interfaces;
using Application.Services.Users.Models;
using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation;
using MediatR;
using Persistence.Context;

namespace Application.Services.Users.Commands;

public class ImportUserDto
{
    public string FullName { get; set; }
    public string Email { get; set; }
}

public sealed class ImportUserDtoMapper : ClassMap<ImportUserDto>
{
    public ImportUserDtoMapper()
    {
        Map(p => p.FullName).Name("fullname");
        Map(p => p.Email).Name("email");
    }
}

public class ImportUserBulkRequestModel : IRequest<ImportUserBulkResponseModel>
{
    public string Host { get; set; }
    public Stream Stream { get; set; }
}

public class ImportUserBulkRequestModelValidator : AbstractValidator<ImportUserBulkRequestModel>
{
    public ImportUserBulkRequestModelValidator() { }
}

public class ImportUserBulkRequestHandler
    : IRequestHandler<ImportUserBulkRequestModel, ImportUserBulkResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IMediator _mediator;

    public ImportUserBulkRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IMediator mediator
    )
    {
        _context = context;
        _sessionService = sessionService;
        _mediator = mediator;
    }

    public async Task<ImportUserBulkResponseModel> Handle(
        ImportUserBulkRequestModel request,
        CancellationToken cancellationToken
    )
    {
        using var reader = new StreamReader(request.Stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<ImportUserDtoMapper>();
        var dockerCsv = csv.GetRecords<ImportUserDto>().ToList();
        var response = await _mediator.Send(
            new CreateUserBulkRequestModel()
            {
                Host = request.Host,
                Users = dockerCsv
                    .Select(p => new CreateUserBulkDto() { Email = p.Email, FullName = p.FullName })
                    .ToList(),
            },
            cancellationToken
        );
        return new ImportUserBulkResponseModel() { Data = response.Data };
    }
}

public class ImportUserBulkResponseModel
{
    public List<UserDto> Data { get; set; }
}
