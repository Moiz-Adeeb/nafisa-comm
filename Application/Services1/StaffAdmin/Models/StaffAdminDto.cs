using System.Linq.Expressions;
using Application.Shared;
using Domain.Entities;

namespace Application.Services.StaffAdmin.Models;

public class StaffAdminDto
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Image { get; set; }
    public bool IsEnabled { get; set; }

    // Identity Role
    public string RoleName { get; set; }

    // Custom Admin Role
    public string AdminRoleId { get; set; }
    public string AdminRoleName { get; set; }

    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset UpdatedDate { get; set; }

    public StaffAdminDto() { }

    public StaffAdminDto(User user)
    {
        Id = user.Id;
        FullName = user.FullName;
        Email = user.Email;
        PhoneNumber = user.PhoneNumber;
        Image = user.Image;
        IsEnabled = user.IsEnabled;
        AdminRoleId = user.AdminRoleId;
        CreatedDate = user.CreatedDate;
        UpdatedDate = user.UpdatedDate;
    }
}

public class StaffAdminSelector
{
    public static readonly Expression<Func<User, StaffAdminDto>> Selector = p => new StaffAdminDto()
    {
        Id = p.Id,
        FullName = p.FullName,
        Email = p.Email,
        PhoneNumber = p.PhoneNumber,
        Image = p.Image,
        IsEnabled = p.IsEnabled,
        RoleName =
            p.UserRoles.FirstOrDefault() != null ? p.UserRoles.FirstOrDefault().Role.Name : null,
        AdminRoleId = p.AdminRoleId,
        AdminRoleName = p.AdminRole != null ? p.AdminRole.Name : null,
        CreatedDate = p.CreatedDate,
        UpdatedDate = p.UpdatedDate,
    };

    public static readonly Expression<Func<User, DropDownDto<string>>> SelectorDropDown =
        p => new DropDownDto<string>() { Id = p.Id, Name = p.FullName };
}
