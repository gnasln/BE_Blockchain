using Base_BE.Identity;
using Base_BE.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceStack;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Domain.Constants;

namespace Base_BE.Endpoints
{
    public class User : EndpointGroupBase
    {
        public override void Map(WebApplication app)
        {
            app.MapGroup(this)
                .MapPost(RegisterUser, "/register");
            
            
            app.MapGroup(this)
                .RequireAuthorization()
                .MapPut(ChangePassword, "/change-password");
        }

        public async Task<IResult> RegisterUser([FromBody] RegisterForm newUser,
            UserManager<ApplicationUser> _userManager)
        {
            if (string.IsNullOrEmpty(newUser.UserName))
            {
                return Results.BadRequest("400|Username is required");
            }

            // Kiểm tra username chỉ chứa chữ cái và chữ số
            if (!newUser.UserName.All(char.IsLetterOrDigit))
            {
                return Results.BadRequest("400|Username can only contain letters or digits.");
            }

            var user = await _userManager.FindByNameAsync(newUser.UserName);
            if (user != null)
            {
                return Results.BadRequest("501|User existed");
            }


            // Tạo mới đối tượng ApplicationUser
            var newUserEntity = new ApplicationUser
            {
                UserName = newUser.UserName,
                Email = newUser.Email,
                FullName = newUser.FullName,
                Birthday = newUser.Birthday,
                Address = newUser.Address,
                Gender = newUser.Gender,
                CellPhone = newUser.CellPhone,
                IdentityCardNumber = newUser.IdentityCardNumber,
                IdentityCardDate = newUser.IdentityCardDate,
                IdentityCardPlace = newUser.IdentityCardPlace,
                ImageUrl = newUser.ImageUrl,
                IdentityCardImage = newUser.UrlIdentityCardImage
            };

            // Tạo tài khoản người dùng mới
            var passwordSeed = "Abc@" + (newUser.Birthday?.ToString("ddMMyyyy") ?? "DefaultDate");
            var result = await _userManager.CreateAsync(newUserEntity, passwordSeed);

            if (result.Succeeded)
            {
                if (newUser.IsAdmin == true)
                {
                    var roleResult = await _userManager.AddToRoleAsync(newUserEntity, Roles.Administrator);
                    if (!roleResult.Succeeded)
                    {
                        return Results.BadRequest("500|Add role failed");
                    }
                }
                else
                {
                    var roleResult = await _userManager.AddToRoleAsync(newUserEntity, Roles.User);
                    if (!roleResult.Succeeded)
                    {
                        return Results.BadRequest("500|Add role failed");
                    }
                }

                return Results.Ok("200|User created");
            }
            else
            {
                return Results.BadRequest($"500|{string.Concat(result.Errors.Select(e => e.Description))}");
            }
        }
        
        //change password
        public async Task<IResult> ChangePassword(UserManager<ApplicationUser> _userManager,
            [FromBody] ChangePassword changePassword, IUser _user)
        {
            try
            {
                if (changePassword == null || string.IsNullOrEmpty(_user.Id))
                {
                    return Results.BadRequest("400| Missing or invalid user ID or change password data.");
                }

                var currentUser = await _userManager.FindByIdAsync(_user.Id);
                if (currentUser == null)
                {
                    return Results.BadRequest("400| Invalid UserId provided.");
                }

                var isOldPasswordCorrect = await _userManager.CheckPasswordAsync(currentUser, changePassword.oldPassword);
                if (!isOldPasswordCorrect)
                {
                    return Results.BadRequest("400| The old password is incorrect.");
                }

                if (!changePassword.newPassword.Equals(changePassword.comfirmedPassword))
                {
                    return Results.BadRequest("400| The new password and confirmation do not match.");
                }

                var result = await _userManager.ChangePasswordAsync(currentUser, changePassword.oldPassword, changePassword.newPassword);
                if (result.Succeeded)
                {
                    return Results.Ok("200| Password changed successfully.");
                }
                else
                {
                    var errorDescriptions = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Results.BadRequest($"500| {errorDescriptions}");
                }
            }
            catch (Exception ex)
            {
                // Log the full stack trace here if possible for more in-depth debugging.
                Console.WriteLine($"Error occurred while changing password: {ex}");
                return Results.Problem("An error occurred while changing the password.", statusCode: 500);
            }
        }

    }
}