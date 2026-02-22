using System.Security.Claims;
using Application.Exceptions;
using Application.Interfaces;
using Common.Constants;
using Domain.Entities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Persistence.Context;
using Persistence.Extension;

namespace WebApi.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public AuthorizationController(
            IOptions<IdentityOptions> identityOptions,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            ApplicationDbContext context,
            IBackgroundTaskQueueService queueService
        )
        {
            _identityOptions = identityOptions;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        /// <summary>
        /// Authorization
        /// </summary>
        /// <returns>Token</returns>
        /// <exception cref="BadRequestException"></exception>
        [HttpPost("~/connect/token")]
        [Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            var requestObject =
                HttpContext.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException("The request cannot be retrieved.");
            if (requestObject.IsPasswordGrantType())
            {
                requestObject.Username = requestObject.Username?.ToLower().Trim();
                //var user = await _userManager.FindByNameAsync(requestObject.Username);
                var user = await _context.Users.GetByAsync(
                    p => p.UserName == requestObject.Username
                );
                if (user == null)
                {
                    throw new BadRequestException("Please check that your credentials are correct");
                }
                // Validate the username/password parameters and ensure the account is not locked out.
                var result = await _signInManager.CheckPasswordSignInAsync(
                    user,
                    requestObject.Password,
                    true
                );
                // Ensure the user is not already locked out.
                if (result.IsLockedOut)
                {
                    throw new BadRequestException("The specified user account has been suspended");
                }

                // Ensure the user is allowed to sign in.
                if (result.IsNotAllowed)
                {
                    throw new BadRequestException("The specified user is not allowed to sign in");
                }

                if (!result.Succeeded)
                {
                    throw new BadRequestException("Please check that your credentials are correct");
                }

                // Create a new authentication ticket.
                var ticket = CreateTicket(requestObject, user);
                ticket.Principal.SetScopes(requestObject.GetScopes());
                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);

            }
            else if (requestObject.IsRefreshTokenGrantType())
            {
                // Retrieve the claims principal stored in the refresh token.
                var info = await HttpContext.AuthenticateAsync(
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                );

                // Retrieve the user profile corresponding to the refresh token.
                // Note: if you want to automatically invalidate the refresh token
                // when the user password/roles change, use the following line instead:
                // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);

                var userId = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

                User user = await _userManager.FindByIdAsync(userId);

                //User user = await _context.Users.GetByAsync(
                //    p => p.Id == info.Principal.FindFirstValue(CustomClaimTypes.UserId),
                //    p =>
                //        p.Include(pr => pr.UserClaims)
                //            .Include(pr => pr.AdminRole)
                //            .ThenInclude(r => r.AdminRoleClaims)
                //            .Include(pr => pr.CompanyStaff)
                //            .ThenInclude(r => r.CompanyRole.CompanyRoleClaims)
                //);
                if (user == null)
                {
                    throw new BadRequestException("The refresh token is no longer valid");
                }

                // Ensure the user is enabled.
                //if (!user.IsEnabled)
                //{
                //    throw new BadRequestException("The specified user account has been blocked");
                //}

                // Ensure the user is still allowed to sign in.
                if (!await _signInManager.CanSignInAsync(user))
                {
                    throw new BadRequestException("The user is no longer allowed to sign in");
                }
                // Create a new authentication ticket, but reuse the properties stored
                // in the refresh token, including the scopes originally granted.
                var ticket = CreateTicket(requestObject, user);
                _context.Update(user);
                await _context.SaveChangesAsync();
                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            }

            throw new BadRequestException("The specified grant type is not supported");
        }

        //private AuthenticationTicket CreateTicket(OpenIddictRequest request, User user)
        //{

        //    // Create a new ClaimsPrincipal containing the claims that
        //    // will be used to create an id_token, a token or a code.
        //    //            var principal = _signInManager.CreateUserPrincipalAsync(user).Result;
        //    //            var claims = _userManager.GetClaimsAsync(user).Result;
        //    //var roles = _userManager.GetRolesAsync(user).Result;
        //    //var claims = new List<Claim>();
        //    //claims.AddRange(roles.Select(p => new Claim(ClaimTypes.Role, p)).ToArray());
        //    //if (user.UserClaims != null && user.UserClaims.Any())
        //    //{
        //    //    claims.AddRange(user.UserClaims.Select(p => new Claim(p.ClaimType, p.ClaimValue)));
        //    //}

        //    //// Add AdminRole claims if user has custom admin role
        //    //if (
        //    //    user.AdminRole != null
        //    //    && user.AdminRole.IsActive
        //    //    && user.AdminRole.AdminRoleClaims != null
        //    //)
        //    //{
        //    //    claims.AddRange(
        //    //        user.AdminRole.AdminRoleClaims.Select(c => new Claim(c.ClaimType, c.ClaimValue))
        //    //    );
        //    //}

        //    //// Add CompanyRole claims if user has custom company role
        //    //if (
        //    //    user.CompanyStaff != null
        //    //    && user.CompanyStaff.CompanyRole.IsActive
        //    //    && user.CompanyStaff.CompanyRole.CompanyRoleClaims != null
        //    //)
        //    //{
        //    //    claims.AddRange(
        //    //        user.CompanyStaff.CompanyRole.CompanyRoleClaims.Select(c => new Claim(
        //    //            c.ClaimType,
        //    //            c.ClaimValue
        //    //        ))
        //    //    );
        //    //}

        //    //var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Basic"));
        //    var principal = _signInManager.CreateUserPrincipalAsync(user).Result;

        //    var identity = principal.Identity as ClaimsIdentity;

        //    if (identity != null && !string.IsNullOrEmpty(user.Name))
        //    {
        //        //identity.AddClaim(new Claim(ClaimTypes.GivenName, user.Name));
        //        identity.AddClaim(OpenIddictConstants.Claims.Subject, user.Id!);
        //        identity.AddClaim(CustomClaimTypes.UserId, user.Id);
        //        identity.AddClaim(CustomClaimTypes.ChatId, user.ChatId);
        //    }


        //    // Create a new authentication ticket holding the user identity.
        //    var ticket = new AuthenticationTicket(
        //        principal,
        //        new AuthenticationProperties(),
        //        OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
        //    );

        //    ticket.Principal.SetAccessTokenLifetime(TimeSpan.FromDays(1));
        //    ticket.Principal.SetIdentityTokenLifetime(TimeSpan.FromDays(1));
        //    ticket.Principal.SetRefreshTokenLifetime(TimeSpan.FromDays(30));
        //    //if (!request.IsRefreshTokenGrantType())
        //    //{
        //    // Set the list of scopes granted to the client application.
        //    // Note: the offline_access scope must be granted
        //    // to allow OpenIddict to return a refresh token.
        //    ticket.Principal.SetScopes(
        //        new[]
        //        {
        //            OpenIddictConstants.Scopes.OpenId,
        //            //OpenIddictConstants.Scopes.Email,
        //            OpenIddictConstants.Scopes.Phone,
        //            OpenIddictConstants.Scopes.Profile,
        //            OpenIddictConstants.Scopes.OfflineAccess,
        //            OpenIddictConstants.Scopes.Roles,
        //        }.Intersect(request.GetScopes())
        //    );
        //    //}

        //    //ticket.SetResources("quickapp-api");

        //    // Note: by default, claims are NOT automatically included in the access and identity tokens.
        //    // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        //    // whether they should be included in access tokens, in identity tokens or in both.
        //    foreach (var claim in ticket.Principal.Claims)
        //    {
        //        // Never include the security stamp in the access and identity tokens, as it's a secret value.
        //        if (claim.Type == _identityOptions.Value.ClaimsIdentity.SecurityStampClaimType)
        //            continue;

        //        var destinations = new List<string>
        //        {
        //            OpenIddictConstants.Destinations.AccessToken,
        //        };

        //        // Only add the iterated claim to the id_token if the corresponding scope was granted to the client application.
        //        // The other claims will only be added to the access_token, which is encrypted when using the default format.
        //        if (
        //            (
        //                claim.Type == OpenIddictConstants.Claims.Subject
        //                && ticket.Principal.HasScope(OpenIddictConstants.Scopes.OpenId)
        //            )
        //            || (
        //                claim.Type == OpenIddictConstants.Claims.GivenName
        //                && ticket.Principal.HasScope(OpenIddictConstants.Scopes.Profile)
        //            )
        //            || (
        //                claim.Type == CustomClaimTypes.ChatId
        //                && ticket.Principal.HasScope(OpenIddictConstants.Scopes.Profile)
        //            )
        //        )
        //        {
        //            destinations.Add(OpenIddictConstants.Destinations.IdentityToken);
        //        }

        //        claim.SetDestinations(destinations);
        //    }

        //    if (!(principal.Identity is ClaimsIdentity Identity))
        //    {
        //        throw new Exception("Error: Principle is Null");
        //    }
        //    identity.AddClaim(OpenIddictConstants.Claims.Subject, user.ChatId!);
        //    identity.AddClaim(CustomClaimTypes.UserId, user.Id);
        //    identity.AddClaim(CustomClaimTypes.ChatId, user.ChatId);

        //    if (user.UserName != null)
        //    {
        //        identity.AddClaim(
        //            CustomClaimTypes.UserName,
        //            user.UserName,
        //            OpenIddictConstants.Destinations.IdentityToken
        //        );
        //    }

        //    //foreach (var role in roles)
        //    //{
        //    //    if (user.IsEnabled)
        //    //    {
        //    //        identity.AddClaim(OpenIddictConstants.Claims.Role, role);
        //    //    }
        //    //    else
        //    //    {
        //    //        identity.AddClaim(OpenIddictConstants.Claims.Role, role);
        //    //    }
        //    //}

        //    if (ticket.Principal.HasScope(OpenIddictConstants.Scopes.Profile))
        //    {
        //        //                if (!string.IsNullOrWhiteSpace(user.JobTitle))
        //        //                    identity.AddClaim(CustomClaimTypes.JobTitle, user.JobTitle, OpenIddictConstants.Destinations.IdentityToken);
        //        //
        //        if (!string.IsNullOrWhiteSpace(user.Name))
        //        {
        //            identity.AddClaim(CustomClaimTypes.FullName, user.Name);
        //        }
        //        //
        //        //                if (!string.IsNullOrWhiteSpace(user.Configuration))
        //        //                    identity.AddClaim(CustomClaimTypes.Configuration, user.Configuration, OpenIddictConstants.Destinations.IdentityToken);
        //    }

        //    if (ticket.Principal.HasScope(OpenIddictConstants.Scopes.Email))
        //    {
        //        if (!string.IsNullOrWhiteSpace(user.ChatId))
        //            identity.AddClaim(CustomClaimTypes.Email, user.ChatId);
        //    }

        //    //principal.SetDestinations(static claim =>
        //    //    claim.Type switch
        //    //    {
        //    //        // If the "profile" scope was granted, allow the "name" claim to be
        //    //        // added to the access and identity tokens derived from the principal.
        //    //        OpenIddictConstants.Claims.Name
        //    //            when claim.Subject != null
        //    //                && claim.Subject.HasScope(OpenIddictConstants.Scopes.Profile) =>
        //    //        [
        //    //            OpenIddictConstants.Destinations.AccessToken,
        //    //            OpenIddictConstants.Destinations.IdentityToken,
        //    //        ],
        //    //        OpenIddictConstants.Claims.Role =>
        //    //        [
        //    //            OpenIddictConstants.Destinations.AccessToken,
        //    //            OpenIddictConstants.Destinations.IdentityToken,
        //    //        ],
        //    //        OpenIddictConstants.Claims.Subject =>
        //    //        [
        //    //            OpenIddictConstants.Destinations.AccessToken,
        //    //            OpenIddictConstants.Destinations.IdentityToken,
        //    //        ],
        //    //        CustomClaimTypes.FullName => [OpenIddictConstants.Destinations.IdentityToken],
        //    //        CustomClaimTypes.UserId =>
        //    //        [
        //    //            OpenIddictConstants.Destinations.AccessToken,
        //    //            OpenIddictConstants.Destinations.IdentityToken,
        //    //        ],
        //    //        CustomClaimTypes.DepartmentId =>
        //    //        [
        //    //            OpenIddictConstants.Destinations.AccessToken,
        //    //            OpenIddictConstants.Destinations.IdentityToken,
        //    //        ],
        //    //        "secret_value" => [],
        //    //        _ => [OpenIddictConstants.Destinations.AccessToken],
        //    //    }
        //    //);

        //    return ticket;
        //}

        private AuthenticationTicket CreateTicket(OpenIddictRequest request, User user)
        {
            // ... (Your existing code to create the initial 'principal' via _signInManager) ...
            var principal = _signInManager.CreateUserPrincipalAsync(user).Result;
            var identity = principal.Identity as ClaimsIdentity;

            if (identity != null && !string.IsNullOrEmpty(user.Name))
            {
                identity.AddClaim(new Claim(ClaimTypes.GivenName, user.Name));
            }

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(
                principal,
                new AuthenticationProperties(),
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            );

            // ... (lifetime and scope settings are fine) ...

            // --- FIX: Add custom claims BEFORE the destination loop ---
            // We add them here so they are part of ticket.Principal.Claims when the loop runs
            if (identity != null)
            {
                // Change the 'sub' claim to the user ID, not ChatId as previously set
                identity.AddClaim(OpenIddictConstants.Claims.Subject, user.Id!);
                identity.AddClaim(OpenIddictConstants.Claims.Name, user.UserName);
                identity.AddClaim(CustomClaimTypes.UserId, user.Id);
                identity.AddClaim(CustomClaimTypes.ChatId, user.ChatId);
                foreach (var claim in identity.Claims)
                {
                    // Add to Access Token (for APIs) AND Identity Token (for the client/decoded object)
                    claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken,
                                         OpenIddictConstants.Destinations.IdentityToken);
                }
            }
            // --------------------------------------------------------

            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination...

            foreach (var claim in ticket.Principal.Claims)
            {
                // 1. Never include the security stamp
                if (claim.Type == _identityOptions.Value.ClaimsIdentity.SecurityStampClaimType)
                    continue;

                // 2. Default all claims to the Access Token (for the API)
                var destinations = new List<string> { OpenIddictConstants.Destinations.AccessToken };

                // 3. Explicitly allow these specific claims into the ID Token (decoded in frontend)
                if (
                    (claim.Type == OpenIddictConstants.Claims.Subject && ticket.Principal.HasScope(OpenIddictConstants.Scopes.OpenId)) ||
                    (claim.Type == OpenIddictConstants.Claims.Name) ||         // The Username
                    (claim.Type == CustomClaimTypes.ChatId) ||                 // The ChatId
                    (claim.Type == CustomClaimTypes.UserId) ||
                    (claim.Type == OpenIddictConstants.Claims.GivenName && ticket.Principal.HasScope(OpenIddictConstants.Scopes.Profile))
                )
                {
                    destinations.Add(OpenIddictConstants.Destinations.IdentityToken);
                }

                claim.SetDestinations(destinations);
            }

            //foreach (var claim in ticket.Principal.Claims)
            //{
            //    // Never include the security stamp in the access and identity tokens, as it's a secret value.
            //    if (claim.Type == _identityOptions.Value.ClaimsIdentity.SecurityStampClaimType)
            //        continue;

            //    var destinations = new List<string>
            //    {
            //        OpenIddictConstants.Destinations.AccessToken, // Default destination for almost all claims
            //    };

            //    // Only add specific claims to the identity token if the scope was requested
            //    if (
            //        (
            //            claim.Type == OpenIddictConstants.Claims.Subject // Includes your new 'sub' claim
            //            && ticket.Principal.HasScope(OpenIddictConstants.Scopes.OpenId)
            //        )
            //        || (
            //            claim.Type == OpenIddictConstants.Claims.Name
            //            && ticket.Principal.HasScope(OpenIddictConstants.Scopes.Profile)
            //        )
            //        || (
            //            claim.Type == OpenIddictConstants.Claims.GivenName
            //            && ticket.Principal.HasScope(OpenIddictConstants.Scopes.Profile)
            //        )
            //        || (
            //            claim.Type == CustomClaimTypes.ChatId
            //            && ticket.Principal.HasScope(OpenIddictConstants.Scopes.Profile) // Or whatever scope is appropriate
            //        )
            //    )
            //    {
            //        destinations.Add(OpenIddictConstants.Destinations.IdentityToken);
            //    }

            //    claim.SetDestinations(destinations);
            //}

            return ticket;
        }

    }
}
