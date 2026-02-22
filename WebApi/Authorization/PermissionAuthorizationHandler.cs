using System.Security.Claims;
using Domain.Constant;
using Microsoft.AspNetCore.Authorization;

namespace WebApi.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement
    )
    {
        if (context.User == null || !context.User.Identity.IsAuthenticated)
        {
            return Task.CompletedTask;
        }

        // Get user roles
        var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        // Check if user is Administrator - has access to all admin permissions
        if (roles.Contains(RoleNames.Administrator))
        {
            // Administrator has access to all admin claims
            if (ClaimNames.AllAdminClaims.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        // Check if user is CompanyAdmin - has access to all company permissions
        if (roles.Contains(RoleNames.CompanyAdmin))
        {
            // CompanyAdmin has access to all company claims
            if (ClaimNames.AllCompanyAdminClaims.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        // Check if user is Employee - has access to employee permissions based on their claims
        if (roles.Contains(RoleNames.Employee))
        {
            // Employee must have the specific claim
            if (
                context.User.HasClaim(c =>
                    c.Type == "Permission" && c.Value == requirement.Permission
                )
            )
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        // For custom roles (not Administrator, CompanyAdmin, Employee), check for specific permission claim
        var hasPermission = context.User.Claims.Any(c =>
            c.Type == "Permission" && c.Value == requirement.Permission
        );

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
