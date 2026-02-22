using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Application.Shared;
using Domain.Entities;

namespace Application.Services.Users.Models;

public class UsersDetailDto : UsersDto { }

public class UsersDto
{
    public string ChatId { get; set; }
    public string Name { get; set; }
    public string UserName { get; set; }
    public string Status { get; set; }
    public DateTimeOffset LastSeen { get; set; }
    public DateTimeOffset CreatedDate { get; set; }

    public UsersDto() { }

    public UsersDto(User user)
    {
        ChatId = user.ChatId;
        Name = user.Name;
        UserName = user.UserName;
        Status = user.Status;
        LastSeen = user.LastSeen;
        CreatedDate = user.CreatedDate;
    }
}

public class UsersSelector
{
    public static readonly Expression<Func<User, UsersDto>> Selector = p => new UsersDto()
    {
        ChatId = p.ChatId,
        UserName = p.UserName,
        Name = p.Name,
        Status = p.Status,
        LastSeen = p.LastSeen,
        CreatedDate = p.CreatedDate,
    };
    public static readonly Expression<Func<User, UsersDetailDto>> SelectorDetail =
        p => new UsersDetailDto()
        {
            ChatId = p.ChatId,
            UserName = p.UserName,
            Name = p.Name,
            Status = p.Status,
            LastSeen = p.LastSeen,
            CreatedDate = p.CreatedDate,
        };

    public static readonly Expression<Func<User, DropDownDto<string>>> SelectorDropDown =
        p => new DropDownDto<string>() { Name = p.Name, Id = p.ChatId };
}
