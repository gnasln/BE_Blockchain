using AutoMapper;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Application.Dtos;
using Base_BE.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetHelper.Common.Models;
using Nethereum.Contracts.QueryHandlers.MultiCall;

namespace Base_BE.Application.Vote.Commands;

public class CreateBallotVoterCommand : IRequest<ResultCustom<BallotVoterDto>>
{
    public List<Guid> CandidateIds { get; set; }
    public Guid VoterId { get; set; }
    public DateTime VotedTime { get; set; } = DateTime.Now;
    public string? Address { get; set; }
    public required Guid VoteId { get; set; }
    public string? BallotTransaction { get; set; }
}

public class CreateBallotVoterCommandHandler : IRequestHandler<CreateBallotVoterCommand, ResultCustom<BallotVoterDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IMapper _mapper;

    public CreateBallotVoterCommandHandler(IApplicationDbContext context, IUser user, IMapper mapper)
    {
        _context = context;
        _user = user;
        _mapper = mapper;
    }
    public async Task<ResultCustom<BallotVoterDto>> Handle(CreateBallotVoterCommand request, CancellationToken cancellationToken)
    {
        var vote = await _context.Votes.FirstOrDefaultAsync(v => v.Id == request.VoteId, cancellationToken);
        if (vote == null) 
        {
            return new ResultCustom<BallotVoterDto>()
            {
                Status = StatusCode.NOTFOUND,
                Message = new[] { "not found" },
                Data = null
            };
        }
        foreach (var candidateId in request.CandidateIds)
        {
            var ballotVoter = new BallotVoter
            {
                CandidateId = candidateId,
                VoterId = request.VoterId,
                VotedTime = request.VotedTime,
                Address = request.Address
            };
            await _context.BallotVoters.AddAsync(ballotVoter, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Cập nhật UserVote
        var userVote = await _context.UserVotes
            .FirstOrDefaultAsync(x => x.VoteId == request.VoteId && x.UserId == _user.Id, cancellationToken);
        if (userVote == null)
        {
            return new ResultCustom<BallotVoterDto>
            {
                Status = StatusCode.NOTFOUND,
                Message = new[] { "UserVote not found" }
            };
        }
        userVote.Status = true;
        userVote.BallotTransaction = request.BallotTransaction;
        userVote.BallotAddress = request.Address;
        _context.UserVotes.Update(userVote);
        await _context.SaveChangesAsync(cancellationToken);

        var resultDto = new BallotVoterDto
        {
            VoterId = vote.Id,
            CandidateIds = request.CandidateIds,
            Address = request.Address,
            VotedTime = request.VotedTime
        };

        return new ResultCustom<BallotVoterDto>
        {
            Status = StatusCode.OK,
            Message = new[] { "Ballot voter created successfully" },
            Data = resultDto
        };
    }
}