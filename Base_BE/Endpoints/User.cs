using Base_BE.Identity;
using Base_BE.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceStack;
using System.Security.Cryptography;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Domain.Constants;
using Base_BE.Dtos;
using Base_BE.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using NetHelper.Common.Models;
using Microsoft.EntityFrameworkCore;

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
                .MapPut(ChangePassword, "/change-password")
                .MapPatch(UpdateUser, "/update-user")
                .MapPost(SendOTP, "/send-otp")
                .MapPost(VerifyOTP, "/verify-otp")
                .MapPut(ChangeEmail, "/change-email")
                .MapGet(GetCurrentUser, "/UserInfo");

            app.MapGroup(this)
                .RequireAuthorization("admin")
                .MapGet(GetAllUsers, "/get-all-users");
        }

        public async Task<IResult> RegisterUser([FromBody] RegisterForm newUser,
            UserManager<ApplicationUser> _userManager, [FromServices] EmailSender _emailSender)
        {
            // Kiểm tra Username
            if (string.IsNullOrEmpty(newUser.UserName))
            {
                return Results.BadRequest("400|Username is required");
            }

            // Kiểm tra Username chỉ chứa chữ cái và chữ số
            if (!newUser.UserName.All(char.IsLetterOrDigit))
            {
                return Results.BadRequest("400|Username can only contain letters or digits.");
            }

            // Kiểm tra xem User đã tồn tại chưa
            var user = await _userManager.FindByNameAsync(newUser.UserName);
            if (user != null)
            {
                return Results.BadRequest("501|User already exists");
            }

            // Tạo cặp khóa RSA
            using var rsa = new RSACryptoServiceProvider(512);
            var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
            var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

            // Tạo đối tượng ApplicationUser
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
                IdentityCardImage = newUser.UrlIdentityCardImage,
                PublicKey = publicKey
            };

            // Tạo mật khẩu mặc định dựa trên ngày sinh hoặc giá trị mặc định nếu không có ngày sinh
            var passwordSeed = "Abc@" + (newUser.Birthday?.ToString("ddMMyyyy") ?? "DefaultDate");
            var result = await _userManager.CreateAsync(newUserEntity, passwordSeed);

            if (result.Succeeded)
            {
                // Gán vai trò cho người dùng mới tạo
                var roleName = newUser.IsAdmin == true ? Roles.Administrator : Roles.User;
                var roleResult = await _userManager.AddToRoleAsync(newUserEntity, roleName);

                if (!roleResult.Succeeded)
                {
                    return Results.BadRequest("500|Failed to assign role to user");
                }

                // Gửi email chứa thông tin tài khoản và khóa riêng (private key)
                await _emailSender.SendEmailRegisterAsync(newUser.Email, newUser.FullName, newUser.UserName,
                    passwordSeed, privateKey);

                return Results.Ok("200|User created successfully");
            }
            else
            {
                // Trả về lỗi nếu quá trình tạo tài khoản thất bại
                return Results.BadRequest($"500|{string.Join(", ", result.Errors.Select(e => e.Description))}");
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

                var isOldPasswordCorrect =
                    await _userManager.CheckPasswordAsync(currentUser, changePassword.oldPassword);
                if (!isOldPasswordCorrect)
                {
                    return Results.BadRequest("400| The old password is incorrect.");
                }

                if (!changePassword.newPassword.Equals(changePassword.comfirmedPassword))
                {
                    return Results.BadRequest("400| The new password and confirmation do not match.");
                }

                var result = await _userManager.ChangePasswordAsync(currentUser, changePassword.oldPassword,
                    changePassword.newPassword);
                if (result.Succeeded)
                {
                    currentUser.ChangePasswordFirstTime = true;
                    _userManager.UpdateAsync(currentUser).Wait();
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

/*
C****: Update User
*/
        public async Task<IResult> UpdateUser(UserManager<ApplicationUser> _userManager,
            [FromBody] UpdateUser updateUser,
            [FromServices] IUser _user)
        {
            try
            {
                var userId = _user.Id;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.BadRequest("400|UserId is empty");
                }

                var currentUser = await _userManager.FindByIdAsync(userId);
                if (currentUser == null)
                {
                    return Results.BadRequest("400|UserId không hợp lệ !");
                }

                if (!updateUser.CellPhone.IsNullOrEmpty()) currentUser.CellPhone = updateUser.CellPhone;
                if (!updateUser.Address.IsNullOrEmpty()) currentUser.Address = updateUser.Address;
                if (!updateUser.FullName.IsNullOrEmpty()) currentUser.FullName = updateUser.FullName;
                currentUser.UpdateDate = DateTime.Now;

                var result = await _userManager.UpdateAsync(currentUser);
                if (result.Succeeded)
                {
                    return Results.Ok("200|User updated successfully");
                }
                else
                {
                    return Results.BadRequest($"500|{string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error", ex.Message);
                return Results.Problem("An error occurred while updating the user", statusCode: 500);
            }
        }

        //change email
        public async Task<IResult> ChangeEmail([FromBody] ChangeEmail changeEmail,
            [FromServices] UserManager<ApplicationUser> _userManager, IUser _user)
        {
            try
            {
                if (changeEmail == null || string.IsNullOrEmpty(_user.Id))
                {
                    return Results.BadRequest("400| Missing or invalid user ID or change password data.");
                }

                var currentUser = await _userManager.FindByIdAsync(_user.Id);
                if (currentUser == null)
                {
                    return Results.BadRequest("400| Invalid UserId provided.");
                }

                currentUser.NewEmail = changeEmail.NewEmail;
                var res = await _userManager.UpdateAsync(currentUser);
                if (res.Succeeded)
                {
                    return Results.Ok("200| Email changed successfully.");
                }

                return Results.BadRequest("400| Change email failed.");
            }
            catch (Exception ex)
            {
                // Log the full stack trace here if possible for more in-depth debugging.
                Console.WriteLine($"Error occurred while changing password: {ex}");
                return Results.Problem("An error occurred while changing the password.", statusCode: 500);
            }
        }

        public async Task<IResult> SendOTP([FromBody] SendOTPRequest request, [FromServices] OTPService _otpService,
            [FromServices] IEmailSender _emailSender, [FromServices] IUser _user)
        {
            var otp = _otpService.GenerateOTP();
            if (request.Email != null)
            {
                _otpService.SaveOTP(request.Email, otp);

                await _emailSender.SendEmailAsync(request.Email, _user.UserName!, $"Mã xác minh của bạn là: {otp}");
            }

            return Results.Ok("200|OTP sent successfully");
        }

        public async Task<IResult> VerifyOTP([FromBody] VerifyOTPRequest request, [FromServices] OTPService _otpService,
            [FromServices] UserManager<ApplicationUser> _userManager, [FromServices] IUser _user)
        {
            var isValid = _otpService.VerifyOTP(request.Email, request.OTP);

            if (isValid)
            {
                var currentUser = await _userManager.FindByIdAsync(_user.Id);
                if (currentUser != null)
                {
                    currentUser.EmailConfirmed = true;
                    await _userManager.UpdateAsync(currentUser);
                    return Results.Ok("200|Xác minh thành công.");
                }

                return Results.BadRequest("khong tim thay nguoi dung.");
            }

            return Results.BadRequest("Mã xác minh không hợp lệ hoặc đã hết hạn.");
        }

        public async Task<ResultCustomPaginate<IEnumerable<UserDto>>> GetAllUsers([FromServices] UserManager<ApplicationUser> _userManager, int page, int pageSize)
        {
            var usersQuery = _userManager.Users;

            var usersList = await usersQuery.ToListAsync();

            var usersDtoList = new List<UserDto>();

            foreach (var u in usersList)
            {

                // Tạo đối tượng UserDto
                var userDto = new UserDto
                {
                    Fullname = u.FullName,
                    UserName = u.UserName,
                    Email = u.Email,
                    CellPhone = u.PhoneNumber,
                    status = u.Status,
                    Birthday = u.Birthday,
                    Address = u.Address,
                    CreatedAt = u.CreateDate,
                };

                usersDtoList.Add(userDto);
            }

            // Sort by ActivationDate
            var sortedUsersDtoList = usersDtoList.OrderByDescending(u => u.CreatedAt).ToList();

            // Áp dụng phân trang
            var paginatedUsers = sortedUsersDtoList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var result = new ResultCustomPaginate<IEnumerable<UserDto>>
            {
                Data = paginatedUsers,
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = sortedUsersDtoList.Count,
                TotalPages = (int)Math.Ceiling((double)sortedUsersDtoList.Count / pageSize)
            };

            return result;
        }

        public async Task<IResult> GetCurrentUser([FromServices] UserManager<ApplicationUser> _userManager, IUser _user)
        {
            var currentUser = await _userManager.FindByIdAsync(_user.Id);
            if (currentUser == null)
            {
                return Results.BadRequest("400|User not found");
            }

            var userDto = new UserDto
            {
                Fullname = currentUser.FullName,
                UserName = currentUser.UserName,
                Email = currentUser.Email,
                Gender = currentUser.Gender,
                CellPhone = currentUser.PhoneNumber,
                status = currentUser.Status,
                Birthday = currentUser.Birthday,
                Address = currentUser.Address,
                CreatedAt = currentUser.CreateDate,
                Role = (await _userManager.GetRolesAsync(currentUser)).FirstOrDefault()
            };

            return Results.Ok(userDto);
        }
    }
}