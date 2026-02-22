using System.Linq.Expressions;
using Application.Shared;
using Domain.Entities;

namespace Application.Services.AdminRoles.Models;

public class AdminRoleDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public int UserCount { get; set; }
    public List<string> Permissions { get; set; } = new List<string>();
    public DateTimeOffset CreatedDate { get; set; }

    public AdminRoleDto() { }

    public AdminRoleDto(AdminRole role)
    {
        Id = role.Id;
        Name = role.Name;
        Description = role.Description;
        IsActive = role.IsActive;
        CreatedDate = role.CreatedDate;
    }
}

public class AdminRoleClaimDto
{
    public string Id { get; set; }
    public string ClaimType { get; set; }
    public string ClaimValue { get; set; }
}

public class AdminRoleSelector
{
    public static readonly Expression<Func<AdminRole, AdminRoleDto>> Selector =
        p => new AdminRoleDto()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            IsActive = p.IsActive,
            UserCount = p.Users.Count,
            CreatedDate = p.CreatedDate,
            Permissions = p
                .AdminRoleClaims.Where(x => x.IsDeleted == false)
                .Select(x => x.ClaimValue)
                .ToList(),
        };

    public static readonly Expression<Func<AdminRole, DropDownDto<string>>> SelectorDropDown =
        p => new DropDownDto<string>() { Id = p.Id, Name = p.Name };
}
