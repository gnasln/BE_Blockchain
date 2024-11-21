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
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime StartDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public required Guid PositionId { get; set; }
        public string Status { get; set; }
        public string Tenure { get; set; }
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
                // Tìm bản ghi Vote
                var entity = await _context.Votes.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
                if (entity is null)
                    return new ResultCustom<VotingReponse>
                    {
                        Status = StatusCode.NOTFOUND,
                        Message = new[] { "This Vote doesn't exist." }
                    };

                // Xác thực dữ liệu đầu vào
                if (request.ExpiredDate <= request.StartDate)
                    return new ResultCustom<VotingReponse>
                    {
                        Status = StatusCode.BADREQUEST,
                        Message = new[] { "ExpiredDate must be later than StartDate." }
                    };

                // Cập nhật thông tin cơ bản của Vote
                UpdateVoteEntity(request, entity);

                // Lưu bản ghi chính Vote
                _context.Votes.Update(entity);
                await _context.SaveChangesAsync(cancellationToken);

                // Cập nhật danh sách Candidates và Voters
                await UpdateCandidatesAsync(entity.Id, request.Candidates, cancellationToken);
                await UpdateVotersAsync(entity.Id, request.Voters, cancellationToken);

                // Map kết quả sang VotingResponse
                var result = _mapper.Map<VotingReponse>(entity);
                result.Candidates = request.Candidates?.ToList() ?? new List<string>();
                result.CandidateNames = request.CandidateNames?.ToList() ?? new List<string>();
                result.Voters = request.Voters?.ToList() ?? new List<string>();
                result.VoterNames = request.VoterNames?.ToList() ?? new List<string>();

                return new ResultCustom<VotingReponse>()
                {
                    Status = StatusCode.OK,
                    Message = new[] { "Vote updated successfully." },
                    Data = result
                };
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                return new ResultCustom<VotingReponse>()
                {
                    Status = StatusCode.INTERNALSERVERERROR,
                    Message = new[] { "An error occurred while updating the vote.", ex.Message }
                };
            }
        }

        private void UpdateVoteEntity(UpdateVoteCommand request, Domain.Entities.Vote entity)
        {
            if (!string.IsNullOrEmpty(request.VoteName)) entity.VoteName = request.VoteName;
            if (request.MaxCandidateVote > 0) entity.MaxCandidateVote = request.MaxCandidateVote;
            if (request.StartDate != default) entity.StartDate = request.StartDate;
            if (request.ExpiredDate != default) entity.ExpiredDate = request.ExpiredDate;
            if (request.PositionId != Guid.Empty) entity.PositionId = request.PositionId;
            if (!string.IsNullOrEmpty(request.Status)) entity.Status = request.Status;
            if (!string.IsNullOrEmpty(request.Tenure)) entity.Tenure = request.Tenure;
            if (request.StartDateTenure != default) entity.StartDateTenure = request.StartDateTenure;
            if (request.EndDateTenure != default) entity.EndDateTenure = request.EndDateTenure;
            if (!string.IsNullOrEmpty(request.ExtraData)) entity.ExtraData = request.ExtraData;
        }

        private async Task UpdateCandidatesAsync(Guid voteId, List<string>? candidates, CancellationToken cancellationToken)
        {
            if (candidates == null || !candidates.Any()) return;

            // Xóa các bản ghi cũ
            var existingCandidates = await _context.UserVotes
                .Where(x => x.VoteId == voteId && x.Role == "Candidate")
                .ToListAsync(cancellationToken);

            _context.UserVotes.RemoveRange(existingCandidates);

            // Thêm các bản ghi mới
            var newCandidates = candidates.Select(candidateId => new Domain.Entities.UserVote
            {
                VoteId = voteId,
                UserId = candidateId,
                Role = "Candidate",
                CreatedDate = DateTime.Now,
                BallotTransaction = "",
                BallotAddress = "",
                Status = false
            }).ToList();

            await _context.UserVotes.AddRangeAsync(newCandidates, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task UpdateVotersAsync(Guid voteId, List<string>? voters, CancellationToken cancellationToken)
        {
            if (voters == null || !voters.Any()) return;

            // Xóa các bản ghi cũ
            var existingVoters = await _context.UserVotes
                .Where(x => x.VoteId == voteId && x.Role == "Voter")
                .ToListAsync(cancellationToken);

            _context.UserVotes.RemoveRange(existingVoters);

            // Thêm các bản ghi mới
            var newVoters = voters.Select(voterId => new Domain.Entities.UserVote
            {
                VoteId = voteId,
                UserId = voterId,
                Role = "Voter",
                CreatedDate = DateTime.Now,
                BallotTransaction = "",
                BallotAddress = "",
                Status = false
            }).ToList();

            await _context.UserVotes.AddRangeAsync(newVoters, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
