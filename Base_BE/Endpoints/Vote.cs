using Base_BE.Application.Common.Interfaces;
using Base_BE.Application.Dtos;
using Base_BE.Application.Vote.Commands;
using Base_BE.Application.Vote.Queries;
using Base_BE.Domain.Entities;
using Base_BE.Dtos;
using Base_BE.Helper;
using Base_BE.Helper.key;
using Base_BE.Helper.Services;
using Base_BE.Infrastructure.Data;
using Base_BE.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetHelper.Common.Models;
using System.Security.Cryptography;
using System.Text.Json;
using IUser = Base_BE.Application.Common.Interfaces.IUser;

namespace Base_BE.Endpoints;

public class Vote : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .RequireAuthorization("admin")
            .MapPost(CreateVote, "/create")
            .MapPut(UpdateVote, "/update")
            .MapDelete(DeleteVote, "/delete/{id}")
            .MapGet(GetAllVote, "/View-list")
        ;

        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetAllCandidatesByVoteId, "/View-candidates/{id}")
            .MapGet(GetAllVotersByVoteId, "/View-voters/{id}")
            .MapGet(GetAllVoteForUser, "/View-list-for-user")
            .MapGet(GetVoteById, "/View-detail/{id}")
            .MapPost(SubmitVote, "/submit-vote")
            .MapGet(GetHistoryVote, "/History-vote")
            .MapGet(GetAllVoteForCandidate, "/View-list-for-candidate")
            .MapPost(SendMailForCandidate, "/send-mail-candidate")
            ;

    }

    public async Task<IResult> CreateVote(
    [FromServices] ISender sender,
    [FromBody] CreateVoteCommand request,
    [FromServices] IEmailSender emailSender,
    [FromServices] UserManager<ApplicationUser> userManager,
    [FromServices] IBackgroundTaskQueue taskQueue)
    {
        // await SendEmailForCandidate(request, emailSender, userManager, taskQueue);
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

        List<ApplicationUser> voters = new List<ApplicationUser>();
        foreach (var voter in request.Voters)
        {
            var user1 = await userManager.FindByIdAsync(voter);
            voters.Add(user1);
        }
        var voterContent = $"Bạn đã được thêm vào cuộc bầu cử: \"{request.VoteName}\" với vai trò là cử tri.";

        //Thêm nhiệm vụ gửi email cho cử tri vào hàng đợi
        // taskQueue.QueueBackgroundWorkItem(async ct =>
        // {
            await emailSender.SendEmailNotificationAsync(voters!, voterContent, request.VoteName, string.Join(", ", request.CandidateNames), request.StartDate, request.ExpiredDate);
        // });

        
        // List<ApplicationUser> candidates = new List<ApplicationUser>();
        // foreach (var candidate in request.Candidates)
        // {
        //     var candidateUser = await userManager.FindByIdAsync(candidate);
        //     candidates.Add(candidateUser);
        // }
        //
        // var candidateContent = $"Bạn đã được thêm vào cuộc bầu cử: \"{request.VoteName}\" với vai trò là ứng viên.";
        //
        // //Thêm nhiệm vụ gửi email cho ứng viên vào hàng đợi
        // taskQueue.QueueBackgroundWorkItem(async ct =>
        // {
        //     await emailSender.SendEmailNotificationCandidateAsync(candidates!, candidateContent, request.VoteName, string.Join(", ", request.CandidateNames), request.StartDate, request.ExpiredDate);
        // });
        return Results.Ok(new
        {
            status = result.Status,
            message = result.Message,
            data = result.Data
        });
    }

    public async Task<IResult> SendMailForCandidate(
        [FromBody] CreateVoteCommand request,
        [FromServices] IEmailSender emailSender,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IBackgroundTaskQueue taskQueue)
    {
        List<ApplicationUser> candidates = new List<ApplicationUser>();
        foreach (var candidate in request.Candidates)
        {
            var candidateUser = await userManager.FindByIdAsync(candidate);
            if (candidateUser != null) candidates.Add(candidateUser);
        }
        
        var candidateContent = $"Bạn đã được thêm vào cuộc bầu cử: \"{request.VoteName}\" với vai trò là ứng viên.";
        
        //Thêm nhiệm vụ gửi email cho ứng viên vào hàng đợi
        // taskQueue.QueueBackgroundWorkItem(async ct =>
        // {
            await emailSender.SendEmailNotificationCandidateAsync(candidates!, candidateContent, request.VoteName, string.Join(", ", request.CandidateNames), request.StartDate, request.ExpiredDate);
        // });
        return Results.Ok(new
        {
            status = StatusCode.OK,
            message = "Send mail success",
            data = candidates
        });
    }

    public async Task<IResult> UpdateVote([FromBody] UpdateVoteCommand request, [FromServices]ISender sender)
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

        return Results.Ok(new
        {
            status = result.Status,
            message = result.Message,
            data = result.Data
        });
    }

    public async Task<IResult> DeleteVote([FromRoute] Guid id, ISender sender)
    {
        var result = await sender.Send(new DeleteVoteCommand() { Id = id});
        if (result.Status == StatusCode.INTERNALSERVERERROR)
        {
            return Results.BadRequest(new
            {
                status = result.Status,
                message = result.Message,
                data = result.Data
            });
        }

        return Results.Ok(new
        {
            status = result.Status,
            message = result.Message,
            data = result.Data
        });
    }

    public async Task<IResult> GetAllVote([FromServices] ISender sender, [FromServices] ApplicationDbContext dbContext, string? voteName, string? status)
    {
        var result = await sender.Send(new GetAllVoteQueries() { VoteName = voteName, Status = status });
        if (result.Status == StatusCode.INTERNALSERVERERROR)
        {
            return Results.BadRequest(new
            {
                status = result.Status,
                message = result.Message,
                data = result.Data
            });
        }

        try
        {
            result.Data?.ForEach(vote =>
            {
                vote.PositionName = (dbContext.Positions.FirstOrDefault(x => x.Id == vote.PositionId))!.PositionName;
            });

            return Results.Ok(new
            {
                status = result.Status,
                message = result.Message,
                data = result.Data
            });
        }
        catch (Exception ex)
        {
            return Results.Problem("An error occurred while processing the request", ex.Message);
        }
    }

    public async Task<IResult> GetVoteById([FromRoute] Guid id, [FromServices] ISender sender)
    {
        var result = await sender.Send(new GetVoteByIdQueries() { Id = id });
        if (result.Status == StatusCode.INTERNALSERVERERROR)
        {
            return Results.BadRequest(new
            {
                status = result.Status,
                message = result.Message,
                data = result.Data
            });
        }

        return Results.Ok(new
        {
            status = result.Status,
            message = result.Message,
            data = result.Data
        });
    }
    
    public async Task<IResult> GetAllCandidatesByVoteId([FromRoute] Guid id, [FromServices] ISender sender, [FromServices] SmartContractService smartContractService)
    {
        var result = await sender.Send(new GetAllCandidatesByVoteIdQueries() { VoteId = id });
        var listCandidate = new List<Base_BE.Dtos.CandidateDto>();
        foreach (var item in result.Data)
        {
            var candidate = new Base_BE.Dtos.CandidateDto
            {
                Id = item.Id,
                FullName = item.FullName,
                UserName = item.UserName,
                Email = item.Email,
                NewEmail = item.NewEmail,
                Address = item.Address,
                CellPhone = item.CellPhone,
                Birthday = item.Birthday,
                ImageUrl = item.ImageUrl,
                IdentityCardImage = item.IdentityCardImage,
                TotalBallot = await smartContractService.CountBallotForCandidateAsync(item.Id!, id.ToString())
            };
            listCandidate.Add(candidate);
        }
        if (result.Status == StatusCode.INTERNALSERVERERROR)
        {
            return Results.BadRequest(new
            {
                status = result.Status,
                message = result.Message,
                data = result.Data
            });
        }

        return Results.Ok(new
        {
            status = result.Status,
            message = result.Message,
            data = listCandidate
        });
    }

    public async Task<IResult> GetAllVoteForUser(
    [FromServices] IUser _user,
    [FromServices] ISender sender,
    ApplicationDbContext dbContext, string? voteName, string? status)
    {
        if (_user == null)
        {
            return Results.BadRequest(new
            {
                status = StatusCode.UNAUTHORIZED,
                message = "User not found"
            });
        }

        var result = await sender.Send(new GetAllVoteForUserQueries { UserId = _user.Id!, VoteName = voteName, Status = status });
        if (result.Status == StatusCode.INTERNALSERVERERROR)
        {
            return Results.BadRequest(new
            {
                status = result.Status,
                message = result.Message,
                data = result.Data
            });
        }

        try
        {
            var userId = Guid.Parse(_user.Id!); // Ensure type safety
            var voteIds = result.Data?.Select(v => v.Id).ToList();

            var candidateData = await (from uv in dbContext.UserVotes
                                       join bv in dbContext.BallotVoters on uv.BallotAddress equals bv.Address
                                       join au in dbContext.ApplicationUsers on bv.CandidateId.ToString() equals au.Id
                                       where uv.UserId == _user.Id && voteIds.Contains(uv.VoteId) && bv.VoterId == userId
                                       select new { uv.VoteId, au.FullName }).ToListAsync();

            var groupedCandidates = candidateData
                .GroupBy(cd => cd.VoteId)
                .ToDictionary(g => g.Key, g => g.Select(c => c.FullName).ToList());

            result.Data?.ForEach( vote =>
            {
                vote.SelectedCandidates = groupedCandidates.GetValueOrDefault(vote.Id, new List<string>());
                vote.PositionName = ( dbContext.Positions.FirstOrDefault(x => x.Id == vote.PositionId))!.PositionName;
            });

            

            return Results.Ok(new
            {
                status = result.Status,
                message = result.Message,
                data = result.Data
            });
        }
        catch (Exception ex)
        {
            return Results.Problem("An error occurred while processing the request", ex.Message);
        }
    }

    public async Task<IResult> GetAllVoteForCandidate(
    [FromServices] IUser _user,
    [FromServices] ISender sender,
    ApplicationDbContext dbContext,
    string? voteName, string? status)
    {
        if (_user == null)
        {
            return Results.BadRequest(new
            {
                status = StatusCode.UNAUTHORIZED,
                message = "User not found"
            });
        }

        var result = await sender.Send(new GetAllVoteForCandidateQueries { UserId = _user.Id!, VoteName = voteName, Status = status });
        if (result.Status == StatusCode.INTERNALSERVERERROR)
        {
            return Results.BadRequest(new
            {
                status = result.Status,
                message = result.Message,
                data = result.Data
            });
        }

        try
        {
            result.Data?.ForEach(vote =>
            {
                vote.PositionName = (dbContext.Positions.FirstOrDefault(x => x.Id == vote.PositionId))!.PositionName;
            });



            return Results.Ok(new
            {
                status = result.Status,
                message = result.Message,
                data = result.Data
            });
        }
        catch (Exception ex)
        {
            return Results.Problem("An error occurred while processing the request", ex.Message);
        }
    }

    public async Task<IResult> GetHistoryVote(
    [FromServices] IUser _user,
    [FromServices] ISender sender,
    ApplicationDbContext dbContext,
    int page, int pageSize)
    {
        if (_user == null)
        {
            return Results.BadRequest(new
            {
                status = StatusCode.UNAUTHORIZED,
                message = "User not found"
            });
        }

        var result = await sender.Send(new GetHistoryVoteQueries { UserId = _user.Id! });
        if (result.Status == StatusCode.INTERNALSERVERERROR)
        {
            return Results.BadRequest(new
            {
                status = result.Status,
                message = result.Message
            });
        }

        try
        {
            var userId = Guid.Parse(_user.Id!); // Ensure type safety
            var voteIds = result.Data?.Select(v => v.Id).ToList();

            var candidateData = await (from uv in dbContext.UserVotes
                                       join bv in dbContext.BallotVoters on uv.BallotAddress equals bv.Address
                                       join au in dbContext.ApplicationUsers on bv.CandidateId.ToString() equals au.Id
                                       where uv.UserId == _user.Id && voteIds.Contains(uv.VoteId) && bv.VoterId == userId
                                       select new { uv.VoteId, au.FullName }).ToListAsync();

            var groupedCandidates = candidateData
                .GroupBy(cd => cd.VoteId)
                .ToDictionary(g => g.Key, g => g.Select(c => c.FullName).ToList());

            result.Data?.ForEach(vote =>
            {
                vote.SelectedCandidates = groupedCandidates.GetValueOrDefault(vote.Id, new List<string>());
                vote.PositionName = (dbContext.Positions.FirstOrDefault(x => x.Id == vote.PositionId))!.PositionName;
            });

            // Add the counts of candidates and voters
            var roleCounts = await (from uv in dbContext.UserVotes
                                    where voteIds.Contains(uv.VoteId)
                                    group uv by uv.VoteId into g
                                    select new
                                    {
                                        VoteId = g.Key,
                                        CandidateCount = g.Count(uv => uv.Role == "Candidate"),
                                        VoterCount = g.Count(uv => uv.Role == "Voter")
                                    }).ToListAsync();

            var roleCountsDict = roleCounts.ToDictionary(rc => rc.VoteId, rc => new { rc.CandidateCount, rc.VoterCount });

            result.Data?.ForEach(vote =>
            {
                var counts = roleCountsDict.GetValueOrDefault(vote.Id, new { CandidateCount = 0, VoterCount = 0 });
                vote.TotalCandidate = counts.CandidateCount;
                vote.TotalVoter = counts.VoterCount;
            });

            var sortedData = result.Data?.OrderByDescending(v => v.StartDate).ToList();
            // Áp dụng phân trang
            var paginatedVotes = sortedData.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var res = new ResultCustomPaginate<List<VotingReponse>>
            {
                Data = paginatedVotes,
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = paginatedVotes.Count,
                TotalPages = (int)Math.Ceiling((double)paginatedVotes.Count / pageSize)
            };

            return Results.Ok(new
            {
                status = result.Status,
                message = result.Message,
                data = res
            });
        }
        catch (Exception ex)
        {
            return Results.Problem("An error occurred while processing the request", ex.Message);
        }
    }


    public async Task<IResult> SubmitVote([FromBody] EncryptData encryptData, [FromServices] ISender sender, SmartContractService smartContractService, IUser user, UserManager<ApplicationUser> userManager, IApplicationDbContext dbContext)
    {
        //giai ma
        string rawData = RsaUtil.Decrypt(encryptData.EncruptData, Constant.PRIVATE_KEY);
        var request = JsonSerializer.Deserialize<SubmitVoteModel>(rawData);

        //check validate
        var checkExistVote = await smartContractService.CheckExistBallotAsync(user.Id!, request.VoteId!);
        if (checkExistVote)
        {
            throw new BadHttpRequestException("Bạn đã bỏ phiếu cho cuộc bầu cử này, không thể bầu cử thêm");
        }

        // Kiểm tra private key
        await CheckPrivateKey(request!.PrivateKey, user, userManager);

        string privateKey = RandomPrivateKeyGenerator.GetRandomPrivateKey();
        Ether ether = EtherService.GenerateAddress(privateKey);

        SubmitVoteModel submitVoteModel = new SubmitVoteModel
        {
            BitcoinAddress = ether.Address,
            Candidates = request!.Candidates,
            VoterId = user.Id!,
            VoteId = request.VoteId,
            VotedTime = DateTime.UtcNow,
            PrivateKey = request.PrivateKey
        };

        Console.WriteLine("Voter {} submitted a vote " + user.UserName);

        var result = await smartContractService.SubmitVoteAsync(submitVoteModel);

        if (result == null)
        {
            return Results.BadRequest(new
            {
                status = StatusCode.INTERNALSERVERERROR,
                message = "Error submitting vote"
            });
        }

        var ballotVoter = new CreateBallotVoterCommand
        {
            VoterId = Guid.Parse(submitVoteModel.VoterId),
            CandidateIds = submitVoteModel.Candidates.Select(Guid.Parse).ToList(),
            VotedTime = submitVoteModel.VotedTime,
            Address = submitVoteModel.BitcoinAddress,
            VoteId = Guid.Parse(submitVoteModel.VoteId),
            BallotTransaction = result.TransactionHash
        };

        var ballotRes = await sender.Send(ballotVoter);
        if(ballotRes.Status == StatusCode.INTERNALSERVERERROR)
        {
            return Results.BadRequest(new
            {
                status = ballotRes.Status,
                message = ballotRes.Message
            });
        }

        return Results.Ok(new
        {
            status = result.Status,
            message = "success",
            data = result
        });
    }

    private async Task<bool> CheckPrivateKey(string privateKey, IUser user, [FromServices] UserManager<ApplicationUser> userManager)
    {
        // Retrieve the current user from UserManager
        var currentUser = await userManager.FindByIdAsync(user.Id!);
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("User not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(currentUser.PublicKey))
        {
            throw new InvalidOperationException("The user does not have a valid public key.");
        }

        try
        {
            // Sinh địa chỉ Ether từ private key
            var ether = EtherService.GenerateAddress(privateKey);

            // So sánh public key sinh ra với public key của người dùng
            return ether.PublicKey.Equals(currentUser.PublicKey, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("The provided private key is not in a valid Base64 format.", ex);
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException("Failed to process the private key. Ensure it is valid and properly formatted.", ex);
        }
    }
    
    public async Task<IResult> GetAllVotersByVoteId([FromRoute] Guid id, [FromServices] ISender sender)
    {
        var result = await sender.Send(new GetAllVotersByVoteIdQueries() { VoteId = id });
        var listVoters = new List<Base_BE.Dtos.VoterDto>();
        foreach (var item in result.Data)
        {
            var voter = new Base_BE.Dtos.VoterDto()
            {
                Id = item.Id,
                Fullname = item.Fullname,
                Email = item.Email,
                NewEmail = item.NewEmail,
                Address = item.Address,
                CellPhone = item.CellPhone,
                Birthday = item.Birthday,
                ImageUrl = item.ImageUrl,
                Status = item.Status,
            };
            listVoters.Add(voter);
        }
        if (result.Status == StatusCode.INTERNALSERVERERROR)
        {
            return Results.BadRequest(new
            {
                status = result.Status,
                message = result.Message,
                data = result.Data
            });
        }

        return Results.Ok(new
        {
            status = result.Status,
            message = result.Message,
            data = listVoters
        });
    }
    
}