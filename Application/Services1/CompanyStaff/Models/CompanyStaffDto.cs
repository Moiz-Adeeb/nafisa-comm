using System.Linq.Expressions;
using Application.Shared;
using Domain.Entities;

namespace Application.Services.CompanyStaff.Models;

public class CompanyStaffDto
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Image { get; set; }
    public bool IsEnabled { get; set; }

    // Branch assignment (if entity supports it)
    public string BranchId { get; set; }
    public string BranchName { get; set; }

    // Salary (if entity supports it)
    public decimal? Salary { get; set; }

    // Identity Role
    public string RoleName { get; set; }

    // Custom Company Role
    public string CompanyRoleId { get; set; }
    public string CompanyRoleName { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public CompanyStaffDto() { }

    public CompanyStaffDto(User user)
    {
        Id = user.Id;
        FullName = user.FullName;
        Email = user.Email;
        PhoneNumber = user.PhoneNumber;
        Image = user.Image;
        IsEnabled = user.IsEnabled;
        CompanyRoleId = user.CompanyStaff?.CompanyRoleId;
        CreatedDate = user.CreatedDate;
    }
}

public class CompanyStaffSelector
{
    public static readonly Expression<Func<User, CompanyStaffDto>> Selector =
        p => new CompanyStaffDto()
        {
            Id = p.Id,
            FullName = p.FullName,
            Email = p.Email,
            PhoneNumber = p.PhoneNumber,
            Image = p.Image,
            IsEnabled = p.IsEnabled,
            RoleName =
                p.UserRoles.FirstOrDefault() != null
                    ? p.UserRoles.FirstOrDefault().Role.Name
                    : null,
            CompanyRoleId = p.CompanyStaff.CompanyRoleId,
            CompanyRoleName =
                p.CompanyStaff.CompanyRole != null ? p.CompanyStaff.CompanyRole.Name : null,
            CreatedDate = p.CreatedDate,
            BranchId = p.CompanyStaff.BranchId,
            Salary = p.CompanyStaff.Salary,
            BranchName = p.CompanyStaff.Branch.Name,
        };

    public static readonly Expression<Func<User, DropDownDto<string>>> SelectorDropDown =
        p => new DropDownDto<string>() { Id = p.Id, Name = p.FullName };
}
