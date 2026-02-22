using Domain.Constant;
using Microsoft.AspNetCore.Authorization;
using WebApi.Authorization;

namespace WebApi.Extension;

public static class AuthorizationPolicyExtension
{
    public static void AddPermissionPolicies(this AuthorizationOptions options)
    {
        // Register policies for all claims
        foreach (var claim in ClaimNames.AllClaims)
        {
            options.AddPolicy(
                claim,
                policy => policy.Requirements.Add(new PermissionRequirement(claim))
            );
        }

        // Add role-based policies for convenience
        options.AddPolicy(
            Policies.AdministratorOnly,
            policy => policy.RequireRole(RoleNames.Administrator)
        );

        options.AddPolicy(
            Policies.CompanyAdminOnly,
            policy => policy.RequireRole(RoleNames.CompanyAdmin)
        );

        options.AddPolicy(Policies.EmployeeOnly, policy => policy.RequireRole(RoleNames.Employee));

        // Combined policies for flexibility
        options.AddPolicy(
            Policies.AdminOrCompanyAdmin,
            policy => policy.RequireRole(RoleNames.Administrator, RoleNames.CompanyAdmin)
        );

        options.AddPolicy(
            Policies.AnyAuthenticatedUser,
            policy => policy.RequireAuthenticatedUser()
        );
    }
}
