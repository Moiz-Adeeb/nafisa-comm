using System.Security.Claims;
using Common.Constants;
using Domain.Constant;

namespace WebApi.Extension
{
    public static class ClaimPrincipleExtension
    {
        public static string GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirstValue(CustomClaimTypes.UserId);
        }
        public static string GetChatId(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirstValue(CustomClaimTypes.ChatId);
        }

        public static int[] GetMarketplaces(this ClaimsPrincipal claimsPrincipal)
        {
            var marketplaces = claimsPrincipal
                .FindAll(p => p.Type == CustomClaimTypes.MarketPlace)
                .ToList();
            return marketplaces.Any()
                ? marketplaces.Select(p => int.Parse(p.Value)).ToArray()
                : Array.Empty<int>();
        }

        public static string GetMerchantId(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirstValue(CustomClaimTypes.MerchantId);
        }

        public static string GetCompanyId(this ClaimsPrincipal claimsPrincipal)
        {
            var id = claimsPrincipal.FindFirstValue(CustomClaimTypes.CompanyId);
            return id;
        }

        public static string GetTeamLeaderId(this ClaimsPrincipal claimsPrincipal)
        {
            var id = claimsPrincipal.FindFirstValue(CustomClaimTypes.TeamLeaderId);
            return id;
        }

        public static string GetDepartmentId(this ClaimsPrincipal claimsPrincipal)
        {
            var id = claimsPrincipal.FindFirstValue(CustomClaimTypes.DepartmentId);
            return id;
        }

        public static string[] GetGroupIds(this ClaimsPrincipal claimsPrincipal)
        {
            var id = claimsPrincipal.FindFirstValue(CustomClaimTypes.GroupIds);
            return id?.Split(',') ?? [];
        }

        public static string GetRole(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.IsInRole(RoleNames.Administrator))
            {
                return RoleNames.Administrator;
            }
            if (claimsPrincipal.IsInRole(RoleNames.CompanyAdmin))
            {
                return RoleNames.CompanyAdmin;
            }
            if (claimsPrincipal.IsInRole(RoleNames.Employee))
            {
                return RoleNames.Employee;
            }
            return "";
        }
    }
}
