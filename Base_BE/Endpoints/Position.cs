﻿using Base_BE.Application.Dtos;
using Base_BE.Application.Position.Commands;
using Base_BE.Application.Position.Queries;
using Microsoft.AspNetCore.Mvc;
using NetHelper.Common.Models;

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
                .MapDelete(DeletePosition, "/delete/{id}")
                .MapGet(GetAllPosition, "/view-list")
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

        public async Task<ResultCustomPaginate<IEnumerable<PositionResponse>>> GetAllPosition([FromServices] ISender sender, int page, int pageSize)
        {
            var result = await sender.Send(new GetAllPositionQuery() { Page = page, PageSize = pageSize }); 
            return new ResultCustomPaginate<IEnumerable<PositionResponse>>
            {
                Status = result.Status,
                Message = result.Message,
                Data = result.Data,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }
    }
}
