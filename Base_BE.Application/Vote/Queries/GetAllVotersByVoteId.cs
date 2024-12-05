using AutoMapper;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Application.Dtos;
using Base_BE.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetHelper.Common.Models;

namespace Base_BE.Application.Vote.Queries;

public class GetAllVotersByVoteIdQueries : IRequest<ResultCustom<List<VoterDto>>>
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

public class GetAllVotersByVoteIdQueriesHandler : IRequestHandler<GetAllVotersByVoteIdQueries, ResultCustom<List<VoterDto>>>
{
    private readonly IApplicationDbContext _context;
    
    public GetAllVotersByVoteIdQueriesHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<ResultCustom<List<VoterDto>>> Handle(GetAllVotersByVoteIdQueries request, CancellationToken cancellationToken)
    {
        var voters = await (from uv in _context.UserVotes
                join au in _context.ApplicationUsers on uv.UserId equals au.Id into userGroup
                from user in userGroup.DefaultIfEmpty()
                where uv.VoteId == request.VoteId && uv.Role == "Voter"
                select new { uv, user })
            .ToListAsync(cancellationToken);

        var voterDtos = voters.Select(v => new VoterDto
        {
            Id = v.user.Id,
            Fullname = v.user.FullName,
            Email = v.user.Email,
            NewEmail = v.user.NewEmail,
            CellPhone = v.user.CellPhone,
            Address = v.user.Address,
            Birthday = v.user.Birthday,
            ImageUrl = v.user.ImageUrl,
            Status = v.uv.Status
        }).ToList();
        
        return new ResultCustom<List<VoterDto>>
        {
            Status = StatusCode.OK,
            Message = new[] { "Get all candidates successfully" },
            Data = voterDtos
        };
    }
}