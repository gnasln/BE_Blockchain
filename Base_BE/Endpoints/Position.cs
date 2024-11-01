using Base_BE.Application.Position.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Base_BE.Endpoints
{
    public class Position : EndpointGroupBase
    {
        public override void Map(WebApplication app)
        {
            app.MapGroup(this)
                .RequireAuthorization()
                .MapPost(CreatePosition, "/create")
                .MapPut(UpdatePosition, "/update")
                .MapDelete(DeletePosition, "/delete/{id}");
            ;
        }

        public async Task<IResult> CreatePosition([FromServices] ISender sender, [FromBody] CreatePositionCommand request)
        {
            var result = await sender.Send(request);
            return Results.Ok(new
            {
                status = result.Status,
                message = result.Message,
                data = result.Data
            });
        }

        public async Task<IResult> UpdatePosition([FromServices] ISender sender, [FromBody] UpdatePositionCommand request)
        {
            var result = await sender.Send(request);
            return Results.Ok(new
            {
                status = result.Status,
                message = result.Message,
                data = result.Data
            });
        }

        public async Task<IResult> DeletePosition([FromServices] ISender sender, [FromRoute] Guid id)
        {
            var result = await sender.Send(new DeletePositionCommand() { Id = id });
            return Results.Ok(new
            {
                status = result.Status,
                message = result.Message
            });
        }
    }
}
