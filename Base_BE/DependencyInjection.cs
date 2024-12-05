using Base_BE.Infrastructure.Data;
using Base_BE.Services;
using Base_BE.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.UI.Services;
using Base_BE.Dtos;
using System.Configuration;
using Base_BE.Helper.Services;
using Base_BE.Domain.Entities;
using Base_BE.Helper;
using Microsoft.AspNetCore.Identity;
using ConfigurationManager = Microsoft.Extensions.Configuration.ConfigurationManager;
using IEmailSender = Base_BE.Helper.Services.IEmailSender;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        services.AddDatabaseDeveloperPageExceptionFilter();
        services.AddScoped<IUser, CurrentUser>();
        services.AddHttpContextAccessor();
        
        services.AddControllers();
        services.AddMemoryCache(); // Thêm dòng này để sử dụng MemoryCache
        services.AddSingleton<OTPService>();
        services.AddScoped<IEmailSender, EmailSender>(); // Giả định bạn có một implementation của IEmailSender
        // Register the BackgroundTaskQueue service
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<QueuedHostedService>();
        //ket noi hop dong thong minh
        services.AddSingleton<SmartContractService>();

        //vo hieu hoa
        services.AddScoped<SignInManager<ApplicationUser>, CustomSignInManager>();


        // Add FluentEmail with configuration settings
   //      services
   //          .AddFluentEmail("lengocsang2k4@gmail.com")
			// .AddRazorRenderer()
			// .AddSmtpSender("smtp.gmail.com", 587, "lengocsang2k4@gmail.com", "wmak huen cqwi puei");


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

    public static void AddFluentEmail(this IServiceCollection services, ConfigurationManager configuration)
    {
        var emailSettings = configuration.GetSection("EmailSettings");
        var defaultFromEmail = emailSettings["FromAddress"];
        var host = emailSettings["Host"];
        var port = emailSettings.GetValue<int>("Port");
        
        services.AddFluentEmail(defaultFromEmail)
            .AddRazorRenderer()
            .AddSmtpSender(host, port, defaultFromEmail, emailSettings["Password"]);
    }
}
