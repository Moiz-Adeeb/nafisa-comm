using System.Linq.Expressions;
using Application.Shared;
using Domain.Entities;

namespace Application.Services.CompanyRoles.Models;

public class CompanyRoleDto
{
    public string Id { get; set; }
    public string CompanyId { get; set; }
    public string CompanyName { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public int UserCount { get; set; }
    public List<string> Permissions { get; set; } = new List<string>();
    public DateTimeOffset CreatedDate { get; set; }

    public CompanyRoleDto() { }

    public CompanyRoleDto(CompanyRole role)
    {
        Id = role.Id;
        CompanyId = role.CompanyId;
        Name = role.Name;
        Description = role.Description;
        IsActive = role.IsActive;
        CreatedDate = role.CreatedDate;
    }
}

public class CompanyRoleClaimDto
{
    public string Id { get; set; }
    public string ClaimType { get; set; }
    public string ClaimValue { get; set; }
}

public class CompanyRoleSelector
{
    public static readonly Expression<Func<CompanyRole, CompanyRoleDto>> Selector =
        p => new CompanyRoleDto()
        {
            Id = p.Id,
            CompanyId = p.CompanyId,
            CompanyName =
                p.Company != null
                    ? p.Company.CompanyName ?? $"{p.Company.FirstName} {p.Company.LastName}"
                    : null,
            Name = p.Name,
            Description = p.Description,
            IsActive = p.IsActive,
            UserCount = p.CompanyStaffs.Count(x => x.IsDeleted == false),
            CreatedDate = p.CreatedDate,
        };

    public static readonly Expression<Func<CompanyRole, DropDownDto<string>>> SelectorDropDown =
        p => new DropDownDto<string>() { Id = p.Id, Name = p.Name };
}
