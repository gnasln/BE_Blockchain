using AutoMapper;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Application.Dtos;
using Base_BE.Application.Position.Queries;
using Base_BE.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
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
    private readonly UserManager<ApplicationUser> _user;

    public GetVoteByIdQueriesHandler(IApplicationDbContext context, IMapper mapper, UserManager<ApplicationUser> user)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
    }

    public async Task<ResultCustom<VotingReponse>> Handle(GetVoteByIdQueries request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _context.Votes.FindAsync(request.Id, cancellationToken);
            if (entity == null)
            {
                return new ResultCustom<VotingReponse>
                {
                    Status = StatusCode.NOTFOUND,
                    Message = new[] { "Vote not found" }
                };
            }


            var result = _mapper.Map<VotingReponse>(entity);
            // Lấy danh sách các UserId của Candidates
            var candidateIds = await _context.UserVotes
                .Where(x => x.VoteId == request.Id && x.Role == "Candidate")
                .Select(x => x.UserId)
                .ToListAsync();

            // Lấy danh sách tên của Candidates
            var candidateNames = new List<string>();
            foreach (var candidateId in candidateIds)
            {
                var candidate = await _user.FindByIdAsync(candidateId);
                if (candidate != null)
                {
                    candidateNames.Add(candidate.FullName);
                }
            }

            // Lấy danh sách các UserId của Voters
            var voterIds = await _context.UserVotes
                .Where(x => x.VoteId == request.Id && x.Role == "Voter")
                .Select(x => x.UserId)
                .ToListAsync();

            // Lấy danh sách tên của Voters
            var voterNames = new List<string>();
            foreach (var voterId in voterIds)
            {
                var voter = await _user.FindByIdAsync(voterId);
                if (voter != null)
                {
                    voterNames.Add(voter.FullName);
                }
            }

            result.PositionName = (await _context.Positions.FindAsync(result.PositionId, cancellationToken)).PositionName;
            result.Candidates = candidateIds;
            result.CandidateNames = candidateNames;
            result.Voters = voterIds;
            result.VoterNames = voterNames;
            result.TotalVoter = voterIds.Count;
            result.TotalCandidate = candidateIds.Count;

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