using AutoMapper;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetHelper.Common.Models;


namespace Base_BE.Application.Vote.Queries
{
    public class GetAllVoteForCandidateQueries : IRequest<ResultCustom<List<VotingReponse>>>
    {
        public string UserId { get; set; }
        public string? VoteName { get; set; }
        public string? Status { get; set; }
    }

    public class GetAllVoteForCandidateQueriesHandler : IRequestHandler<GetAllVoteForCandidateQueries, ResultCustom<List<VotingReponse>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetAllVoteForCandidateQueriesHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ResultCustom<List<VotingReponse>>> Handle(GetAllVoteForCandidateQueries request, CancellationToken cancellationToken)
        {
            try
            {
                // Tạo truy vấn ban đầu
                var query = from vote in _context.Votes
                    join userVote in _context.UserVotes on vote.Id equals userVote.VoteId into voteGroup
                    from userVote in voteGroup.DefaultIfEmpty()
                    where userVote.UserId == request.UserId && userVote.Role == "Candidate"
                    select vote;

                // Lọc theo VoteName nếu được cung cấp
                if (!string.IsNullOrEmpty(request.VoteName))
                {
                    query = query.Where(v => v.VoteName.Contains(request.VoteName));
                }

                // Lọc theo Status nếu được cung cấp
                if (!string.IsNullOrEmpty(request.Status))
                {
                    query = query.Where(v => v.Status.Contains(request.Status));
                }

                // Thực hiện truy vấn và lấy danh sách
                var entities = await query.ToListAsync(cancellationToken);

                // Ánh xạ từ entity sang DTO
                var result = _mapper.Map<List<VotingReponse>>(entities);

                // Trả về kết quả
                return new ResultCustom<List<VotingReponse>>
                {
                    Status = StatusCode.OK,
                    Message = new[] { "Get all Vote successfully" },
                    Data = result
                };
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                return new ResultCustom<List<VotingReponse>>
                {
                    Status = StatusCode.INTERNALSERVERERROR,
                    Message = new[] { ex.Message }
                };
            }
        }
    }
}