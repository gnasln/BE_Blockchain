using AutoMapper;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetHelper.Common.Models;

namespace Base_BE.Application.Position.Commands;

public class UpdatePositionCommand : IRequest<ResultCustom<PositionResponse>>
{
    public required Guid Id { get; set; }
    public required string PositionName { get; set; }
    public string? PositionDescription { get; set; }
    public bool Status { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<UpdatePositionCommand, Domain.Entities.Position>();
            CreateMap<Domain.Entities.Position, PositionResponse>();
        }
    }
}

public class UpdatePositionCommandhandle : IRequestHandler<UpdatePositionCommand, ResultCustom<PositionResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdatePositionCommandhandle(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ResultCustom<PositionResponse>> Handle(UpdatePositionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Positions
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return new ResultCustom<PositionResponse>
            {
                Status = StatusCode.NOTFOUND,
                Message = new[] { "Position not found" },
                Data = null
            };
        }

        entity = _mapper.Map(request, entity);

        _context.Positions.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new ResultCustom<PositionResponse>
        {
            Status = StatusCode.OK,
            Message = new[] { "Position updated successfully" },
            Data = _mapper.Map<PositionResponse>(entity)
        };
    }
}