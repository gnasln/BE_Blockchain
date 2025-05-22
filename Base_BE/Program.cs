using System.Configuration;
using Base_BE.Domain.Entities;
using System.Security.Claims;
using Base_BE.Infrastructure.Data;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Formatting.Json;
using Newtonsoft.Json;

// log
var logger = Log.Logger = new LoggerConfiguration()
  .Enrich.FromLogContext()
  .WriteTo.Console()
  .CreateLogger();

logger.Information("Starting web host");

var builder = WebApplication.CreateBuilder(args);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
// Add services to the container

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices();
builder.Services.AddControllers();

#region Add module / microservice
// TodoModule:
#endregion

// log
builder.Host.UseSerilog((ctx, config) =>
{
    config.WriteTo.Console().MinimumLevel.Information();
    config.WriteTo.File(
      path: AppDomain.CurrentDomain.BaseDirectory + "/logs/log-.txt",
      rollingInterval: RollingInterval.Day,
      rollOnFileSizeLimit: true,
      formatter: new JsonFormatter()).MinimumLevel.Information();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT token into authorization header using the Bearer scheme. Example: Bearer {token}",
        Name = "Authorization",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            { new OpenApiSecurityScheme
                { Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme } }
            , new List<string>() }
        });

    options.SwaggerDoc("v1", new OpenApiInfo { Title = "API root", Version = "v1" });
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});
builder.Services.AddFluentEmail(builder.Configuration);


var app = builder.Build();

// use log
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto
});
app.UseCertificateForwarding();
app.UseHsts();
if (app.Environment.IsDevelopment() || app.Configuration["EnableDumpEnv"] == "1")
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // await app.InitializeDatabaseAsync();
}


app.UseHealthChecks("/healthz");

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseAuthentication();
app.UseMiddleware<AccountDisabledMiddleware>();
app.UseAuthorization();

app.UseStaticFiles();
app.MapControllers();
app.MapEndpoints();

await app.RunAsync();

public class AccountDisabledMiddleware
{
    private readonly RequestDelegate _next;

    public AccountDisabledMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            try
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("User is not authenticated or ID is missing.");
                    return;
                }

                var user = await userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("User not found.");
                    return;
                }

                if (user.Status?.ToLower() == "disable")
                {
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                    {
                        status = StatusCodes.Status403Forbidden,
                        message = "Your account has been disabled."
                    }));
                    return;
                }
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                {
                    status = StatusCodes.Status500InternalServerError,
                    message = $"An error occurred: {ex.Message}"
                }));
                return;
            }
        }

        await _next(context);
    }
}


