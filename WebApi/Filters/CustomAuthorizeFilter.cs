using System.IdentityModel.Tokens.Jwt;
using Application.Exceptions;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.Filters
{
    public class CustomAuthorizeFilter : IAsyncAuthorizationFilter
    {
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var filters = context.Filters.ToList();
            if (filters.Any(p => p.GetType() == typeof(AuthorizeFilter)))
            {
                if (context.HttpContext.Request.Headers.ContainsKey("Authorization"))
                {
                    var token = context.HttpContext.Request.Headers["Authorization"].ToString();
                    if (token.StartsWith("Bearer "))
                    {
                        token = token.Substring("Bearer ".Length);
                        if (IsExpired(token))
                        {
                            throw new TokenExpiredException();
                        }
                    }
                }
                else
                {
                    throw new TokenNotFoundException();
                }
            }
            return Task.CompletedTask;
        }

        private bool IsExpired(string token)
        {
            var tokenS = new JwtSecurityToken(token);
            DateTime compareTo = DateTime.Now.AddMinutes(1);
            int result = DateTime.Compare(tokenS.ValidTo, compareTo);
            return result < 0;
        }
    }
}
