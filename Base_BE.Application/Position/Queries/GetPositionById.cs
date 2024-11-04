using AutoMapper;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Application.Dtos;
using MediatR;
using NetHelper.Common.Models;

namespace Base_BE.Application.Position.Queries;

public class GetPositionByIdQuery : IRequest<ResultCustom<PositionResponse>>
{
    public Guid Id { get; set; }
    
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<PositionResponse, Base_BE.Domain.Entities.Position>();
        }
    }
}

public class GetPositionByIdQueryHandle : IRequestHandler<GetPositionByIdQuery, ResultCustom<PositionResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPositionByIdQueryHandle(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ResultCustom<PositionResponse>> Handle(GetPositionByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _context.Positions.FindAsync(request.Id, cancellationToken);
            if (entity != null)
            {
                var result = _mapper.Map<PositionResponse>(entity);
                return new ResultCustom<PositionResponse>()
                {
                    Status = StatusCode.OK,
                    Message = new[] { "Position found successfully" },
                    Data = result
                };
            }
            else
            {
                return new ResultCustom<PositionResponse>()
                {
                    Status = StatusCode.NOTFOUND,
                    Message = new[] { "Position not found" },
                    Data = null
                };
            }
        }catch (Exception ex)
        {
            return new ResultCustom<PositionResponse>()
            {
                Status = StatusCode.INTERNALSERVERERROR,
                Message = new[] { ex.Message },
                Data = null
            };
        }
    }
}