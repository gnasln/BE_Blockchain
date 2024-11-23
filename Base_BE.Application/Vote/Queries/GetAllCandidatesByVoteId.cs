using AutoMapper;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Application.Dtos;
using Base_BE.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetHelper.Common.Models;

namespace Base_BE.Application.Vote.Queries;

public class GetAllCandidatesByVoteIdQueries : IRequest<ResultCustom<List<CandidateDto>>>
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

public class GetAllCandidatesByIdQueriesHandler : IRequestHandler<GetAllCandidatesByVoteIdQueries, ResultCustom<List<CandidateDto>>>
{
    private readonly IApplicationDbContext _context;
    
    
    public GetAllCandidatesByIdQueriesHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<ResultCustom<List<CandidateDto>>> Handle(GetAllCandidatesByVoteIdQueries request, CancellationToken cancellationToken)
    {
        var candidates = await (from uv in _context.UserVotes
                join au in _context.ApplicationUsers on uv.UserId equals au.Id into userGroup
                from user in userGroup.DefaultIfEmpty()
                where uv.VoteId == request.VoteId && uv.Role == "Candidate"
                select new { uv, user })
            .ToListAsync(cancellationToken);

        var candidateDtos = candidates.Select(x => new CandidateDto
        {
            Id = x.user.Id,
            UserName = x.user.UserName,
            Email = x.user.Email,
            Birthday = x.user.Birthday,
            CellPhone = x.user.CellPhone,
            Address = x.user.Address,
            FullName = x.user.FullName,
            ImageUrl = x.user.ImageUrl,
            NewEmail = x.user.NewEmail,
            IdentityCardImage = x.user.IdentityCardImage
        }).ToList();

        return new ResultCustom<List<CandidateDto>>
        {
            Status = StatusCode.OK,
            Message = new[] { "Get all candidates successfully" },
            Data = candidateDtos
        };
    }
}