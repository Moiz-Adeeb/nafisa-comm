using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Application.Shared;
using Domain.Entities;

namespace Application.Services.Users.Models;

public class UserDetailDto : UserDto { }

public class UserDto
{
    public string Id  { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Image { get; set; }
    public string Role { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsAllowEmail { get; set; }
    public bool IsAllowNotification { get; set; }

    public UserDto() { }

    public UserDto(User user)
    {
        Id = user.Id;
        Name = user.Name;
        Email = user.Email;
        Image = user.Image;
        CreatedDate = user.CreatedDate;
        IsEnabled = user.IsEnabled;
    }
}

public class UserSelector
{
    public static readonly Expression<Func<User, UserDto>> Selector = p => new UserDto()
    {
        Name = p.Name,
        Id = p.Id,
        Email = p.Email,
        Image = p.Image,
        Role = p.UserRoles.FirstOrDefault().Role.Name,
        CreatedDate = p.CreatedDate,
        IsEnabled = p.IsEnabled,
    };
    public static readonly Expression<Func<User, UserDetailDto>> SelectorDetail =
        p => new UserDetailDto()
        {
            Name = p.Name,
            Id = p.Id,
            Email = p.Email,
            Role = p.UserRoles.FirstOrDefault().Role.Name,
            Image = p.Image,
            CreatedDate = p.CreatedDate,
            IsEnabled = p.IsEnabled,
        };

    public static readonly Expression<Func<User, DropDownDto<string>>> SelectorDropDown =
        p => new DropDownDto<string>() { Name = p.Name, Id = p.Id };
}
