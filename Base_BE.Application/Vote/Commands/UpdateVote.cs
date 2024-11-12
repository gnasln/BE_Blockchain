using AutoMapper;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetHelper.Common.Models;


namespace Base_BE.Application.Vote.Commands
{
    public class UpdateVoteCommand : IRequest<ResultCustom<VotingReponse>>
    {
        public required Guid Id { get; set; }
        public required string VoteName { get; set; }
        public int MaxCandidateVote { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public Guid PositionId { get; set; }
        public string Status { get; set; }
        public required string Tenure { get; set; }
        public DateTime StartDateTenure { get; set; }
        public DateTime EndDateTenure { get; set; }
        public string? ExtraData { get; set; }
        public List<string>? Candidates { get; set; }
        public List<string>? CandidateNames { get; set; }
        public List<string>? Voters { get; set; }
        public List<string>? VoterNames { get; set; }

        public class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<UpdateVoteCommand, Domain.Entities.Vote>();
                CreateMap<Domain.Entities.Vote, VotingReponse>();
            }
        }
    }

    public class UpdateVoteCommandHandler : IRequestHandler<UpdateVoteCommand, ResultCustom<VotingReponse>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UpdateVoteCommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ResultCustom<VotingReponse>> Handle(UpdateVoteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _context.Votes.FindAsync(request.Id, cancellationToken);
                if (entity is null)
                    return new ResultCustom<VotingReponse>
                    {
                        Status = StatusCode.NOTFOUND,
                        Message = new[] { "This Vote doesn't exist " }
                    };

                // Gán các thuộc tính từ request vào entity
                if (!string.IsNullOrEmpty(request.VoteName)) entity.VoteName = request.VoteName;
                if (request.MaxCandidateVote > 0) entity.MaxCandidateVote = request.MaxCandidateVote;
                if (request.StartDate != default) entity.StartDate = request.StartDate;
                if (request.ExpiredDate != default) entity.ExpiredDate = request.ExpiredDate;
                if (request.PositionId != Guid.Empty) entity.PositionId = request.PositionId;
                if (!string.IsNullOrEmpty(request.Status)) entity.Status = request.Status;
                if (!string.IsNullOrEmpty(request.Tenure))entity.Tenure = request.Tenure;
                if (request.StartDateTenure != default) entity.StartDateTenure = request.StartDateTenure;
                if (request.EndDateTenure != default) entity.EndDateTenure = request.EndDateTenure;
                if (!string.IsNullOrEmpty(request.ExtraData)) entity.ExtraData = request.ExtraData;

                // Cập nhật danh sách Candidates
                if (request.Candidates != null && request.Candidates.Count > 0)
                {
                    var existingCandidates = await _context.UserVotes
                        .Where(x => x.VoteId == entity.Id && x.Role == "Candidate")
                        .ToListAsync(cancellationToken);

                    _context.UserVotes.RemoveRange(existingCandidates);

                    var newCandidates = request.Candidates.Select(candidateId => new Domain.Entities.UserVote
                    {
                        VoteId = entity.Id,
                        UserId = candidateId,
                        Role = "Candidate"
                    }).ToList();

                    await _context.UserVotes.AddRangeAsync(newCandidates, cancellationToken);
                }

                // Cập nhật danh sách Voters
                if (request.Voters != null && request.Voters.Count > 0)
                {
                    var existingVoters = await _context.UserVotes
                        .Where(x => x.VoteId == entity.Id && x.Role == "Voter")
                        .ToListAsync(cancellationToken);

                    _context.UserVotes.RemoveRange(existingVoters);

                    var newVoters = request.Voters.Select(voterId => new Domain.Entities.UserVote
                    {
                        VoteId = entity.Id,
                        UserId = voterId,
                        Role = "Voter"
                    }).ToList();

                    await _context.UserVotes.AddRangeAsync(newVoters, cancellationToken);
                }

                await _context.SaveChangesAsync(cancellationToken);

                var result = _mapper.Map<VotingReponse>(entity);
                result.Candidates = request.Candidates.ToList();
                result.CandidateNames = request.CandidateNames.ToList();
                result.Voters = request.Voters.ToList();
                result.VoterNames = request.VoterNames.ToList();

                return new ResultCustom<VotingReponse>()
                {
                    Status = StatusCode.CREATED,
                    Message = new[] { "Vote created successfully" },
                    Data = result
                };

            }
            catch (Exception e)
            {
                return new ResultCustom<VotingReponse>()
                {
                    Status = StatusCode.INTERNALSERVERERROR,
                    Message = new[] { e.Message }
                };
            }
        }
    }
}
