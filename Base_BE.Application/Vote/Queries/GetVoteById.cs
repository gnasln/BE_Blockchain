using AutoMapper;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetHelper.Common.Models;

namespace Base_BE.Application.Vote.Queries;
public class GetVoteByIdQueries : IRequest<ResultCustom<VotingReponse>>
{
    public Guid Id { get; set; }
}

public class GetVoteByIdQueriesHandler : IRequestHandler<GetVoteByIdQueries, ResultCustom<VotingReponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper; 

    public GetVoteByIdQueriesHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ResultCustom<VotingReponse>> Handle(GetVoteByIdQueries request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _context.Votes.FindAsync(request.Id);
            if (entity == null)
            {
                return new ResultCustom<VotingReponse>
                {
                    Status = StatusCode.NOTFOUND,
                    Message = new[] { "Vote not found" }
                };
            }
            var result = _mapper.Map<VotingReponse>(entity);
            var userCandidates = await _context.UserVotes.Where(x => x.VoteId == request.Id && x.Role == "Candidate").ToListAsync();
            var userVoters = await _context.UserVotes.Where(x => x.VoteId == request.Id && x.Role == "Voter").ToListAsync();
            
            result.Candidates = userCandidates.Select(x => x.UserId).ToList();
            result.Voters = userVoters.Select(x => x.UserId).ToList();

            return new ResultCustom<VotingReponse>
            {
                Status = StatusCode.OK,
                Message = new[] { "Get Vote by Id successfully" },
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ResultCustom<VotingReponse>
            {
                Status = StatusCode.INTERNALSERVERERROR,
                Message = new[] { ex.Message }
            };
        }
    }
}