using AutoMapper;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Application.Dtos;
using Base_BE.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetHelper.Common.Models;

namespace Base_BE.Application.Position.Commands;

public record CreatePositionCommand : IRequest<ResultCustom<PositionResponse>>
{
    public required string PositionName { get; set; }
    public string? PositionDescription { get; set; } = string.Empty;
    public bool Status { get; set; } = false;

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CreatePositionCommand, Domain.Entities.Position>();
            CreateMap<Domain.Entities.Position, PositionResponse>();
        }
    }

}

public class CreatePositionCommandHandler : IRequestHandler<CreatePositionCommand, ResultCustom<PositionResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;   

    public CreatePositionCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ResultCustom<PositionResponse>> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
    {

        // Check if a position with the same name already exists
        var existingPosition = await _context.Positions
            .FirstOrDefaultAsync(p => p.PositionName == request.PositionName, cancellationToken);

        if (existingPosition != null)
        {
            return new ResultCustom<PositionResponse>
            {
                Status = StatusCode.CONFLICT,
                Message = new[] { "Position with the same name already exists" },
                Data = null
            };
        }

        var entity = _mapper.Map<Domain.Entities.Position>(request);

        await _context.Positions.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<PositionResponse>(entity);

        return new ResultCustom<PositionResponse>
        {
            Status = StatusCode.CREATED,
            Message = new[] { "Position created successfully" },
            Data = response
        };

    }
}