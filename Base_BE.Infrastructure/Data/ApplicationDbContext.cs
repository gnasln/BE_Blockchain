using Base_BE.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Base_BE.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
  
    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    
}