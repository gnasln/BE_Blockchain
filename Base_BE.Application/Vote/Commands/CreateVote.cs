using Base_BE.Application.Dtos;
using MediatR;
using AutoMapper;
using Base_BE.Application.Common.Interfaces;
using NetHelper.Common.Models;

namespace Base_BE.Application.Vote.Commands
{
    public class CreateVoteCommand : IRequest<ResultCustom<VotingReponse>>
    {
        public required string VoteName { get; set; }
        public int MaxCandidateVote { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public Guid PositionId { get; set; }
        public string? Status { get; set; }
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
                CreateMap<CreateVoteCommand, Domain.Entities.Vote>();
                CreateMap<Domain.Entities.Vote, VotingReponse>();
            }
        }
    }
    
    public class CreateVoteCommandHanler : IRequestHandler<CreateVoteCommand, ResultCustom<VotingReponse>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        
        public CreateVoteCommandHanler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ResultCustom<VotingReponse>> Handle(CreateVoteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var entity = _mapper.Map<Domain.Entities.Vote>(request);
                await _context.Votes.AddAsync(entity, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                
                //add candidates
                if (request.Candidates != null && request.Candidates.Count > 0)
                {
                    foreach (var candidate in request.Candidates)
                    {
                        var userVote = new Domain.Entities.UserVote
                        {
                            VoteId = entity.Id,
                            UserId = candidate,
                            CreatedDate = DateTime.Now,
                            Role = "Candidate",
                            BallotTransaction = "",
                            BallotAddress = "",
                            Status = false
                        };
                        await _context.UserVotes.AddAsync(userVote, cancellationToken);
                    }
                    await _context.SaveChangesAsync(cancellationToken);
                }
                
                //add voters
                if (request.Voters != null && request.Voters.Count > 0)
                {
                    foreach (var voter in request.Voters)
                    {
                        var userVote = new Domain.Entities.UserVote
                        {
                            VoteId = entity.Id,
                            UserId = voter,
                            CreatedDate = DateTime.Now,
                            Role = "Voter",
                            BallotTransaction = "",
                            BallotAddress = "",
                            Status = false
                        };
                        await _context.UserVotes.AddAsync(userVote, cancellationToken);
                    }
                    await _context.SaveChangesAsync(cancellationToken);
                }
                

                var result = new VotingReponse()
                {
                    VoteName = entity.VoteName,
                    MaxCandidateVote = entity.MaxCandidateVote,
                    CreateDate = entity.CreateDate,
                    StartDate = entity.StartDate,
                    ExpiredDate = entity.ExpiredDate,
                    PositionId = entity.PositionId,
                    Status = entity.Status,
                    Tenure = entity.Tenure,
                    StartDateTenure = entity.StartDateTenure,
                    EndDateTenure = entity.EndDateTenure,
                    ExtraData = entity.ExtraData,
                    Candidates = request.Candidates.ToList(),
                    CandidateNames = request.CandidateNames.ToList(),
                    Voters = request.Voters.ToList(),
                    VoterNames = request.VoterNames.ToList()
                };

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
