using Base_BE.Application.Vote.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Base_BE.Endpoints;

public class Vote : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapPost(CreateVote, "/create")
            ;
        
    }
    
    public async Task<IResult> CreateVote([FromServices] ISender sender, [FromBody] CreateVoteCommand request)
    {
        var result = await sender.Send(request);
        return Results.Ok(new
        {
            status = result.Status,
            message = result.Message,
            data = result.Data
        });
    }
}