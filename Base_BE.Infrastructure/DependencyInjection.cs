using Base_BE.Infrastructure.Data.Interceptors;
using Base_BE.Infrastructure.Data;
using Base_BE.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Ardalis.GuardClauses;
using Base_BE.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("e_voting");

        Guard.Against.Null(connectionString, message: "Connection string 'e_voting' not found.");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseMySQL(connectionString);
            options.UseOpenIddict();
        });


		services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        // Register the Identity services.
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<ApplicationDbContextInitializer>();

        // OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
        // (like pruning orphaned authorizations/tokens from the database) at regular intervals.
        services.AddQuartz(options =>
        {
            options.UseMicrosoftDependencyInjectionJobFactory();
            options.UseSimpleTypeLoader();
            options.UseInMemoryStore();
        });

        services.AddSingleton(TimeProvider.System);

        // Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        services.AddOpenIddict()

            // Register the OpenIddict core components.
            .AddCore(options =>
            {
                // Configure OpenIddict to use the Entity Framework Core stores and models.
                // Note: call ReplaceDefaultEntities() to replace the default OpenIddict entities.
                options.UseEntityFrameworkCore()
                       .UseDbContext<ApplicationDbContext>();

                // Enable Quartz.NET integration.
                options.UseQuartz();
            })

            // Register the OpenIddict server components.
            .AddServer(options =>
            {
                // Enable the authorization, logout, token and userinfo endpoints.
                options.SetAuthorizationEndpointUris("connect/authorize")
                       .SetLogoutEndpointUris("connect/logout")
                       .SetIntrospectionEndpointUris("connect/introspect")
                       .SetTokenEndpointUris("connect/token")
                       .SetUserinfoEndpointUris("connect/userinfo")
                       .SetVerificationEndpointUris("connect/verify");

                // Mark the "openId", "email", "profile" and "roles" scopes as supported scopes.
                options.RegisterScopes(Scopes.OpenId, Scopes.Email, Scopes.Profile, Scopes.Roles);

                options
                       .AllowAuthorizationCodeFlow()
                       .AllowPasswordFlow()
                       .AllowImplicitFlow()
                       .AllowRefreshTokenFlow()
                       .AllowClientCredentialsFlow()
                       ;

                // Register the signing and encryption credentials.
                options
                       //.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate()
                       .AddEncryptionKey(new SymmetricSecurityKey(Convert.FromBase64String("YWN0aXZlX3NpZ24ga2V5LiBLZWVwIGl0IHNlY3JldCE=")))
                       //.DisableAccessTokenEncryption()
                       ;

                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                options.UseAspNetCore()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableLogoutEndpointPassthrough()
                       .EnableTokenEndpointPassthrough()
                       .EnableUserinfoEndpointPassthrough()
                       .EnableStatusCodePagesIntegration()
                       .DisableTransportSecurityRequirement()
                       ;
                options.UseReferenceAccessTokens().UseReferenceRefreshTokens();

                options.UseDataProtection()
                       .PreferDefaultAccessTokenFormat()
                       .PreferDefaultAuthorizationCodeFormat()
                       .PreferDefaultDeviceCodeFormat()
                       .PreferDefaultRefreshTokenFormat()
                       .PreferDefaultUserCodeFormat()
                       ;
            })

            // Register the OpenIddict validation components.
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance.
                options.UseLocalServer();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();

                options.UseDataProtection();
                // options.EnableTokenEntryValidation
            })
            ;

        return services;
    }
}
