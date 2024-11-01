
using Microsoft.EntityFrameworkCore;

namespace Base_BE.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        DbSet<Domain.Entities.Position> Positions { get; }
    }
}
