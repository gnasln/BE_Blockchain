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
        public string? VoteName { get; set; }
        public string? Status { get; set; }
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
                // Bắt đầu tạo truy vấn
                var query = _context.Votes.AsQueryable();

                // Tìm kiếm theo VoteName
                if (!string.IsNullOrEmpty(request.VoteName))
                {
                    query = query.Where(v => v.VoteName.Contains(request.VoteName));
                }

                // Tìm kiếm theo Status
                if (!string.IsNullOrEmpty(request.Status))
                {
                    query = query.Where(v => v.Status.Contains(request.Status));
                }

                // Lấy danh sách các entity sau khi áp dụng bộ lọc
                var entities = await query.ToListAsync(cancellationToken);

                // Chuyển đổi entities thành DTOs
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
