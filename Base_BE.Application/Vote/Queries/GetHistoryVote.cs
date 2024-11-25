using AutoMapper;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetHelper.Common.Models;


namespace Base_BE.Application.Vote.Queries
{
    public class GetHistoryVoteQueries : IRequest<ResultCustom<List<VotingReponse>>>
    {
        public string UserId { get; set; }
    }

    public class GetHistoryVoteQueriesHandler : IRequestHandler<GetHistoryVoteQueries, ResultCustom<List<VotingReponse>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetHistoryVoteQueriesHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ResultCustom<List<VotingReponse>>> Handle(GetHistoryVoteQueries request, CancellationToken cancellationToken)
        {
            try
            {
                var entities = await (from vote in _context.Votes
                                      join userVote in _context.UserVotes on vote.Id equals userVote.VoteId into voteGroup
                                      from userVote in voteGroup.DefaultIfEmpty()
                                      where userVote.UserId == request.UserId && (userVote.Role == "Voter" || userVote.Role == "Candidate")
                                      select vote)
                    .ToListAsync(cancellationToken);

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