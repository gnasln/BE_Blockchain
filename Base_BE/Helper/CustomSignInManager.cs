using Base_BE.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Base_BE.Helper;
public class CustomSignInManager : SignInManager<ApplicationUser>
{
    public CustomSignInManager(UserManager<ApplicationUser> userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<ApplicationUser>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<ApplicationUser> confirmation)
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation) { }

    public override async Task<bool> CanSignInAsync(ApplicationUser user)
    {
        if (user.Status?.ToLower() == "disable")
        {
            Logger.LogWarning("User {UserId} attempted to sign in but the account is disabled.", user.Id);
            return false;
        }

        return await base.CanSignInAsync(user);
    }
}
