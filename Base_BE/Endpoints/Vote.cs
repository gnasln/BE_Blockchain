using Base_BE.Application.Vote.Commands;
using Base_BE.Domain.Entities;
using Base_BE.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using NetHelper.Common.Models;

namespace Base_BE.Endpoints;

public class Vote : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .RequireAuthorization("admin")
            .MapPost(CreateVote, "/create")
            ;
        
    }
    
    public async Task<IResult> CreateVote(
        [FromServices] ISender sender,
        [FromBody] CreateVoteCommand request,
        [FromServices] EmailSender emailSender,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IBackgroundTaskQueue taskQueue)
    {
        var result = await sender.Send(request);
        if (result.Status == StatusCode.INTERNALSERVERERROR)
        {
            return Results.BadRequest(new
            {
                status = result.Status,
                message = result.Message,
                data = result.Data
            });
        }

        foreach (var voter in request.Voters)
        {
            var user1 = await userManager.FindByIdAsync(voter);
            var email1 = user1.Email ?? user1.NewEmail;
            var content = $"Bạn đã được thêm vào cuộc bầu cử: \"{request.VoteName}\" với vai trò là cử tri";

            // Queue email sending task
            taskQueue.QueueBackgroundWorkItem(async ct =>
            {
                await emailSender.SendEmailNotificationAsync(email1, user1.FullName, content, request.VoteName, string.Join(", ", request.CandidateNames), request.StartDate, request.ExpiredDate);
            });
        }

        foreach (var candidate in request.Candidates)
        {
            var user2 = await userManager.FindByIdAsync(candidate);
            var email2 = user2.Email ?? user2.NewEmail;
            var content = $"Bạn đã được thêm vào cuộc bầu cử: \"{request.VoteName}\" với vai trò là ứng viên";

            // Queue email sending taskss
            taskQueue.QueueBackgroundWorkItem(async ct =>
            {
                await emailSender.SendEmailNotificationAsync(email2, user2.FullName, content, request.VoteName, string.Join(", ", request.CandidateNames), request.StartDate, request.ExpiredDate);
            });
        }

        return Results.Ok(new
        {
            status = result.Status,
            message = result.Message,
            data = result.Data
        });
    }

    
}