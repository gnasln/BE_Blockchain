using Base_BE.Application.Common.Interfaces;
using Base_BE.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Base_BE.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
  
    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<UserVote> UserVotes => Set<UserVote>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<BallotVoter> BallotVoters => Set<BallotVoter>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.UserName).IsUnique();
            entity.HasIndex(e => e.CellPhone).IsUnique();
            entity.HasIndex(e => e.IdentityCardNumber).IsUnique();
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasIndex(e => e.PositionName).IsUnique();
        });
        
    }
    
}