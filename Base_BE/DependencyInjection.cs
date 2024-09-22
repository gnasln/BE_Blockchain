using Base_BE.Infrastructure.Data;
using Base_BE.Services;
using Base_BE.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        services.AddDatabaseDeveloperPageExceptionFilter();
        services.AddScoped<IUser, CurrentUser>();
        services.AddHttpContextAccessor();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        services.AddExceptionHandler<CustomExceptionHandler>();
        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
                .Build())
            .AddPolicy("OpenIddict.Server.AspNetCore", policy =>
            {
                policy.AuthenticationSchemes.Add(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
            })
            .AddPolicy("admin", policy =>
            {
                policy.AuthenticationSchemes = [OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme];
                policy.RequireAuthenticatedUser();
                policy.RequireRole("Administrator");
            });


        return services;
    }
}
