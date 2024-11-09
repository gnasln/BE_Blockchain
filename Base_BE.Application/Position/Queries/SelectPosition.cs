using AutoMapper;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetHelper.Common.Models;

namespace Base_BE.Application.Position.Queries;

public class SelectPositionQuery : IRequest<ResultCustomPaginate<IEnumerable<PositionResponse>>>
{
    public int Page { get; set; }
    public int PageSize { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<PositionResponse, Base_BE.Domain.Entities.Position>();
        }
    }
}

public class SelectPositionQueryHandle : IRequestHandler<SelectPositionQuery, ResultCustomPaginate<IEnumerable<PositionResponse>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SelectPositionQueryHandle(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ResultCustomPaginate<IEnumerable<PositionResponse>>> Handle(SelectPositionQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Positions.AsQueryable();

        var totalItems = await query.CountAsync(cancellationToken);

        var entity = await query.Where(e => e.Status == true)
            .Skip(request.PageSize * (request.Page - 1))
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var response = _mapper.Map<IEnumerable<PositionResponse>>(entity);

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