using Base_BE.Domain.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using System.Globalization;
using static OpenIddict.Abstractions.OpenIddictConstants;


namespace Base_BE.Infrastructure.Data
{
    public static class InitializerExtensions
    {
        public static async Task InitializeDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();

            await initialiser.InitializeAsync();

            await initialiser.SeedAsync();
        }
    }

    public class ApplicationDbContextInitializer
    {
        private readonly ILogger<ApplicationDbContextInitializer> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IOpenIddictApplicationManager _manager;
        private readonly IOpenIddictScopeManager _managerScope;

        public ApplicationDbContextInitializer(ILogger<ApplicationDbContextInitializer> logger, ApplicationDbContext context
            , UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager
            , IOpenIddictApplicationManager manager
            , IOpenIddictScopeManager managerScope)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _manager = manager;
            _managerScope = managerScope;
        }

        public async Task InitializeAsync()
        {
            try
            {
                await _context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initialising the database.");
                throw;
            }
        }

        public async Task SeedAsync()
        {
            try
            {
                await TrySeedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        public async Task TrySeedAsync()
        {
            #region Init Administrator
            // Default roles
            var administratorRole = new IdentityRole(Roles.Administrator);
            var user = new IdentityRole(Roles.User);

            if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
            {
                await _roleManager.CreateAsync(administratorRole);
            }
            if (_roleManager.Roles.All(r => r.Name != user.Name))
            {
                await _roleManager.CreateAsync(user);
            }

            // Default users
            var administrator = new ApplicationUser { UserName = "administrator", Email = "administrator@localhost" };

            if (_userManager.Users.All(u => u.UserName != administrator.UserName))
            {
                await _userManager.CreateAsync(administrator, "Administrator1!");
                if (!string.IsNullOrWhiteSpace(administratorRole.Name))
                {
                    _ = await _userManager.AddToRolesAsync(administrator, [administratorRole.Name]);
                }
            }
            #endregion

            #region Init Client
            // Resource Server
            if (await _manager.FindByClientIdAsync("resource_svr_1") == null)
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = "resource_svr_1",
                    ClientSecret = "835B62E0-4215-A99D-DEF9-86E6B8DAB342",
                    RedirectUris =
                    {
                        new Uri("https://localhost:5051")
                    },
                    Permissions =
                    {
                        Permissions.Endpoints.Introspection
                    },
                    ApplicationType = ApplicationTypes.Native
                };

                _ = await _manager.CreateAsync(descriptor);
            }

            // Web Flow
            if (await _manager.FindByClientIdAsync("aw") is null)
            {
                _ = await _manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    DisplayName = "Web authorization code flow with PKCE client",
                    ClientId = "aw",
                    ClientSecret = "$,Zf.EXS@quyb}DYC8{@i]P{!?*J=k",
                    RedirectUris =
                    {
                        new Uri("http://localhost:5051/index.html"),
                        new Uri("http://localhost:5051/signin-callback.html"),
                        new Uri("http://localhost:5051/signin-silent-callback.html"),
                        },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Logout,
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Permissions.Prefixes.Scope + "openid",
                        Permissions.Prefixes.Scope + "api"
                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }

            // App Flow
            if (await _manager.FindByClientIdAsync("aa") is null)
            {
                _ = await _manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    DisplayName = "App authorization code flow with PKCE client",
                    ClientId = "aa",
                    ClientSecret = "$,Zf.EXS@quyb}DYC8{@i]P{!?*J=k",
                    RedirectUris =
                    {
                        //new Uri("ez.active.app"),
                        //new Uri("ez.active.app/signin-callback.html")
                    },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Logout,
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Permissions.Prefixes.Scope + "openid",
                        Permissions.Prefixes.Scope + "api"
                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }

            // Password flow
            if (await _manager.FindByClientIdAsync("pm") is null)
            {
                _ = await _manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "pm",
                    DisplayName = "Password & Authorization with Code flow client",
                    RedirectUris =
                    {
                        new Uri("https://oauth.pstmn.io/v1/callback")
                    },
                    ClientSecret = "$,Zf.EXS@quyb}DYC8{@i]P{!?*J=k",
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Logout,
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.Password,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,
                        Permissions.ResponseTypes.Token,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Permissions.Prefixes.Scope + "openid",
                        Permissions.Prefixes.Scope + "api"
                    }
                });
            }
            #endregion

            #region Init Scope
            if (await _managerScope.FindByNameAsync("api") is null)
            {
                await _managerScope.CreateAsync(new OpenIddictScopeDescriptor
                {
                    DisplayName = "BE_API access",
                    DisplayNames =
                    {
                        [CultureInfo.GetCultureInfo("vi-VN")] = "BE_API."
                    },
                    Name = "api",
                    Resources =
                    {
                        "resource_a"
                    }
                });
            }
            #endregion
        }
    }
}
