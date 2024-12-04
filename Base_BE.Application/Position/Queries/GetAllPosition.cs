using AutoMapper;
using AutoMapper.Configuration;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Application.Dtos;
using Base_BE.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetHelper.Common.Models;

namespace Base_BE.Application.Position.Queries;

public class GetAllPositionQuery : IRequest<ResultCustomPaginate<IEnumerable<PositionResponse>>>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? PositionName { get; set; }
    public bool? Status { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<PositionResponse, Base_BE.Domain.Entities.Position>();
        }
    }
}

public class GetAllPositionQueryHandle : IRequestHandler<GetAllPositionQuery, ResultCustomPaginate<IEnumerable<PositionResponse>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllPositionQueryHandle(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ResultCustomPaginate<IEnumerable<PositionResponse>>> Handle(GetAllPositionQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Positions.AsQueryable();

        // Tìm kiếm theo PositionName
        if (!string.IsNullOrEmpty(request.PositionName))
        {
            query = query.Where(p => p.PositionName.Contains(request.PositionName));
        }

        // Tìm kiếm theo Status
        if (request.Status.HasValue)
        {
            query = query.Where(p => p.Status == request.Status.Value);
        }

        // Lấy tổng số bản ghi
        var totalItems = await query.CountAsync(cancellationToken);

        // Lấy danh sách bản ghi theo phân trang
        var entity = await query
            .Skip(request.PageSize * (request.Page - 1))
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Chuyển đổi entity sang DTO
        var response = _mapper.Map<IEnumerable<PositionResponse>>(entity);

        // Trả về kết quả phân trang
        return new ResultCustomPaginate<IEnumerable<PositionResponse>>
        {
            Status = StatusCode.OK,
            Message = new[] { "Position found successfully" },
            Data = response,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize),
            PageNumber = request.Page,
            PageSize = request.PageSize
        };
    }

}