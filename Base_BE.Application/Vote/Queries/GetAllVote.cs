using AutoMapper;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetHelper.Common.Models;


namespace Base_BE.Application.Vote.Queries
{
    public class GetAllVoteQueries : IRequest<ResultCustom<List<VotingReponse>>>
    {
    }

    public class GetAllVoteQueriesHandler : IRequestHandler<GetAllVoteQueries, ResultCustom<List<VotingReponse>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetAllVoteQueriesHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ResultCustom<List<VotingReponse>>> Handle(GetAllVoteQueries request, CancellationToken cancellationToken)
        {
            try
            {
                var entities = await _context.Votes.ToListAsync(cancellationToken);
                var result = _mapper.Map<List<VotingReponse>>(entities);
                return new ResultCustom<List<VotingReponse>>
                {
                    Status = StatusCode.OK,
                    Message = new[] { "Get all Vote successfully" },
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ResultCustom<List<VotingReponse>>
                {
                    Status = StatusCode.INTERNALSERVERERROR,
                    Message = new[] { ex.Message }
                };
            }
        }
    }
}
