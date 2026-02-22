using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.Companies.Models;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.Companies.Commands;

public class RegisterCompanyRequestModel : IRequest<RegisterCompanyResponseModel>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string CompanyName { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string BusinessRegistrationCertificate { get; set; } // Base64 or file path
    public string VatCertificate { get; set; } // Base64 or file path
    public string AuthorizationIdProof { get; set; } // Base64 or file path
}

public class RegisterCompanyRequestModelValidator : AbstractValidator<RegisterCompanyRequestModel>
{
    public RegisterCompanyRequestModelValidator()
    {
        RuleFor(p => p.FirstName).Required().Max(100);
        RuleFor(p => p.LastName).Required().Max(100);
        RuleFor(p => p.Email).EmailAddress().Required().Max(150);
        RuleFor(p => p.PhoneNumber).Required().Max(20);
        RuleFor(p => p.CompanyName).Max(200);
        RuleFor(p => p.Password).Password().Max(50);
        RuleFor(p => p.ConfirmPassword)
            .Matches(p => p.Password)
            .WithMessage("Passwords must match");
        RuleFor(p => p.BusinessRegistrationCertificate).Required();
        RuleFor(p => p.VatCertificate).Required();
        RuleFor(p => p.AuthorizationIdProof).Required();
    }
}

public class RegisterCompanyRequestHandler
    : IRequestHandler<RegisterCompanyRequestModel, RegisterCompanyResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly IImageService _imageService;

    public RegisterCompanyRequestHandler(ApplicationDbContext context, IImageService imageService)
    {
        _context = context;
        _imageService = imageService;
    }

    public async Task<RegisterCompanyResponseModel> Handle(
        RegisterCompanyRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var email = request.Email.ToLower();

        // Check if company with email already exists
        var existingCompany = await _context.Companies.AnyAsync(
            c => c.Email == email && !c.IsDeleted,
            cancellationToken
        );

        if (existingCompany)
        {
            throw new AlreadyExistsException($"Company with email '{email}' already exists");
        }

        // Check if user with email already exists
        var existingUser = await _context.Users.AnyAsync(
            u => u.Email == email && !u.IsDeleted,
            cancellationToken
        );

        if (existingUser)
        {
            throw new AlreadyExistsException($"User with email '{email}' already exists");
        }

        // Save documents
        var businessCert = await _imageService.SaveImageToServer(
            request.BusinessRegistrationCertificate,
            ".pdf",
            "companies/documents"
        );

        var vatCert = await _imageService.SaveImageToServer(
            request.VatCertificate,
            ".pdf",
            "companies/documents"
        );

        var authProof = await _imageService.SaveImageToServer(
            request.AuthorizationIdProof,
            ".pdf",
            "companies/documents"
        );

        var company = new Company
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PhoneNumber = request.PhoneNumber.Trim(),
            CompanyName = request.CompanyName?.Trim(),
            BusinessRegistrationCertificate = businessCert,
            VatCertificate = vatCert,
            AuthorizationIdProof = authProof,
            Status = RequestStatus.Pending,
            IsActive = false,
            SubscriptionActive = false,
        };

        await _context.Companies.AddAsync(company, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new RegisterCompanyResponseModel
        {
            Data = new CompanyDto(company),
            Message =
                "Company registration submitted successfully. Please wait for admin approval.",
        };
    }
}

public class RegisterCompanyResponseModel
{
    public CompanyDto Data { get; set; }
    public string Message { get; set; }
}
