using Domain.Constant;
using MediatR;

namespace Application.Services.Permissions.Queries;

public class GetAllAvailablePermissionsRequestModel
    : IRequest<GetAllAvailablePermissionsResponseModel>
{
    public string Type { get; set; } // "Admin" or "Company"
}

public class GetAllAvailablePermissionsRequestHandler
    : IRequestHandler<
        GetAllAvailablePermissionsRequestModel,
        GetAllAvailablePermissionsResponseModel
    >
{
    public Task<GetAllAvailablePermissionsResponseModel> Handle(
        GetAllAvailablePermissionsRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var response = new GetAllAvailablePermissionsResponseModel();

        if (request.Type?.ToLower() == "admin")
        {
            response.Data = GetAdminPermissions();
        }
        else if (request.Type?.ToLower() == "company")
        {
            response.Data = GetCompanyPermissions();
        }
        else
        {
            // Return both if no type specified
            response.Data = new PermissionCategoryDto
            {
                Categories = new List<PermissionGroupDto>
                {
                    new PermissionGroupDto
                    {
                        Name = "Admin Permissions",
                        Groups = GetAdminPermissions().Categories,
                    },
                    new PermissionGroupDto
                    {
                        Name = "Company Permissions",
                        Groups = GetCompanyPermissions().Categories,
                    },
                },
            };
        }

        return Task.FromResult(response);
    }

    private PermissionCategoryDto GetAdminPermissions()
    {
        return new PermissionCategoryDto
        {
            Categories = new List<PermissionGroupDto>
            {
                new PermissionGroupDto
                {
                    Name = "Subscription Plans",
                    Permissions = new List<string>
                    {
                        ClaimNames.ViewSubscriptionPlans,
                        ClaimNames.CreateSubscriptionPlans,
                        ClaimNames.UpdateSubscriptionPlans,
                        ClaimNames.DeleteSubscriptionPlans,
                    },
                },
                new PermissionGroupDto
                {
                    Name = "Companies",
                    Permissions = new List<string>
                    {
                        ClaimNames.ViewCompanies,
                        ClaimNames.ApproveCompanies,
                        ClaimNames.RejectCompanies,
                        ClaimNames.AssignSubscription,
                        ClaimNames.UpdateCompanySubscription,
                    },
                },
                new PermissionGroupDto
                {
                    Name = "Admin Staff",
                    Permissions = new List<string>
                    {
                        ClaimNames.ViewAdminStaff,
                        ClaimNames.CreateAdminStaff,
                        ClaimNames.UpdateAdminStaff,
                        ClaimNames.DeleteAdminStaff,
                    },
                },
                new PermissionGroupDto
                {
                    Name = "Admin Roles",
                    Permissions = new List<string>
                    {
                        ClaimNames.ViewAdminRoles,
                        ClaimNames.CreateAdminRoles,
                        ClaimNames.UpdateAdminRoles,
                        ClaimNames.DeleteAdminRoles,
                    },
                },
                new PermissionGroupDto
                {
                    Name = "Settings",
                    Permissions = new List<string>
                    {
                        ClaimNames.ViewGlobalSettings,
                        ClaimNames.UpdateGlobalSettings,
                    },
                },
                new PermissionGroupDto
                {
                    Name = "System",
                    Permissions = new List<string> { ClaimNames.ViewAuditLogs },
                },
            },
        };
    }

    private PermissionCategoryDto GetCompanyPermissions()
    {
        return new PermissionCategoryDto
        {
            Categories = new List<PermissionGroupDto>
            {
                new PermissionGroupDto
                {
                    Name = "Company Roles",
                    Permissions = new List<string>
                    {
                        ClaimNames.ViewCompanyRoles,
                        ClaimNames.CreateCompanyRoles,
                        ClaimNames.UpdateCompanyRoles,
                        ClaimNames.DeleteCompanyRoles,
                    },
                },
                new PermissionGroupDto
                {
                    Name = "Branches",
                    Permissions = new List<string>
                    {
                        ClaimNames.ViewBranches,
                        ClaimNames.CreateBranches,
                        ClaimNames.UpdateBranches,
                        ClaimNames.DeleteBranches,
                    },
                },
                new PermissionGroupDto
                {
                    Name = "Company Staff",
                    Permissions = new List<string>
                    {
                        ClaimNames.ViewCompanyStaff,
                        ClaimNames.CreateCompanyStaff,
                        ClaimNames.UpdateCompanyStaff,
                        ClaimNames.DeleteCompanyStaff,
                    },
                },
                new PermissionGroupDto
                {
                    Name = "Loan Offers",
                    Permissions = new List<string>
                    {
                        ClaimNames.ViewLoanOffers,
                        ClaimNames.CreateLoanOffers,
                        ClaimNames.UpdateLoanOffers,
                        ClaimNames.DeleteLoanOffers,
                    },
                },
                new PermissionGroupDto
                {
                    Name = "Loan Requests",
                    Permissions = new List<string>
                    {
                        ClaimNames.ViewLoanRequests,
                        ClaimNames.ApproveLoanRequests,
                        ClaimNames.RejectLoanRequests,
                        ClaimNames.ViewLoanHistory,
                    },
                },
                new PermissionGroupDto
                {
                    Name = "Payroll",
                    Permissions = new List<string>
                    {
                        ClaimNames.ViewPayroll,
                        ClaimNames.GeneratePayroll,
                        ClaimNames.ApprovePayroll,
                    },
                },
                new PermissionGroupDto
                {
                    Name = "Company Settings",
                    Permissions = new List<string>
                    {
                        ClaimNames.ViewCompanySettings,
                        ClaimNames.UpdateCompanySettings,
                    },
                },
                new PermissionGroupDto
                {
                    Name = "Reports",
                    Permissions = new List<string>
                    {
                        ClaimNames.ViewCompanyReports,
                        ClaimNames.ExportCompanyReports,
                    },
                },
            },
        };
    }
}

public class GetAllAvailablePermissionsResponseModel
{
    public PermissionCategoryDto Data { get; set; }
}

public class PermissionCategoryDto
{
    public List<PermissionGroupDto> Categories { get; set; } = new List<PermissionGroupDto>();
}

public class PermissionGroupDto
{
    public string Name { get; set; }
    public List<string> Permissions { get; set; } = new List<string>();
    public List<PermissionGroupDto> Groups { get; set; } = new List<PermissionGroupDto>();
}
