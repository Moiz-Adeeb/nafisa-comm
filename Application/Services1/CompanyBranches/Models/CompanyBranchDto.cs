using System.Linq.Expressions;
using Application.Shared;
using Domain.Entities;

namespace Application.Services.CompanyBranches.Models;

public class CompanyBranchDto
{
    public string Id { get; set; }
    public string CompanyId { get; set; }
    public string CompanyName { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public int StaffCount { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset UpdatedDate { get; set; }

    public CompanyBranchDto() { }

    public CompanyBranchDto(CompanyBranch branch)
    {
        Id = branch.Id;
        CompanyId = branch.CompanyId;
        Name = branch.Name;
        Code = branch.Code;
        PhoneNumber = branch.PhoneNumber;
        Address = branch.Address;
        CreatedDate = branch.CreatedDate;
        UpdatedDate = branch.UpdatedDate;
    }
}

public class CompanyBranchSelector
{
    public static readonly Expression<Func<CompanyBranch, CompanyBranchDto>> Selector =
        p => new CompanyBranchDto()
        {
            Id = p.Id,
            CompanyId = p.CompanyId,
            CompanyName =
                p.Company != null
                    ? p.Company.CompanyName ?? $"{p.Company.FirstName} {p.Company.LastName}"
                    : null,
            Name = p.Name,
            Code = p.Code,
            PhoneNumber = p.PhoneNumber,
            Address = p.Address,
            StaffCount = p.CompanyStaffs != null ? p.CompanyStaffs.Count() : 0,
            CreatedDate = p.CreatedDate,
            UpdatedDate = p.UpdatedDate,
        };

    public static readonly Expression<Func<CompanyBranch, DropDownDto<string>>> SelectorDropDown =
        p => new DropDownDto<string>() { Id = p.Id, Name = p.Name };
}
