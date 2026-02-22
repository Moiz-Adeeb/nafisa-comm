using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Application.Shared;
using Domain.Entities;

namespace Application.Services.Users.Models;

public class UsersDetailDto : UsersDto { }

public class UsersDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string ProfilePicture { get; set; }
    public DateTimeOffset CreatedDate { get; set; }

    public UsersDto() { }

    public UsersDto(User user)
    {
        Name = user.Name;
        Email = user.Email;
        ProfilePicture = user.ProfilePicture;
        CreatedDate = user.CreatedDate;
    }
}

public class UserSelector
{
    public static readonly Expression<Func<User, UsersDto>> Selector = p => new UsersDto()
    {
        Email = p.Email,
        Name = p.Name,
        CreatedDate = p.CreatedDate,
    };
    public static readonly Expression<Func<User, UsersDetailDto>> SelectorDetail =
        p => new UsersDetailDto()
        {
            Email = p.Email,
            Name = p.Name,
            ProfilePicture = p.ProfilePicture,
            CreatedDate = p.CreatedDate,
        };

    public static readonly Expression<Func<User, DropDownDto<string>>> SelectorDropDown =
        p => new DropDownDto<string>() { Name = p.Name, Id = p.Id };
}
