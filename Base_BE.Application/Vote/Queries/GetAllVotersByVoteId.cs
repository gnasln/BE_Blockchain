using AutoMapper;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Application.Dtos;
using Base_BE.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetHelper.Common.Models;

namespace Base_BE.Application.Vote.Queries;

public class GetAllVotersByVoteIdQueries : IRequest<ResultCustom<List<UserDto>>>
{
    public Guid VoteId { get; set; }
    
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, UserDto>();
        }
    }
}

public class GetAllVotersByVoteIdQueriesHandler : IRequestHandler<GetAllVotersByVoteIdQueries, ResultCustom<List<UserDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    
    public GetAllVotersByVoteIdQueriesHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<ResultCustom<List<UserDto>>> Handle(GetAllVotersByVoteIdQueries request, CancellationToken cancellationToken)
    {
        var voters = await (from uv in _context.UserVotes
                join au in _context.ApplicationUsers on uv.UserId equals au.Id into userGroup
                from user in userGroup.DefaultIfEmpty()
                where uv.VoteId == request.VoteId && uv.Role == "Voter"
                select new { uv, user })
            .ToListAsync(cancellationToken);

        var voterDtos = _mapper.Map<List<UserDto>>(voters.Select(x => x.user));
        
        return new ResultCustom<List<UserDto>>
        {
            Status = StatusCode.OK,
            Message = new[] { "Get all candidates successfully" },
            Data = voterDtos
        };
    }
}