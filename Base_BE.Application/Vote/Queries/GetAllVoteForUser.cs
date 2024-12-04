using AutoMapper;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetHelper.Common.Models;


namespace Base_BE.Application.Vote.Queries
{
    public class GetAllVoteForUserQueries : IRequest<ResultCustom<List<VotingReponse>>>
    {
        public string UserId { get; set; }
        public string? VoteName { get; set; }
        public string? Status { get; set; }
    }

    public class GetAllVoteForUserQueriesHandler : IRequestHandler<GetAllVoteForUserQueries, ResultCustom<List<VotingReponse>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetAllVoteForUserQueriesHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ResultCustom<List<VotingReponse>>> Handle(GetAllVoteForUserQueries request, CancellationToken cancellationToken)
        {
            try
            {
                // Xây dựng truy vấn với join và điều kiện lọc
                var query = from vote in _context.Votes
                    join userVote in _context.UserVotes on vote.Id equals userVote.VoteId into voteGroup
                    from userVote in voteGroup.DefaultIfEmpty()
                    where userVote.UserId == request.UserId 
                          && userVote.Role == "Voter" // Kiểm tra quyền của user
                    select vote;

                // Tìm kiếm theo VoteName nếu có
                if (!string.IsNullOrEmpty(request.VoteName))
                {
                    query = query.Where(v => v.VoteName.Contains(request.VoteName));
                }

                // Tìm kiếm theo Status nếu có
                if (!string.IsNullOrEmpty(request.Status))
                {
                    query = query.Where(v => v.Status.Contains(request.Status));
                }

                // Lấy kết quả
                var entities = await query.ToListAsync(cancellationToken);

                // Chuyển đổi entities thành DTO
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