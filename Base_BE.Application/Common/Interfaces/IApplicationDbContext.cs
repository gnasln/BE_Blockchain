
using Base_BE.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Base_BE.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        DbSet<Domain.Entities.Position> Positions { get; }
        DbSet<Domain.Entities.Vote> Votes { get; }
        DbSet<Domain.Entities.UserVote> UserVotes { get; }
        DbSet<Domain.Entities.Notification> Notifications { get; }
        DbSet<Domain.Entities.BallotVoter> BallotVoters { get; }
        DbSet<ApplicationUser> ApplicationUsers { get; }
    }
}
