using Base_BE.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetHelper.Common.Models;

namespace Base_BE.Application.Position.Commands;

public class DeletePositionCommand : IRequest<ResultCustom<string>>
{
    public required Guid Id { get; set; }
}
public class DeletePositionCommandHandle : IRequestHandler<DeletePositionCommand, ResultCustom<string>>
{
    private readonly IApplicationDbContext _context;

    public DeletePositionCommandHandle(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ResultCustom<string>> Handle(DeletePositionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Positions
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return new ResultCustom<string>
            {
                Status = StatusCode.NOTFOUND,
                Message = new[] { "Position not found" },
                Data = null
            };
        }

        _context.Positions.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new ResultCustom<string> { Status = StatusCode.OK, Message = new[] { "Position deleted successfully" }, Data = null };
    }
}