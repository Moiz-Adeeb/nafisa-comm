using Application.Interfaces;
using Common.Extensions;
using WebApi.Extension;

namespace WebApi.Services
{
    public class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetTimeZone()
        {
            var data = _httpContextAccessor.HttpContext?.Request.Headers["TimeZone"];
            if (!data.HasValue)
            {
                return "";
            }
            return data.ToString() ?? "";
        }

        public string GetUserId()
        {
            CheckContext();
            return _httpContextAccessor.HttpContext?.User.GetUserId() ?? "";
        }

        public string GetChatId()
        {
            CheckContext();
            return _httpContextAccessor.HttpContext?.User.GetChatId() ?? "";
        }

        public string GetCompanyId()
        {
            CheckContext();
            return _httpContextAccessor.HttpContext?.User.GetCompanyId() ?? "";
        }

        public string GetRole()
        {
            CheckContext();
            return _httpContextAccessor.HttpContext?.User.GetRole() ?? "";
        }

        public bool HasRole(params string[] role)
        {
            CheckContext();
            return role.Select(s => _httpContextAccessor.HttpContext?.User.IsInRole(s) ?? false)
                .Any(hasRole => hasRole);
        }

        public string GetTeamLeaderId()
        {
            return _httpContextAccessor.HttpContext?.User.GetTeamLeaderId() ?? "";
        }

        public string GetTeamLeaderIdOrUserId()
        {
            // if (GetRole() == RoleNames.TeamMember)
            // {
            //     return GetTeamLeaderId();
            // }
            return GetUserId();
        }

        public string[] GetAllUserIds()
        {
            var userId = GetUserId();
            var teamMemberId = GetTeamLeaderId();
            var list = new List<string>();
            if (userId.IsNotNullOrWhiteSpace())
            {
                list.Add(userId);
            }
            if (teamMemberId.IsNotNullOrWhiteSpace())
            {
                list.Add(teamMemberId);
            }
            return list.ToArray();
        }

        public string GetIpAddress()
        {
            if (_httpContextAccessor.HttpContext?.Connection.RemoteIpAddress != null)
                return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString()
                    ?? "";
            return "";
        }

        public string GetDepartmentId()
        {
            return _httpContextAccessor.HttpContext?.User.GetDepartmentId() ?? "";
        }

        public string[] GetGroupIds()
        {
            return _httpContextAccessor.HttpContext?.User.GetGroupIds() ?? [];
        }

        private void CheckContext()
        {
            if (
                _httpContextAccessor.HttpContext == null
                || _httpContextAccessor.HttpContext.User.GetUserId().IsNullOrWhiteSpace()
            )
            {
                throw new UnauthorizedAccessException();
            }
        }
    }
}
