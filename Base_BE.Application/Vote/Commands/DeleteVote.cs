using Base_BE.Application.Common.Interfaces;
using MediatR;
using NetHelper.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base_BE.Application.Vote.Commands
{
    public class DeleteVoteCommand : IRequest<ResultCustom<string>>
    {
        public required Guid Id { get; set; }
    }

    public class DeleteVoteCommandHandler : IRequestHandler<DeleteVoteCommand, ResultCustom<string>>
    {
        private readonly IApplicationDbContext _context;

        public DeleteVoteCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ResultCustom<string>> Handle(DeleteVoteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _context.Votes.FindAsync(request.Id, cancellationToken);
                if (entity is null)
                    return new ResultCustom<string>
                    {
                        Status = StatusCode.NOTFOUND,
                        Message = new[] { "This Vote doesn't exist " }
                    };

                _context.Votes.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);

                return new ResultCustom<string>
                {
                    Status = StatusCode.OK,
                    Message = new[] { "Delete Vote successfully" },
                    Data = entity.Id.ToString()
                };
            }
            catch (Exception ex)
            {
                return new ResultCustom<string>
                {
                    Status = StatusCode.INTERNALSERVERERROR,
                    Message = new[] { ex.Message }
                };
            }
        }
    }
}
